using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseWiggler;

class Program
{
    private const int MOUSE_MAX_IDLE_TIME_IN_MILLISECONDS = 3000;
    private const int WAIT_TIME_BETWEEN_EACH_HOP_IN_MILLISECONDS = 500;

    // Import the necessary WinAPI functions
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    public static Stopwatch StopwatchForLastAutoWiggle = new();

    static async Task Main(string[] args)
    {
        // Check the platform type
        if (!IsWindows())
        {
            Console.WriteLine("Error: This application can only run on Windows.");
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        StopwatchForLastAutoWiggle.Start();

        Console.WriteLine($"Mouse wiggling every {MOUSE_MAX_IDLE_TIME_IN_MILLISECONDS / 1000} seconds. Press 'q' to exit.");

        // Start a thread to wiggle the mouse
        var mouseWigglerThread = MouseWiggler(cancellationToken);


        // Wait for user input to cancel the task
        while (Console.ReadKey().Key != ConsoleKey.Q) { }
        cancellationTokenSource.Cancel();

        await mouseWigglerThread;
    }

    private static async Task MouseWiggler(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (GetUserIdleTimeInMilliSeconds() < MOUSE_MAX_IDLE_TIME_IN_MILLISECONDS || StopwatchForLastAutoWiggle.ElapsedMilliseconds < MOUSE_MAX_IDLE_TIME_IN_MILLISECONDS)
            {
                await Task.Delay(100);
                continue;
            }

            Console.WriteLine($"User is idle for more than {MOUSE_MAX_IDLE_TIME_IN_MILLISECONDS / 1000} seconds. Simulating mouse movement.");

            // Move the mouse a little to the right
            MoveMouseRelativeToCurrentPosition(100, 0);
            StopwatchForLastAutoWiggle.Restart();

            await Task.Delay(WAIT_TIME_BETWEEN_EACH_HOP_IN_MILLISECONDS);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (GetUserIdleTimeInMilliSeconds() < MOUSE_MAX_IDLE_TIME_IN_MILLISECONDS)
            {
                StopwatchForLastAutoWiggle.Restart();
                continue;
            }

            // Move the mouse a little to the left
            MoveMouseRelativeToCurrentPosition(-100, 0);

            await Task.Delay(WAIT_TIME_BETWEEN_EACH_HOP_IN_MILLISECONDS);
        }
    }

    private static void MoveMouseRelativeToCurrentPosition(int deltaX, int deltaY)
    {
        // Get current mouse position
        GetCursorPos(out POINT currentPos);

        // Calculate new position
        int newX = currentPos.X + deltaX;
        int newY = currentPos.Y + deltaY;

        // Set new mouse position
        SetCursorPos(newX, newY);
    }

    private static uint GetUserIdleTimeInMilliSeconds()
    {
        var lastInputInfo = new LASTINPUTINFO();
        lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
        GetLastInputInfo(ref lastInputInfo);

        var idletime = ((uint)Environment.TickCount - lastInputInfo.dwTime);
        //Console.WriteLine($"Current idle time: {idletime / 1000} seconds");
        return idletime;
    }

    private static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}