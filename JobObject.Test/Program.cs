using Stormancer.JobManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobObject.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var job = new Job())
            {
                var prc = ProcessFactory.CreateProcess("../../../JobObject.Test.ChildProcess/bin/Debug/JobObject.Test.ChildProcess.exe", "test", null);

                job.AddProcess(prc);

                Console.Read();
            }
        }
    }
}
