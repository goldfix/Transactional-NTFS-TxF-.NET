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
    public class Directory
    {
        public Directory()
        {

        }

        internal Directory(IntPtr pointerToDirectory)
        {
            this.PointerToDirectory = pointerToDirectory;
        }

        public IntPtr PointerToDirectory
        { get; private set; }

        public static Directory GetDirectory(string pathDirectory)
        {
            Transaction t = new Transaction(true);
            return GetDirectory(pathDirectory, t);
        }

        public static Directory GetDirectory(string pathDirectory, Transaction transaction)
        {
            IntPtr p = apiwindows.CreateFileTransactedW(pathDirectory,
                                        apiwindows.DesiredAccess.GENERIC_READ,
                                        apiwindows.ShareMode.FILE_SHARE_ND,
                                        new apiwindows.LPSECURITY_ATTRIBUTES(),
                                        apiwindows.CreationDisposition.OPEN_EXISTING,
                                        apiwindows.FlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS,
                                        IntPtr.Zero,
                                        transaction.TransactionHandle,
                                        null,
                                        IntPtr.Zero);

            if (p.ToInt32() == apiwindows.INVALID_HANDLE_VALUE)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            Directory d = new Directory(p);
            return d;
        }

        /// <summary>
        /// Create a single directory
        /// </summary>
        /// <param name="path">Path new directory</param>
        /// <param name="overwrite">If this parameter is true and the directory exists, the method don't generate exception</param>
        public static void CreateDirectory(string path, bool overwrite)
        {
            Transaction t = new Transaction(true);
            CreateDirectory(path,overwrite,  t);
        }
        /// <summary>
        /// Create a single directory
        /// </summary>
        /// <param name="path">Path new directory</param>
        /// <param name="overwrite">If this parameter is true and the directory exists, the method don't generate exception</param>
        /// <param name="transaction">Transaction active</param>
        public static void CreateDirectory(string path, bool overwrite, Transaction transaction)
        {
            if (overwrite && System.IO.Directory.Exists(path))
            {
                return ;
            }

            int err = apiwindows.CreateDirectoryTransactedW(null, path, new apiwindows.LPSECURITY_ATTRIBUTES(), transaction.TransactionHandle);

            if (err == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Delete directory
        /// </summary>
        /// <param name="path">Path directory to delete</param>
        /// <param name="checkExist">If this parameter is false and the directory doesn't exists, the method don't generate exception</param>
        public static void Delete(string path, bool checkExist)
        {
            Transaction t = new Transaction(true);
            Delete(path,checkExist, t);
        }

        /// <summary>
        /// Delete directory
        /// </summary>
        /// <param name="path">Path directory to delete</param>
        /// <param name="checkExist">If this parameter is false and the directory doesn't exists, the method don't generate exception</param>
        /// <param name="transaction">Transaction active</param>
        public static void Delete(string path, bool checkExist, Transaction transaction)
        {
            if ((!checkExist) && (!System.IO.Directory.Exists(path)))
            {
                return;
            }

            int err = apiwindows.RemoveDirectoryTransactedW(path, transaction.TransactionHandle);

            if (err == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }


        /// <summary>
        /// Create a new SymbolicLink Directory
        /// </summary>
        /// <param name="pathSymbolicLinkDirectory">Path new SymbolicLink Directory</param>
        /// <param name="pathFile">Original path directory</param>
        public static void CreateSymbolicLink(string pathSymbolicLinkDirectory, string pathDirector)
        {
            Transaction t = new Transaction(true);
            CreateSymbolicLink(pathSymbolicLinkDirectory, pathDirector, t);
        }

        /// <summary>
        /// Create a new SymbolicLink Directory
        /// </summary>
        /// <param name="pathSymbolicLinkDirectory">Path new SymbolicLink Directory</param>
        /// <param name="pathDirector">Original path directory</param>
        /// <param name="transaction">Transaction active</param>
        public static void CreateSymbolicLink(string pathSymbolicLinkDirectory, string pathDirector, Transaction transaction)
        {
            try
            {
                int err = apiwindows.CreateSymbolicLinkTransactedW(pathSymbolicLinkDirectory, pathDirector, apiwindows.TypeSymbolicLink.SYMBOLIC_LINK_FLAG_DIRECTORY, transaction.TransactionHandle);
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
    }
}
