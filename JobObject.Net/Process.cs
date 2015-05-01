using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.JobManagement
{
    /// <summary>
    /// A factory that creates processes with the CREATE_BREAKAWAY_FROM_JOB set, that enables to assign the process to jobs even if the current process is itself running inside a job (That's the case when running the program in Visual Studio for instance) 
    /// </summary>
    public class ProcessFactory
    {
        private const int CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
        private const int CREATE_NO_WINDOW = 0x08000000;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            out ProcessInfo lpProcessInformation
        );


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct ProcessInfo
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 ProcessId;
            public Int32 ThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SecurityAttributes
        {
            public int length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct StartupInfo
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }


        public static System.Diagnostics.Process CreateProcess(string path, string args, string currentDirectory)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("File does not exist");
            }
            ProcessInfo proc = new ProcessInfo();
            try
            {
                var inf = new StartupInfo();

                inf.cb = (uint)Marshal.SizeOf(typeof(StartupInfo));
                var cmd = path;
                if(!string.IsNullOrWhiteSpace(args))
                {
                    cmd += " " + args;
                }
                if (!CreateProcess(null, cmd, IntPtr.Zero, IntPtr.Zero, false, CREATE_BREAKAWAY_FROM_JOB | CREATE_NO_WINDOW, IntPtr.Zero, currentDirectory, ref inf, out proc))
                {
                    throw new InvalidOperationException("Couldn't create process");
                }

                return System.Diagnostics.Process.GetProcessById(proc.ProcessId);
            }
            finally
            {
                if (proc.hProcess != IntPtr.Zero)
                {
                    Interop.CloseHandle(proc.hProcess);
                }
            }


        }



    }
}
