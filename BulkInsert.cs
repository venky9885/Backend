https://medium.com/@gabrieletronchin/c-net-8-sql-bulk-insert-dapper-vs-bulkcopy-vs-table-value-parameters-665d9db39477
// Dapper Bulk Insert
// Dapper is a lightweight ORM for .NET, It is known for its simplicity and performance.

  public async Task DapperBulkInsert()
    {
        using var connection = new SqlConnection(ConnectionString);
        var list2Insert = new List<Test>();

        for (int i = 0; i < this.NumberOfRow; i++)
        {
            list2Insert.Add(new Test(System.Guid.NewGuid(), $"Test Desc {i}"));
        }

        await connection.ExecuteAsync("INSERT INTO DapperBulkInsert (id, Description) VALUES (@id, @description)", list2Insert);
    }

// Bulk Copy
// Bulk Copy is a more efficient method for large datasets. It leverages the SqlBulkCopy class provided by .NET, which is specifically designed for high-performance bulk data operations.

// Here’s an example of how to use SqlBulkCopy:

public async Task BulkCopyInsert()
 {
     using var connection = new SqlConnection(ConnectionString);
     await connection.OpenAsync();

     using var transaction = connection.BeginTransaction();
     using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
     {
         bulkCopy.DestinationTableName = "DapperBulkCopyInsert";

         var table = new DataTable();
         table.Columns.Add("id", typeof(System.Guid));
         table.Columns.Add("Description", typeof(string));

         for (int i = 0; i < this.NumberOfRow; i++)
         {
             table.Rows.Add( System.Guid.NewGuid(), $"Test Desc {i}");
         }

         await bulkCopy.WriteToServerAsync(table);
     }

     transaction.Commit();
 }

// Table-Value Parameters
// With the terms table value parameter (TVPs) I reference to the usage of a store procedure that allow you to pass an entire table as
// a parameter to a stored procedure. Which can then insert the data in a single operation.


/* Create a table type. */
/*CREATE TYPE SampleTableType 
   AS TABLE
      ( [id] UNIQUEIDENTIFIER
      , [Description] NVARCHAR(50) );
GO*/
/* Create a procedure to receive data for the table-valued parameter. */
/*CREATE PROCEDURE dbo. usp_SampleTableInsert
   @TVP SampleTableType READONLY
      AS
      SET NOCOUNT ON
      INSERT INTO [DapperTableInsert]
      SELECT *
      FROM @TVP;
GO
*/
    public async Task BulkInsertTable() {

        var table = new DataTable();
        table.Columns.Add("id", typeof(System.Guid));
        table.Columns.Add("Description", typeof(string));

        for (int i = 0; i < this.NumberOfRow; i++)
        {
            table.Rows.Add(System.Guid.NewGuid(), $"Test Desc {i}");
        }

        using var connection = new SqlConnection(ConnectionString);

        await connection.ExecuteAsync("usp_SampleTableInsert", new { TVP = table.AsTableValuedParameter("SampleTableType") }, commandType: CommandType.StoredProcedure);

    }



// Conclusion
// Dapper Bulk Insert: Simple and suitable for small to medium-sized datasets. Easy to use but may not be optimal for large volumes of data.
// Bulk Copy: Highly efficient for large datasets. Best for high-performance bulk inserts with minimal overhead.
// Table-Value Parameters: Combines flexibility and efficiency. Suitable for large datasets and complex operations.
