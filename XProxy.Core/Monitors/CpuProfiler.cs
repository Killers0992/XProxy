using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Core.Monitors
{
    public class CpuProfiler
    {
        public static double GetCpuUsage()
        {
            using (var process = Process.GetCurrentProcess())
            {
                // Take initial CPU time snapshot
                TimeSpan startCpuTime = process.TotalProcessorTime;
                DateTime startTime = DateTime.UtcNow;

                // Sleep for an interval to measure CPU time
                Thread.Sleep(500);  // Adjust as needed for better accuracy

                // Take a second CPU time snapshot
                TimeSpan endCpuTime = process.TotalProcessorTime;
                DateTime endTime = DateTime.UtcNow;

                // Calculate CPU usage
                double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
                double elapsedMs = (endTime - startTime).TotalMilliseconds;

                // Percentage CPU = (CPU time used / elapsed time) * number of processors
                return (cpuUsedMs / elapsedMs) * 100 / Environment.ProcessorCount;
            }
        }
    }
}
