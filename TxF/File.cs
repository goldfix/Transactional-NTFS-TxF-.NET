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

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using TxF.WindowsApi;


namespace TxF
{
    public class File 
    {
        public File()
        { }
        private File(IntPtr pointerToFile)
        {
            this.PointerToFile = pointerToFile;
        }

        public IntPtr PointerToFile
        { get; private set; }

        /// <summary>
        /// Find files in specific folder. This method does not search in sub folder.
        /// </summary>
        /// <param name="fileName">Path file to search. (ex: c:\tmp\Test*.*)</param>
        /// <returns></returns>
        public static string[] Find(string fileName)
        {
            Transaction t = new Transaction(true);
            return Find(fileName, t);
        }
        /// <summary>
        /// Find files in specific folder. This method does not search in sub folder.
        /// </summary>
        /// <param name="fileName">Path file to search. (ex: c:\tmp\Test*.*)</param>
        /// <param name="transaction">Transaction active.</param>
        /// <returns>Return list file match with search parameters.</returns>
        public static string[] Find(string fileName, Transaction transaction)
        {
            List<apiwindows.WIN32_FIND_DATAW> listFindFile = new List<apiwindows.WIN32_FIND_DATAW>();
            apiwindows.WIN32_FIND_DATAW find_dataw = new apiwindows.WIN32_FIND_DATAW();
            IntPtr result = IntPtr.Zero;

            try
            {
                result = apiwindows.FindFirstFileTransactedW(fileName, apiwindows.FINDEX_INFO_LEVELS.FindExInfoStandard, out find_dataw, apiwindows.FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, 0, transaction.TransactionHandle);

                if (result.ToInt32() == apiwindows.INVALID_HANDLE_VALUE)
                {
                    int errNum = Marshal.GetLastWin32Error();
                    if (errNum == 2)
                    {
                        return new string[0];
                    }
                    throw new Win32Exception(errNum);
                }

                listFindFile.Add(find_dataw);

                while (true)
                {
                    find_dataw = new apiwindows.WIN32_FIND_DATAW();
                    int err = apiwindows.FindNextFileW(result, out find_dataw);
                    if (err == 0)
                    {
                        int errNum = Marshal.GetLastWin32Error();
                        if (errNum == 18)
                        {
                            break;
                        }
                        throw new Win32Exception(errNum);
                    }
                    listFindFile.Add(find_dataw);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (result != IntPtr.Zero)
                {
                    apiwindows.FindClose(result);
                }
            }

            string[] resultFiles = new string[listFindFile.Count];
            int i = 0;
            foreach (apiwindows.WIN32_FIND_DATAW w in listFindFile)
            {
                resultFiles[i] = w.cFileName;
                i++;
            }

            return resultFiles;
        }

        /// <summary>
        /// Copy single file.
        /// </summary>
        /// <param name="sourceFileName">Source path file</param>
        /// <param name="destFileName">Destination path file</param>
        /// <param name="overwrite">If true, overwrite file if exists</param>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            Transaction t = new Transaction(true);
            Copy(sourceFileName, destFileName, overwrite, t);
        }

