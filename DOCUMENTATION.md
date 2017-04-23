!! Transaction Scope

* Example with *transaction scope*:

```
using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
{
  File.Copy(sourceFileName, destFileName, overwrite);
  ts.Complete();
}
```

!! Transaction (with using)

* Example with transaction:
{code:c#}
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
{code:c#}

!! Transaction

* Example with transaction:
{code:c#}
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
{code:c#}

!! Transaction with DB

* Example with transaction scope and database:
{code:c#}
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
{code:c#}