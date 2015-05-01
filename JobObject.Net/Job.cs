using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Stormancer.JobManagement
{
    public class Job : IDisposable
    {
        private const int JobObjectExtendedLimitInformation = 9;
        private const int CREATE_SUSPENDED = 0x00000004;

        private const int INFINITE = -1;
        private const int GWL_STYLE = -16;
        private const int WS_CHILD = 0x40000000;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int HWND_MESSAGE = -3;





     

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr a, string lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, UInt32 cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        private IntPtr handle;
        private bool disposed;

        public Job()
        {
            handle = CreateJobObject(IntPtr.Zero, null);
            UpdateMemoryLimit(null, null, null);


        }

        public void UpdateMemoryLimit(uint? minPrcWorkingSet, uint? maxPrcWorkingSet, uint? maxJobVirtualMemory)
        {
            var flags = JOBOBJECT_BASIC_LIMIT_FLAGS.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            var basicLimits = new JOBOBJECT_BASIC_LIMIT_INFORMATION();
            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = basicLimits,

            };
            if (minPrcWorkingSet.HasValue && maxPrcWorkingSet.HasValue)
            {
                flags |= JOBOBJECT_BASIC_LIMIT_FLAGS.JOB_OBJECT_LIMIT_WORKINGSET;
                basicLimits.MinimumWorkingSetSize = (UIntPtr)minPrcWorkingSet.Value;
                basicLimits.MaximumWorkingSetSize = (UIntPtr)maxPrcWorkingSet.Value;

            }

            if(maxJobVirtualMemory.HasValue)
            {
                flags |= JOBOBJECT_BASIC_LIMIT_FLAGS.JOB_OBJECT_LIMIT_JOB_MEMORY;
                extendedInfo.JobMemoryLimit = (UIntPtr)maxJobVirtualMemory.Value;
            }


            basicLimits.LimitFlags = (uint)flags;

            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
            {
                Marshal.FreeHGlobal(extendedInfoPtr);
                throw new Exception(string.Format("Unable to set extended information.  Error: {0}", Marshal.GetLastWin32Error()));
            }
            Marshal.FreeHGlobal(extendedInfoPtr);

        }

        /// <summary>
        /// Updates the CPU rate limit associated with the job.
        /// </summary>
        /// <param name="value">The limit in total CPU percent.</param>
        public void UpdateCpuRateLimit(double value)
        {
            var cpuRateInfo = new JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
            {
                ControlFlags = 1 | 4,
                CpuRate = (uint)(value * 100)
            };
            var length = Marshal.SizeOf(typeof(JOBOBJECT_CPU_RATE_CONTROL_INFORMATION));
            IntPtr cpuRateInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(cpuRateInfo, cpuRateInfoPtr, false);
            if (!SetInformationJobObject(handle, JobObjectInfoType.JobObjectCpuRateControlInformation, cpuRateInfoPtr, (uint)length))
            {
                Marshal.FreeHGlobal(cpuRateInfoPtr);
                throw new Exception(string.Format("Unable to set cpu rate information.  Error: {0}", Marshal.GetLastWin32Error()));

            }
            Marshal.FreeHGlobal(cpuRateInfoPtr);

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) { }

            Close();
            disposed = true;
        }

        public void Close()
        {
            Interop.CloseHandle(handle);
            handle = IntPtr.Zero;
        }

        public bool AddProcess(IntPtr processHandle)
        {
            return AssignProcessToJobObject(handle, processHandle);
        }

        public bool AddProcess(int processId)
        {
            return AddProcess(System.Diagnostics.Process.GetProcessById(processId));
        }

        public bool AddProcess(System.Diagnostics.Process process)
        {
            return AddProcess(process.Handle);
        }

       

    }

    #region Helper classes

    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    }

    internal enum JOBOBJECT_BASIC_LIMIT_FLAGS
    {
        JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000,
        JOB_OBJECT_LIMIT_JOB_MEMORY = 0x00000200,
        JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x00000100,
        JOB_OBJECT_LIMIT_WORKINGSET = 0x00000001,


    }
    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public Int64 PerProcessUserTimeLimit;
        public Int64 PerJobUserTimeLimit;
        public UInt32 LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public UInt32 ActiveProcessLimit;
        public UIntPtr Affinity;
        public UInt32 PriorityClass;
        public UInt32 SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public UInt32 nLength;
        public IntPtr lpSecurityDescriptor;
        public Int32 bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
    {
        public UInt32 ControlFlags;
        public UInt32 CpuRate;

    }
    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions = 4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11,
        JobObjectCpuRateControlInformation = 15
    }

    #endregion

}