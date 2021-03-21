using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EFCore
{
    /// <summary>BulkUploader</summary>
    public abstract class BulkUploader
    {
        /// <summary>Default Package size</summary>
        public const int PACKAGE_SIZE = 100000;

    }
}
