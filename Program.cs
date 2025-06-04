// See https://aka.ms/new-console-template for more information

using Archipelago.Core.GameClients;
using Archipelago.Core.Util;
using ArchipelagoTest; // Make sure this namespace matches your ApplicationStartup class

// --- Elevation Check and Restart Logic ---
// This block should be at the very beginning of your application's entry point.
if (!ApplicationStartup.IsRunningAsAdministrator())
{
    Console.WriteLine("Application is not running as administrator. Attempting to restart with elevated privileges...");
    ApplicationStartup.RestartAsAdministrator();
    // IMPORTANT: If RestartAsAdministrator succeeds, it will call Environment.Exit(0)
    // and this current non-elevated process will terminate.
    // If RestartAsAdministrator fails (e.g., user declines UAC), it will print an error
    // and the current non-elevated process will continue from here.
    return; // Exit the current non-ezlevated instance, whether a restart was initiated or failed.
}

// --- Application Logic (Only runs if elevated) ---
// If the code reaches this point, it means the application is now running as administrator.
Console.WriteLine("Application is running as administrator.");

if (!PrivilegeHelper.EnableDebugPrivilege())
{
    Console.WriteLine("Failed to enable SeDebugPrivilege. This might prevent interaction with some processes.");
    // Decide if you want to exit here, or continue and handle potential failures.
}
else
{
    Console.WriteLine("SeDebugPrivilege enabled successfully.");
}

var gameClient = new DuckstationClient();
bool connected = gameClient.Connect();

if (!connected)
{
    Console.WriteLine("Failed to connect to Duckstation. Is it running and loaded with a game?");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return; // Exit if not connected
}

Console.WriteLine("Successfully connected to Duckstation.");

ulong mem = 0;
try
{
    mem = Memory.GetDuckstationOffset();
    Console.WriteLine($"Duckstation memory offset: {mem}");
}
catch (System.ComponentModel.Win32Exception w32ex)
{
    if (w32ex.NativeErrorCode == 5) // Error code 5 is "Access is denied."
    {
        Console.WriteLine("Access denied when trying to get Duckstation memory offset.");
        Console.WriteLine("This might happen if Duckstation is running with higher integrity than your app, or if specific permissions are still missing.");
        Console.WriteLine($"Details: {w32ex.Message}");
    }
    else
    {
        Console.WriteLine($"An unexpected Win32Exception occurred: {w32ex.Message}");
    }
    Console.WriteLine(w32ex); // Print full exception for debugging
}
catch (Exception ex)
{
    Console.WriteLine($"An unexpected error occurred while getting Duckstation memory offset: {ex.Message}");
    Console.WriteLine(ex); // Print full exception for debugging
}

Console.WriteLine("Press Enter to exit...");
Console.ReadLine(); // Keep console open until user presses Enter
