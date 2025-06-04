using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;

public class PrivilegeHelper
{
    // Constants for access rights and privileges
    private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    private const uint TOKEN_QUERY = 0x0008;
    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    private const string SE_DEBUG_NAME = "SeDebugPrivilege";

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle,
        uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LookupPrivilegeValue(string lpSystemName,
        string lpName, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
        bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState,
        uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes;
    }

    public static bool EnableDebugPrivilege()
    {
        IntPtr hToken = IntPtr.Zero;
        TOKEN_PRIVILEGES tp;
        LUID luid;

        Process currentProcess = Process.GetCurrentProcess();
        IntPtr hProcess = currentProcess.Handle; // GetCurrentProcess() returns a pseudo-handle, but Process.GetCurrentProcess().Handle is usually fine.

        try
        {
            if (!OpenProcessToken(hProcess, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken))
            {
                Console.WriteLine($"OpenProcessToken failed: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
                return false;
            }

            if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, out luid))
            {
                Console.WriteLine($"LookupPrivilegeValue failed: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
                return false;
            }

            tp.PrivilegeCount = 1;
            tp.Privileges.Luid = luid;
            tp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

            if (!AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                Console.WriteLine($"AdjustTokenPrivileges failed: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
                return false;
            }
            Console.WriteLine("looks all good to me");
            return true;
        }
        finally
        {
            if (hToken != IntPtr.Zero)
            {
                CloseHandle(hToken);
            }
        }
    }
}

// How to use it in your main program:
// if (!PrivilegeHelper.EnableDebugPrivilege())
// {
//     Console.WriteLine("Failed to enable SeDebugPrivilege. OpenProcess might fail.");
//     // Handle the error appropriately, perhaps exit or inform the user.
// }
// else
// {
//     // Now try your OpenProcess call
// }