        /// <summary>
        /// Copy single file.
        /// </summary>
        /// <param name="sourceFileName">Source path file</param>
        /// <param name="destFileName">Destination path file</param>
        /// <param name="overwrite">If true, overwrite file if exists</param>
        /// <param name="transaction">Transaction active.</param>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite, Transaction transaction)
        {
            int pbCancel = 0;
            apiwindows.COPY_FLAGS dwCopyFlags = apiwindows.COPY_FLAGS.COPY_FILE_COPY_ND;
            if (!overwrite)
            {
                dwCopyFlags = apiwindows.COPY_FLAGS.COPY_FILE_FAIL_IF_EXISTS;
            }
            int err = apiwindows.CopyFileTransactedW(sourceFileName, destFileName, null, IntPtr.Zero, ref pbCancel, dwCopyFlags, transaction.TransactionHandle);

            if (err == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public enum CreationDisposition
        {
            CreatesNewfileAlways = apiwindows.CreationDisposition.CREATE_ALWAYS,
            CreatesNewfileIfNotExist = apiwindows.CreationDisposition.CREATE_NEW,
            OpensFileOrCreate = apiwindows.CreationDisposition.OPEN_ALWAYS,
            OpensFile = apiwindows.CreationDisposition.OPEN_EXISTING,
            OpensFileAndTruncate = apiwindows.CreationDisposition.TRUNCATE_EXISTING
        }

        /// <summary>
        /// Create a file with data
        /// </summary>
        /// <param name="fileName">Path file</param>
        /// <param name="creationDisposition">Options to creation</param>
        /// <param name="data">Stream data</param>
        /// <returns></returns>
        public static int CreateAndWriteFile(string fileName, CreationDisposition creationDisposition, byte[] data)
        {
            Transaction t = new Transaction(true);
            return CreateAndWriteFile(fileName, creationDisposition, data, t);
        }
        /// <summary>
        /// Create a file with data
        /// </summary>
        /// <param name="fileName">Path file</param>
        /// <param name="creationDisposition">Options to creation</param>
        /// <param name="data">Stream data</param>
        /// <param name="transaction">Transaction active.</param>
        /// <returns></returns>
        public static int CreateAndWriteFile(string fileName, CreationDisposition creationDisposition,byte[] data, Transaction transaction)
        {
            File f = CreateFile(fileName, creationDisposition, transaction);
            return WriteFile(f, data);
        }
        /// <summary>
        /// Create a file
        /// </summary>
        /// <param name="fileName">Path file</param>
        /// <param name="creationDisposition">Options to creation</param>
        /// <returns>Pointer to file.</returns>
        public static File CreateFile(string fileName, CreationDisposition creationDisposition)
        {
            Transaction t = new Transaction(true);
            File f = CreateFile(fileName, creationDisposition, t);
            return f;
        }
        /// <summary>
        /// Create a file
        /// </summary>
        /// <param name="fileName">Path file</param>
        /// <param name="creationDisposition">Options to creation</param>
        /// <param name="transaction">Transaction active.</param>
        /// <returns>Pointer to file.</returns>
        public static File CreateFile(string fileName, CreationDisposition creationDisposition, Transaction transaction)
        {
            IntPtr p = apiwindows.CreateFileTransactedW(fileName,
                        apiwindows.DesiredAccess.GENERIC_READ | apiwindows.DesiredAccess.GENERIC_WRITE,
                        apiwindows.ShareMode.FILE_SHARE_ND,
                        new apiwindows.LPSECURITY_ATTRIBUTES(),
                        (apiwindows.CreationDisposition)creationDisposition,
                        apiwindows.FlagsAndAttributes.FILE_ATTRIBUTE_NORMAL | apiwindows.FlagsAndAttributes.FILE_ATTRIBUTE_ARCHIVE,
                        IntPtr.Zero,
                        transaction.TransactionHandle,
                        null,
                        IntPtr.Zero
                        );

            if (p.ToInt32() == apiwindows.INVALID_HANDLE_VALUE)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            File f = new File(p);
            return f;
        }

        /// <summary>
        /// Write data into file
        /// </summary>
        /// <param name="file">Pointer to file</param>
        /// <param name="data">Stream data</param>
        /// <returns></returns>
        public static int WriteFile(File file, byte[] data)
        {
            try
            {
                int i = 0;
                int err = apiwindows.WriteFile(file.PointerToFile, data, data.Length, ref i, new apiwindows.LPOVERLAPPED());
                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                return err;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                apiwindows.CloseHandle(file.PointerToFile);
            }            
        }
        /// <summary>
        /// Read data from file
        /// </summary>
        /// <param name="file">Path file</param>
        /// <returns>Data stream</returns>
        public static byte[] ReadFile(string file)
        {
            IntPtr f = _OpenFile(file);
            return _ReadFile(f);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="file">Path file</param>
        /// <returns></returns>
        public static int Delete(string file)
        {
            Transaction t = new Transaction(true);
            return DeleteFile(file, t);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="file">Path file</param>
        /// <param name="transaction">Transaction active</param>
        /// <returns></returns>
        public static int DeleteFile(string file, Transaction transaction )
        {
            try
            {
                int err = apiwindows.DeleteFileTransactedW(file, transaction.TransactionHandle);
                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                return err;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //--
            }
        }


        [Flags]
        public enum FileAttributes
        {
            Archive = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_ARCHIVE ,
            Hidden = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_HIDDEN ,
            Normal = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL,
            Not_content_indexed = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_NOT_CONTENT_INDEXED,
            Offline = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_OFFLINE,
            Readonly = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_READONLY,
            System = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_SYSTEM,
            Temporary = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_TEMPORARY,

            //Not supported from TxF
            Compressed = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_COMPRESSED,
            Device = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_DEVICE,
            Directory = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY,
            Encrypted = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_ENCRYPTED,
            Reparse_point = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_REPARSE_POINT,
            Sparse_file = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_SPARSE_FILE,
            Virtual = apiwindows.FILE_ATTRIBUTE.FILE_ATTRIBUTE_VIRTUAL 
        }

        /// <summary>
        /// Set attributes file
        /// </summary>
        /// <param name="pathFile">Path file</param>
        /// <param name="fileAttributes"></param>
        public static void SetAttributes(string pathFile, FileAttributes fileAttributes)
        {
            Transaction t = new Transaction(true);
            SetAttributes(pathFile, fileAttributes, t);
        }

        /// <summary>
        /// Set attributes file
        /// </summary>
        /// <param name="pathFile">Path file</param>
        /// <param name="fileAttributes"></param>
        /// <param name="transaction">Transaction active</param>
        public static void SetAttributes(string pathFile, FileAttributes fileAttributes, Transaction transaction)
        {
            if (
                ((fileAttributes & FileAttributes.Compressed) == FileAttributes.Compressed)
                ||
                ((fileAttributes & FileAttributes.Device) == FileAttributes.Device)
                ||
                ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                ||
                ((fileAttributes & FileAttributes.Reparse_point) == FileAttributes.Reparse_point)
                ||
                ((fileAttributes & FileAttributes.Sparse_file) == FileAttributes.Sparse_file)
                )
            {
                throw new FileTransactedException("FileAttributes not supported.");
            }

            int err = apiwindows.SetFileAttributesTransactedW(pathFile, (apiwindows.FILE_ATTRIBUTE)fileAttributes, transaction.TransactionHandle);
            if (err == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return ;
        }

        /// <summary>
        /// Get attributes file
        /// </summary>
        /// <param name="pathFile">Path file</param>
        /// <returns></returns>
        public static FileAttributes GetAttributes(string pathFile)
        {
            Transaction t = new Transaction(true);
            return GetAttributes(pathFile, t);
        }

        /// <summary>
        /// Get attributes file
        /// </summary>
        /// <param name="pathFile">Path file</param>
        /// <param name="transaction">Transaction active</param>
        /// <returns></returns>
        public static FileAttributes GetAttributes(string pathFile, Transaction transaction)
        {
            apiwindows.WIN32_FILE_ATTRIBUTE_DATA win32_file_attribute_data = new apiwindows.WIN32_FILE_ATTRIBUTE_DATA();
            int err = apiwindows.GetFileAttributesTransactedW(pathFile, apiwindows.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out win32_file_attribute_data, transaction.TransactionHandle);
            if (err == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return (FileAttributes)win32_file_attribute_data.dwFileAttributes;
        }

        /// <summary>
        /// Create a new HardLink File
        /// </summary>
        /// <param name="pathHardLinkFile">Path new HardLink File</param>
        /// <param name="pathFile">Original path file</param>
        public static void CreateHardLink(string pathHardLinkFile, string pathFile)
        {
            Transaction t = new Transaction(true);
            CreateHardLink(pathHardLinkFile, pathFile, t);
        }
        
        /// <summary>
        /// Create a new HardLink File
        /// </summary>
        /// <param name="pathHardLinkFile">Path new HardLink File</param>
        /// <param name="pathFile">Original path file</param>
        /// <param name="transaction">Transaction active</param>
        public static void CreateHardLink(string pathHardLinkFile, string pathFile, Transaction transaction)
        {
            try
            {
                int err = apiwindows.CreateHardLinkTransactedW(pathHardLinkFile, pathFile, new apiwindows.LPSECURITY_ATTRIBUTES(), transaction.TransactionHandle);
                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //--
            }
        }

        /// <summary>
        /// Create a new SymbolicLink File
        /// </summary>
        /// <param name="pathSymbolicLinkFile">Path new SymbolicLink File</param>
        /// <param name="pathFile">Original path file</param>
        public static void CreateSymbolicLink(string pathSymbolicLinkFile, string pathFile)
        {
            Transaction t = new Transaction(true);
            CreateSymbolicLink(pathSymbolicLinkFile, pathFile, t);
        }

        /// <summary>
        /// Create a new SymbolicLink File
        /// </summary>
        /// <param name="pathSymbolicLinkFile">Path new SymbolicLink File</param>
        /// <param name="pathFile">Original path file</param>
        /// <param name="transaction">Transaction active</param>
        public static void CreateSymbolicLink(string pathSymbolicLinkFile, string pathFile, Transaction transaction)
        {
            try
            {
                int err = apiwindows.CreateSymbolicLinkTransactedW(pathSymbolicLinkFile, pathFile, apiwindows.TypeSymbolicLink.File ,transaction.TransactionHandle);
                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //--
            }
        }


        private static byte[] _ReadFile(IntPtr file)
        {
            try
            {
                int i = 0;
                int sizeFile = _SizeFile(file);
                byte[] result = new byte[sizeFile];
                int err = apiwindows.ReadFile(file, result,  result.Length, ref  i, new apiwindows.LPOVERLAPPED());
                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                apiwindows.CloseHandle(file);
            }            
        }

        private static IntPtr _OpenFile(string file)
        {
            apiwindows.OFSTRUCT of = new apiwindows.OFSTRUCT();
            IntPtr result = apiwindows.OpenFile(new StringBuilder(file), ref of, apiwindows.Style.OF_READ);
            if (result.ToInt32() == (int)apiwindows.HFILE_ERROR)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return result;
        }

        private static int _SizeFile(IntPtr file)
        {
            int i = 0;
            int result = apiwindows.GetFileSize(file, ref i);
            if (result == apiwindows.INVALID_FILE_SIZE)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return result;
        }
    }
}
