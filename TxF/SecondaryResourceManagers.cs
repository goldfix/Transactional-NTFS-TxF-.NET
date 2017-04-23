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
using TxF.WindowsApi;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TxF
{
    public class SecondaryResourceManagers
    {
        public SecondaryResourceManagers()
        {
            throw new NotImplementedException();

            if (!Transaction.IsSupported)
            {
                new FileTransactedException("TxF Transactional NTFS not supported in this version operating system.");
            }
        }

        private IntPtr _GetPointDirectory(string pathDirectory)
        {
            IntPtr p = apiwindows.CreateFileW(pathDirectory,
                            apiwindows.DesiredAccess.GENERIC_WRITE,
                            apiwindows.ShareMode.FILE_SHARE_WRITE,
                            new apiwindows.LPSECURITY_ATTRIBUTES(),
                            apiwindows.CreationDisposition.OPEN_EXISTING,
                            apiwindows.FlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS,
                            IntPtr.Zero);

            if (p.ToInt32() == apiwindows.INVALID_HANDLE_VALUE)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return p;
        }

        public void Create(string pathDirectory)
        {
            try
            {
                IntPtr directory = _GetPointDirectory(pathDirectory);
                int lpBytesReturned = 0;
                apiwindows.LPOVERLAPPED ol = new apiwindows.LPOVERLAPPED();
                int err = apiwindows.DeviceIoControl(directory, apiwindows.FSCTL_TXFS_CREATE_SECONDARY_RM, IntPtr.Zero, 0, IntPtr.Zero, 0, ref lpBytesReturned, ol);
                
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
                
            }  
        }

        public void Start(string pathDirectory)
        {
            try
            {
                IntPtr directory = _GetPointDirectory(pathDirectory);
                int lpBytesReturned = 0;
                apiwindows.LPOVERLAPPED ol = new apiwindows.LPOVERLAPPED();
                int err = apiwindows.DeviceIoControl(directory, apiwindows.FSCTL_TXFS_START_RM, new apiwindows.TXFS_START_RM_INFORMATION() , 0, IntPtr.Zero, 0, ref lpBytesReturned, ol);

                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                err = apiwindows.DeviceIoControl(directory, apiwindows.FSCTL_TXFS_ROLLFORWARD_REDO,  new apiwindows.TXFS_ROLLFORWARD_REDO_INFORMATION(), 0, IntPtr.Zero, 0, ref lpBytesReturned, ol);
                if (err == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                err = apiwindows.DeviceIoControl(directory, apiwindows.FSCTL_TXFS_ROLLFORWARD_UNDO, IntPtr.Zero, 0, IntPtr.Zero, 0, ref lpBytesReturned, ol);
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

            }
        }

        public void Close(string pathDirectory)
        {
            try
            {
                IntPtr directory = _GetPointDirectory(pathDirectory);
                int lpBytesReturned = 0;
                apiwindows.LPOVERLAPPED ol = new apiwindows.LPOVERLAPPED();
                int err = apiwindows.DeviceIoControl(directory, apiwindows.FSCTL_TXFS_SHUTDOWN_RM, IntPtr.Zero, 0, IntPtr.Zero, 0, ref lpBytesReturned, ol);

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

            }
        }
    }
}
