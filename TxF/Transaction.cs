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
    public class Transaction:IDisposable
    {
        public Transaction()
        {
            if (!Transaction.IsSupported)
            {
                new FileTransactedException("TxF Transactional NTFS not supported in this version operating system.");
            }

            this.TransactionHandle = IntPtr.Zero;
            if (System.Transactions.Transaction.Current != null)
            {
                this.TransactionHandle = _GetPtrFromDtc();
            }
            else
            {
                Create();
            }
        }

        public Transaction(bool useTransactionScope)
        {
            if (!Transaction.IsSupported)
            {
                new FileTransactedException("TxF Transactional NTFS not supported in this version operating system.");
            }

            this.TransactionHandle = IntPtr.Zero;
            if (useTransactionScope)
            {
                this.TransactionHandle = _GetPtrFromDtc();
            }
            else
            {                
                Create();
            }
        }

        public static bool IsSupported
        {
            get 
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            private set { }
        }

        public IntPtr TransactionHandle { get; private set; }

        private IntPtr _GetPtrFromDtc()
        {
            if (System.Transactions.Transaction.Current == null)
            {
                throw new FileTransactedException("TransactionScope not initialized.");
            }

            apiwindows.IKernelTransaction dtcT = (apiwindows.IKernelTransaction)System.Transactions.TransactionInterop.GetDtcTransaction(System.Transactions.Transaction.Current);
            IntPtr result = IntPtr.Zero;
            dtcT.GetHandle(out result);
            return result;
        }

        private IntPtr Create()
        {
            apiwindows.LPSECURITY_ATTRIBUTES lpTransactionAttributes = new apiwindows.LPSECURITY_ATTRIBUTES();
            apiwindows.LPGUID UOW = new apiwindows.LPGUID();
            UOW.Value = IntPtr.Zero;
            int CreateOptions = 0;
            int IsolationLevel = 0;
            int IsolationFlags = 0;
            int Timeout = 0;
            StringBuilder Description = new StringBuilder("ND");
            IntPtr transactionHandle = apiwindows.CreateTransaction(lpTransactionAttributes, UOW, CreateOptions, IsolationLevel, IsolationFlags, Timeout, Description);

            if (transactionHandle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            this.TransactionHandle = transactionHandle;
            return transactionHandle;
        }

        public int Commit()
        { 
            int result = apiwindows.CommitTransaction(this.TransactionHandle);

            if (result == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return result;
        }

        public int Rollback()
        {
            int result = apiwindows.RollbackTransaction(this.TransactionHandle);

            if (result == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return result;
        }

        public int Close()
        {
            int result = apiwindows.CloseHandle(this.TransactionHandle);

            if (result == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return result;
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
