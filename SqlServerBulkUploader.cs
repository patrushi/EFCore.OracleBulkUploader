using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using EFCore;

namespace EFCore.SqlServerBulkUploader
{
    /// <summary>SQLServerBulkUploader</summary>
    public class SqlServerBulkUploader: BulkUploader
    {
        /// <summary>BulkInsert</summary>
        public static void Insert<T>(DbContext dbContext, List<T> list, int packageSize = PACKAGE_SIZE)
            where T : class
        {
            var model = dbContext.Model.GetEntityTypes().Where(e => e.ClrType == typeof(T)).Single();
            var tableName = model.Relational().TableName;
            var columnNames = model.GetProperties().Select(e => e.Relational().ColumnName).ToList();

            string query = $"INSERT INTO {tableName} ({string.Join(",", columnNames.Select(e => $"{e}"))}) VALUES ({string.Join(",", columnNames.Select(e => $"&{e}"))})";
            
            var packageCnt = (int)Math.Ceiling((decimal)(list.Count / (decimal)packageSize));

            for (var i = 0; i < packageCnt; i++)
            {
                var packageList = list.Skip(i*packageSize).Take(packageSize).ToList();

                using (var command = (SqlCommand)dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    //command.BindByName = true;
                    //command.ArrayBindCount = packageList.Count;

                    foreach (var c in model.GetProperties())
                    {
                        var item = Expression.Parameter(typeof(T), "item");
                        var prop = Expression.Convert(Expression.Property(item, c.PropertyInfo.Name), typeof(object));
                        var f = Expression.Lambda<Func<T, object>>(prop, item).Compile();

                       //TBD>command.Parameters.Add($"&{c.Relational().ColumnName}", GetDbType(c.ClrType), packageList.Select(e => f(e)).ToArray(), ParameterDirection.Input);
                    }

                    var result = command.ExecuteNonQuery();
                }
            }
        }

        private static SqlDbType GetDbType(Type type)
        {
            if (type == typeof(string)) return SqlDbType.VarChar;
            if (type == typeof(short) || type == typeof(Nullable<short>)) return SqlDbType.SmallInt;
            if (type == typeof(int) || type == typeof(Nullable<int>)) return SqlDbType.Int;
            if (type == typeof(long) || type == typeof(Nullable<long>)) return SqlDbType.BigInt;
            if (type == typeof(decimal) || type == typeof(Nullable<decimal>)) return SqlDbType.Decimal;
            if (type == typeof(DateTime) || type == typeof(Nullable<DateTime>)) return SqlDbType.DateTime;
            if (type == typeof(bool) || type == typeof(Nullable<bool>)) return SqlDbType.Bit;

            throw new ArgumentException(type.ToString());
        }
    }
}