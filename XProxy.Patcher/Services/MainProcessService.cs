using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using XProxy.Shared.Services;

namespace XProxy.Patcher.Services
{
    public class MainProcessService : BackgroundService
    {
        static string Executable => OperatingSystem.IsWindows() ? "XProxy.exe" : "XProxy";
        
        static string AssemblyFile => "XProxy.dll";
        public static Version AssemblyVersion { get; set; } = new Version(0, 0, 0);
        public static bool AssemblyUpdated { get; set; }


        public static bool IsUpdating;
        public static bool DoUpdate;
        public static bool Exited;

        Process _mainProcess;

        void GetAssemblyVersion()
        {
            if (File.Exists(AssemblyFile))
            {
                AssemblyName name = AssemblyName.GetAssemblyName(AssemblyFile);
                if (name != null)
                    AssemblyVersion = name.Version;
            }
            else
            {
                DoUpdate = true;
                AssemblyVersion = new Version(0, 0, 0);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            GetAssemblyVersion();

            Console.WriteLine("Check for updates");

            while (UpdaterService.CheckForUpdates)
            {
                await Task.Delay(10);
            }

            Console.WriteLine("Start process");

            while (true)
            {
                if (File.Exists(Executable))
                {
                    StartProcess();

                    if (_mainProcess == null)
                    {
                        Console.WriteLine("Failed to start main process, retrying in 5 seconds...");
                        await Task.Delay(5000);
                        continue;
                    }

                    Console.WriteLine("Waiting for exit");
                    await _mainProcess.WaitForExitAsync();
                    Exited = true;

                    DoUpdate = File.Exists("./update");
                    if (DoUpdate) IsUpdating = true;
                }

                while (IsUpdating)
                {
                    await Task.Delay(10);
                }

                if (AssemblyUpdated)
                    GetAssemblyVersion();

                await Task.Delay(10);
            }
        }

        void StartProcess()
        {
            ProcessStartInfo info = new ProcessStartInfo(Executable);

            _mainProcess = Process.Start(info);
        }
    }
}
