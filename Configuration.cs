using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EFCore
{
    /// <summary>BulkUploader</summary>
    public abstract class BulkUploader
    {
        /// <summary>Default Package size</summary>
        public const int PACKAGE_SIZE = 4000;
        /// <summary>Identity used for primary key column</summary>
        public static bool IdentityUse { get; set; }
        /// <summary>Identity column name</summary>
        public static string IdentityColumnName { get; set; }
    }
}
