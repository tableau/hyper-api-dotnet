# Hyper API

## Example usage

Create a new extract file and write some data into it:
```c#
using (var hyper = new HyperProcess(Telemetry.SendUsageDataToTableau, "myapp"))
{
    using (var connection = new Connection(hyper.Endpoint, "mydb.hyper", CreateMode.Create))
    {
        connection.Catalog.CreateSchema("Extract");

        var tableSchema = new TableDefinition(new TableName("Extract", "Extract"))
            .AddColumn("a", SqlType.Text(), "en_us_ci", Nullability.NotNullable)
            .AddColumn("b", SqlType.Bool());

        connection.Catalog.CreateTable(tableSchema);

        using (var inserter = new Inserter(connection, tableSchema))
        {
            inserter.Add("x");
            inserter.Add(true);
            inserter.EndRow();
            inserter.Add("y");
            inserter.AddNull();
            inserter.EndRow();
            inserter.Execute();
        }
    }
}
```

Connect to an extract file and append data to a table:
```c#
using (var connection = new Connection(hyper.Endpoint, "mydb.hyper"))
{
    var tableName = new TableName("Extract", "Extract");
    using (var inserter = new Inserter(connection, tableName))
    {
        inserter.Add("z");
        inserter.Add(false);
        inserter.EndRow();
        inserter.Execute();
    }
}
```

Connect to an extract file and run queries and commands in it:
```c#
using (var connection = new Connection(hyper.Endpoint, "mydb.hyper"))
{
    var tableName = new TableName("Extract", "Extract");

    using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName}"))
    {
        while (result.NextRow())
        {
            string v1 = result.GetString(0);
            bool? v2 = result.IsNull(1) ? null : (bool?)result.GetBool(1);
            // ...
        }
    }

    int max = connection.ExecuteScalarQuery<int>($"SELECT MAX(b) FROM {tableName}");

    // Delete the table
    connection.ExecuteCommand($"DROP TABLE {tableName}");
}
```
