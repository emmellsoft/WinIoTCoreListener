# WinIoTCoreListener
A support library for discovering Windows IoT Core devices on a network, just like the WinIoTCoreWatcher application.


You can also get it as a NuGet package: "WinIoTCoreListener".


Example (the Program.cs file in a standard Console application):

```
using System;
using Laserbrain.WinIoTCoreListener.Lib;

namespace Laserbrain.WinIoTCoreListener.ConsoleExample
{
    internal static class Program
    {
        private static void Main()
        {
            using (IWinIotCoreListener winIotCoreListener = WinIotCoreListenerFactory.Create())
            {
                winIotCoreListener.OnDeviceInfoUpdated += (s, e) =>
                {
                    Console.WriteLine(e.UpdateStatus + ": " + e.DeviceInfo);
                };

                Console.WriteLine("Up'n'running. Press SPACE to list current devices and ESCAPE to exit!");

                while (true)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Spacebar:
                            Console.WriteLine("-------------------------");
                            foreach (DeviceInfo deviceInfo in winIotCoreListener.DeviceInfos)
                            {
                                Console.WriteLine("* " + deviceInfo);
                            }
                            Console.WriteLine("-------------------------");
                            break;

                        case ConsoleKey.Escape:
                            return;
                    }
                }
            }
        }
    }
}
```
