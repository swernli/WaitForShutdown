using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace waitforshutdown
{
    class Program
    {

        [DllImport("Kernel32")]
        internal static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool Add);
        internal delegate bool HandlerRoutine(CtrlTypes ctrlType);

        internal enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        public static void Main(string[] args)
        {
            var timeout = Int32.Parse(args[0]);

            var waitEvent = new System.Threading.AutoResetEvent(false);

            var hr = new HandlerRoutine(type =>
            {
                Console.WriteLine($"ConsoleCtrlHandler got signal: {type}");

                Console.WriteLine("Waiting for shutdown.");
                for (int countdown = 1; countdown <= timeout; countdown++)
                {
                    Console.WriteLine($"{countdown}");
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                Console.WriteLine("Done waiting - setting event and exiting.");

                waitEvent.Set();

                return false;
            });
            SetConsoleCtrlHandler(hr, true);

            Console.WriteLine("Shutdown will wait {0} seconds.", timeout);
            Console.WriteLine("Waiting on handler to trigger");

            waitEvent.WaitOne();

            GC.KeepAlive(hr);
        }
    }
}
