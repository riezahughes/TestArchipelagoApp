using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection; // Added for Assembly

namespace ArchipelagoTest
{
    internal class ApplicationStartup
    {
        public static bool IsRunningAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdministrator()
        {
            // Get the path to the current entry assembly (your application's DLL or EXE)
            string? appEntryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            if (appEntryAssemblyLocation == null)
            {
                Console.WriteLine("Error: Entry assembly location is null. Cannot restart.");
                Environment.Exit(1); // Exit with an error code
                return;
            }

            // Get the path to the executable that started the current process
            // This will be "dotnet.exe" if you're running via `dotnet run` or F5 in VS,
            // or your app's .exe if it's a self-contained deployment.
            string currentProcessExecutable = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

            string fileNameToExecute;
            string argumentsToPass = "";

            // Check if the current process executable is 'dotnet.exe'
            if (currentProcessExecutable.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                // We're running via dotnet.exe, so we need to restart dotnet.exe
                // and pass our application's DLL as an argument.
                fileNameToExecute = currentProcessExecutable; // This is "dotnet.exe"
                argumentsToPass = appEntryAssemblyLocation;  // This is "ArchipelagoTest.dll"
            }
            else
            {
                // We're likely running a self-contained executable (.exe).
                // In this case, the appEntryAssemblyLocation already points to the .exe.
                fileNameToExecute = appEntryAssemblyLocation; // This is "ArchipelagoTest.exe"
                // No arguments are needed as it's the executable itself.
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = fileNameToExecute,
                Arguments = argumentsToPass, // Pass arguments if running via dotnet.exe
                Verb = "runas" // This triggers the elevation prompt
            };

            try
            {
                Process.Start(startInfo);
                Environment.Exit(0); // Close current instance
            }
            catch (Exception ex)
            {
                // User declined elevation or other error
                Console.WriteLine($"Failed to restart as administrator: {ex.Message}");
            }
        }
    }
}