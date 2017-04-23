### Transaction Scope ###

Example with *transaction scope*:

```
using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
{
  File.Copy(sourceFileName, destFileName, overwrite);
  ts.Complete();
}
```

### Transaction (*with using*) ###

Example with transaction:

```
using (System.IO.Transactions.Transaction transaction = new System.IO.Transactions.Transaction())
{
    try
    {
        IntPtr actual = File.CreateFile("c:\\tmp\\out.bin", File.CreationDisposition.CreatesNewfileAlways, transaction);
        int result = File.WriteFile(actual, data);
        transaction.Commit();
    }
    catch (FileTransactedException)
    {
        transaction.Rollback();
        throw;
    }
    finally
    { }
}  
```

### Transaction ###

Example with transaction:

```
System.IO.Transactions.Transaction transaction = new System.IO.Transactions.Transaction();

try
{
    IntPtr actual = File.CreateFile("c:\\tmp\\out.bin", File.CreationDisposition.CreatesNewfileAlways, transaction);
    int result = File.WriteFile(actual, data);
    transaction.Commit();
}
catch (FileTransactedException)
{
    transaction.Rollback();
    throw;
}
finally
{
    transaction.Close();
}
```

### Transaction with DB ###

Example with transaction scope and database:

```
using (cnDb)
{
    cnDb.Open();
    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
    {
        try
        {
            File.Copy(sourceFileName, destFileName, overwrite);
            cmDb.ExecuteNonQuery();
            ts.Complete();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        { }
    }
}
```
