using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    internal static class ThreadIO
    {
        public static BackgroundProcessingDisposer BeginBackgroundProcessing(Boolean process = false)
        {
            ChangeBackgroundProcessing(process, true);
            return new BackgroundProcessingDisposer(process);
        }

        public static void EndBackgroundProcessing(Boolean process = false)
        {
            ChangeBackgroundProcessing(process, false);
        }

        private static void ChangeBackgroundProcessing(Boolean process, Boolean start)
        {
            Boolean ok = process
               ? SetPriorityClass(GetCurrentWin32ProcessHandle(),
                     start ? ProcessBackgroundMode.Start : ProcessBackgroundMode.End)
               : SetThreadPriority(GetCurrentWin32ThreadHandle(),
                     start ? ThreadBackgroundgMode.Start : ThreadBackgroundgMode.End);
            if (!ok) throw new Win32Exception();
        }

        // This struct lets C#'s using statement end the background processing mode
        public struct BackgroundProcessingDisposer : IDisposable
        {
            private readonly Boolean m_process;
            public BackgroundProcessingDisposer(Boolean process) { m_process = process; }
            public void Dispose() { EndBackgroundProcessing(m_process); }
        }


        // See Win32’s THREAD_MODE_BACKGROUND_BEGIN and THREAD_MODE_BACKGROUND_END
        private enum ThreadBackgroundgMode { Start = 0x10000, End = 0x20000 }

        // See Win32’s PROCESS_MODE_BACKGROUND_BEGIN and PROCESS_MODE_BACKGROUND_END   
        private enum ProcessBackgroundMode { Start = 0x100000, End = 0x200000 }

        [DllImport("Kernel32", EntryPoint = "GetCurrentProcess", ExactSpelling = true)]
        private static extern SafeWaitHandle GetCurrentWin32ProcessHandle();

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean SetPriorityClass(SafeWaitHandle hprocess, ProcessBackgroundMode mode);


        [DllImport("Kernel32", EntryPoint = "GetCurrentThread", ExactSpelling = true)]
        private static extern SafeWaitHandle GetCurrentWin32ThreadHandle();

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean SetThreadPriority(SafeWaitHandle hthread, ThreadBackgroundgMode mode);

        // http://msdn.microsoft.com/en-us/library/aa480216.aspx
        [DllImport("Kernel32", SetLastError = true, EntryPoint = "CancelSynchronousIo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean CancelSynchronousIO(SafeWaitHandle hThread);
    }
}
