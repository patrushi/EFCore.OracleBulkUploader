# EFCore.OracleBulkUploader

BulkInsert in Oracle/SQLServer from list of efcore entities.

```
public class User
{
    [Key]
    public long Id { get;set; }
    public string Name { get; set; }
}
```

## Oracle
```
var list = new List<User>();
...
OracleBulkUploader.Insert(DbContext, list);
```
## SQLServer
```
var list = new List<User>();
...
OracleBulkUploader.Insert(DbContext, list);
```

# Links
* Nuget - https://www.nuget.org/packages/EFCore.OracleBulkUploader/
