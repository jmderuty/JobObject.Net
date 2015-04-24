using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestTechno
{
    public class Process
    {
        private const int CREATE_BREAKAWAY_FROM_JOB = 0x01000000;

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


        public Process(string path,string args, string currentDirectory)
        {
            if(!File.Exists(path))
            {
                throw new ArgumentException("File does not exist");
            }

            _inf = new StartupInfo();
            _inf.cb = (uint)Marshal.SizeOf(typeof(StartupInfo));
            _proc = new ProcessInfo();
            if(!CreateProcess(path, args, IntPtr.Zero, IntPtr.Zero, false, CREATE_BREAKAWAY_FROM_JOB,IntPtr.Zero,currentDirectory,ref _inf,out _proc))
            {
                throw new InvalidOperationException("Couldn't create process");
            }
        }

        private ProcessInfo _proc;
        private StartupInfo _inf;

        public IntPtr Handle
        {
            get
            {
                return _proc.hProcess;
            }
        }

        public int ProcessId
        {
            get
            {
                return _proc.ProcessId;
            }
        }

    }
}
