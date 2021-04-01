using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EFCore.OracleBulkUploader
{
    /// <summary>OracleBulkUploader</summary>
    public class OracleBulkUploader : BulkUploader
    {
        /// <summary>BulkInsert</summary>
        public static void Insert<T>(DbContext dbContext, List<T> list, int packageSize = PACKAGE_SIZE)
            where T : class
        {
            var model = dbContext.Model.GetEntityTypes().Where(e => e.ClrType == typeof(T)).Single();
            var tableName = model.Relational().TableName;
            var modelProperties = BulkUploader.IdentityUse ? model.GetProperties().Where(e => e.Relational().ColumnName != BulkUploader.IdentityColumnName) : model.GetProperties();
            var columnNames = modelProperties.Select(e => e.Relational().ColumnName).ToList();

            string query = $"INSERT INTO {tableName} ({string.Join(",", columnNames.Select(e => $"\"{e}\""))}) VALUES ({string.Join(",", columnNames.Select(e => $":b_{e}"))})";
            
            var packageCnt = (int)Math.Ceiling((decimal)(list.Count / (decimal)packageSize));

            for (var i = 0; i < packageCnt; i++)
            {
                var packageList = list.Skip(i*packageSize).Take(packageSize).ToList();

                using (var command = (OracleCommand)dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.BindByName = true;
                    command.ArrayBindCount = packageList.Count;

                    foreach (var c in model.GetProperties())
                    {
                        var item = Expression.Parameter(typeof(T), "item");
                        var prop = Expression.Convert(Expression.Property(item, c.PropertyInfo.Name), typeof(object));
                        var f = Expression.Lambda<Func<T, object>>(prop, item).Compile();

                        command.Parameters.Add($":b_{c.Relational().ColumnName}", GetDbType(c.ClrType), packageList.Select(e => f(e)).ToArray(), ParameterDirection.Input);
                    }

                    var result = command.ExecuteNonQuery();
                }
            }
        }

        private static OracleDbType GetDbType(Type type)
        {
            if (type == typeof(string)) return OracleDbType.Varchar2;
            if (type == typeof(short) || type == typeof(Nullable<short>)) return OracleDbType.Int16;
            if (type == typeof(int) || type == typeof(Nullable<int>)) return OracleDbType.Int32;
            if (type == typeof(long) || type == typeof(Nullable<long>)) return OracleDbType.Int64;
            if (type == typeof(decimal) || type == typeof(Nullable<decimal>)) return OracleDbType.Decimal;
            if (type == typeof(DateTime) || type == typeof(Nullable<DateTime>)) return OracleDbType.Date;
            if (type == typeof(bool) || type == typeof(Nullable<bool>)) return OracleDbType.Int16;
            if (type == typeof(TimeSpan) || type == typeof(Nullable<TimeSpan>)) return OracleDbType.IntervalDS;

            throw new ArgumentException(type.ToString());
        }
    }
}