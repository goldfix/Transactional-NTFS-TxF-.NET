/*
The MIT License (MIT)
Copyright (c) 2017 pietro partescano

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using TxF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Transactions;

namespace TxF.Test
{
    
    
    /// <summary>
    ///This is a test class for FileTest and is intended
    ///to contain all FileTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Copy
        ///</summary>
        [TestMethod()]
        public void CopyTest()
        {
            string sourceFileName = @"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.source";
            string destFileName = @"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.dest";
            
            bool overwrite = true;

            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                File.Copy(sourceFileName, destFileName, overwrite, transaction);
                transaction.Commit();
            }
        }

        [TestMethod()]
        public void LoadTest()
        {
            
        }

        /// <summary>
        ///A test for Copy
        ///</summary>
        [TestMethod()]
        public void CopyTestWithDb()
        {
            string sourceFileName = @"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.source";
            string destFileName = @"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.dest";
            System.Data.SqlServerCe.SqlCeConnection cnDb = new System.Data.SqlServerCe.SqlCeConnection(@"Data Source=C:\tmp\System.IO.Transactions.TxF\System.IO.Transactions.TxF.Test\DatabaseTest.sdf");
            System.Data.SqlServerCe.SqlCeCommand cmDb = new System.Data.SqlServerCe.SqlCeCommand("INSERT INTO TestTbl (Col_1) VALUES (GETDATE());", cnDb);

            bool overwrite = true;


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
        }


        /// <summary>
        ///A test for Copy
        ///</summary>
        [TestMethod()]
        public void CopyTest2()
        {
            string sourceFileName = @"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.source";
            string destFileName = @"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.dest";

            bool overwrite = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                File.Copy(sourceFileName, destFileName, overwrite);
                ts.Complete();
            }
        }


        /// <summary>
        ///A test for CreateFileTransacted
        ///</summary>
        [TestMethod()]
        public void CreateFileTransactedTest()
        {
            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                File actual = File.CreateFile("c:\\tmp\\out.txt",File.CreationDisposition.CreatesNewfileIfNotExist  , transaction);
                transaction.Commit();
            }
        }

        /// <summary>
        ///A test for WriteFile
        ///</summary>
        [TestMethod()]
        public void WriteFileTest()
        {
            byte[] data = System.IO.File.ReadAllBytes("c:\\tmp\\test.rar");

            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                try
                {
                    File actual = File.CreateFile("c:\\tmp\\out.bin", File.CreationDisposition.CreatesNewfileAlways, transaction);
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
        }

        /// <summary>
        ///A test for WriteFile
        ///</summary>
        [TestMethod()]
        public void WriteFileTest2()
        {
            byte[] data = System.IO.File.ReadAllBytes(@"c:\tmp\System.IO.Transactions.TxF\TestFiles\TestFile.zip");

            TxF.Transaction transaction = new TxF.Transaction(false);

            try
            {
                File actual = File.CreateFile("c:\\tmp\\out.bin", File.CreationDisposition.CreatesNewfileAlways, transaction);
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

        }


        /// <summary>
        ///A test for ReadFile
        ///</summary>
        [TestMethod()]
        public void ReadFileTest()
        {
            string file = "c:\\tmp\\Test.rar";
            
            byte[] actual;
            actual = File.ReadFile(file);

            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                File f = File.CreateFile("c:\\tmp\\out.bin", File.CreationDisposition.CreatesNewfileAlways, transaction);
                int result = File.WriteFile(f, actual);
                transaction.Commit();
            }   

        }

        /// <summary>
        ///A test for DeleteFile
        ///</summary>
        [TestMethod()]
        public void DeleteFileTest()
        {
            string file = "c:\\tmp\\Test.rar";

            byte[] actual;
            actual = File.ReadFile(file);

            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                File f = File.CreateFile("c:\\tmp\\out.bin", File.CreationDisposition.CreatesNewfileAlways, transaction);
                int result = File.WriteFile(f, actual);
                transaction.Commit();
            }

            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                File.DeleteFile("c:\\tmp\\out.bin", transaction);
                transaction.Commit();
            }   
        }


        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindTest()
        {
            string fileName = @"c:\Progetti\WIN\System.IO.Transactions.TxF\TestFiles\TestFile1.*";
            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                string[] actual;
                actual = File.Find(fileName, transaction);
            }
        }



        /// <summary>
        ///A test for GetAttributes
        ///</summary>
        [TestMethod()]
        public void GetAttributesTest()
        {
            System.IO.FileAttributes zz = System.IO.File.GetAttributes(@"c:\Progetti\WIN\System.IO.Transactions.TxF\TestFiles\TestFile.zip");

            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                string pathFile = @"c:\Progetti\WIN\System.IO.Transactions.TxF\TestFiles\TestFile.zip";
                TxF.File.FileAttributes result = File.GetAttributes(pathFile, transaction);
            }
        }

        /// <summary>
        ///A test for SetAttributes
        ///</summary>
        [TestMethod()]
        public void SetAttributesTest()
        {
            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                string pathFile = @"c:\Progetti\WIN\System.IO.Transactions.TxF\TestFiles\TestFile.zip";
                File.SetAttributes(pathFile, TxF.File.FileAttributes.Archive| File.FileAttributes.Compressed, transaction);
                transaction.Commit();
            }
        }

        /// <summary>
        ///A test for CreateHardLink
        ///</summary>
        [TestMethod()]
        public void CreateHardLinkTest()
        {
            string pathHardLinkFile = @"c:\tmp\TestFile_HL.zip";
            string pathFile = @"c:\Progetti\WIN\System.IO.Transactions.TxF\TestFiles\TestFile.zip";
            using (TxF.Transaction transaction = new TxF.Transaction(false))
            {
                File.CreateHardLink(pathHardLinkFile, pathFile, transaction);
                transaction.Commit();
            }
        }

        /// <summary>
        ///A test for CreateSymbolicLink
        ///</summary>
        [TestMethod()]
        public void CreateSymbolicLinkTest()
        {
            string pathSymbolicLinkFile = @"c:\tmp\TestFile_SL.zip";
            string pathFile = @"c:\Progetti\WIN\System.IO.Transactions.TxF\TestFiles\TestFile.zip";
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                File.CreateSymbolicLink(pathSymbolicLinkFile, pathFile);
                ts.Complete();
            }
        }
    }
}
