using System.Diagnostics;
using System.Runtime.InteropServices;

public static class WindowMinimizer
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_MINIMIZE = 6;

    private static void MinimizeWindow(IntPtr windowHandle)
    {
        ShowWindow(windowHandle, SW_MINIMIZE);
    }

    private const int SW_RESTORE = 9;

    private static void ShowMinimizedWindow(IntPtr windowHandle)
    {
        ShowWindow(windowHandle, SW_RESTORE);
    }


    public static void MinimizeWindowByProcessName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);

        if (processes.Length > 0)
        {
            IntPtr mainWindowHandle = processes[0].MainWindowHandle;
            MinimizeWindow(mainWindowHandle);
        }
        else
        {

            Console.WriteLine($"Process '{processName}' not found.");
        }
    }

    public static void ShowMinimizedWindowByProcessName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);

        if (processes.Length > 0)
        {
            IntPtr mainWindowHandle = processes[0].MainWindowHandle;
            ShowMinimizedWindow(mainWindowHandle);
        }
        else
        {
            Console.WriteLine($"Process '{processName}' not found.");
        }
    }
}
