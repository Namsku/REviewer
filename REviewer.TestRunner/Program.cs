using System;
using REviewer.Services;
using REviewer.Core.Constants;
using REviewer.Modules.RE.Json;
using System.Collections.Generic;

namespace REviewer.TestRunner
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Running Manual Tests...");
            int failures = 0;

            failures += RunGameStateServiceTests();
            failures += RunTimerServiceTests();
            // failures += RunInventoryServiceTests(); // Skip for now if dependencies are tricky

            if (failures == 0)
            {
                Console.WriteLine("All tests passed!");
                return 0;
            }
            else
            {
                Console.WriteLine($"{failures} tests failed.");
                return 1;
            }
        }

        static int RunGameStateServiceTests()
        {
            int fails = 0;
            Console.WriteLine("Testing GameStateService...");

            var service = new GameStateService();
            service.Deaths = 5;
            service.Resets = 3;
            service.SetGame(GameConstants.BIOHAZARD_2);

            if (service.SelectedGame != GameConstants.BIOHAZARD_2) { Console.WriteLine("FAIL: SetGame did not set SelectedGame"); fails++; }
            if (service.Deaths != 0) { Console.WriteLine("FAIL: SetGame did not reset Deaths"); fails++; }
            if (service.Resets != 0) { Console.WriteLine("FAIL: SetGame did not reset Resets"); fails++; }

            // Test RE1 Death
            service.SetGame(GameConstants.BIOHAZARD_1);
            service.UpdateState(0, null, null, null, null, null, null, null); // Init previous state
            service.UpdateState(0x01000000, null, null, null, null, null, null, null); // Death state
            if (!service.IsDead) { Console.WriteLine("FAIL: Top-level RE1 death detection failed"); fails++; }
            if (service.Deaths != 1) { Console.WriteLine("FAIL: RE1 death count not incremented"); fails++; }

            return fails;
        }

        static int RunTimerServiceTests()
        {
            int fails = 0;
            Console.WriteLine("Testing TimerService...");

            var service = new TimerService();
            
            // RE1 Test: 30fps
            long timerValue = 90; // 3 seconds
            service.UpdateTimer(GameConstants.BIOHAZARD_1, timerValue, null, null, false, 0);
            if (service.IGTHumanFormat != "00:00:03.00") { Console.WriteLine($"FAIL: RE1 Timer format incorrect. Expected 00:00:03.00, got {service.IGTHumanFormat}"); fails++; }

            // RE2 Test: Timer + Frame/60
            timerValue = 10;
            long frameValue = 30; // 0.5s
            service.UpdateTimer(GameConstants.BIOHAZARD_2, timerValue, frameValue, null, false, 0);
            if (service.IGTHumanFormat != "00:00:10.50") { Console.WriteLine($"FAIL: RE2 Timer format incorrect. Expected 00:00:10.50, got {service.IGTHumanFormat}"); fails++; }

            return fails;
        }
    }
}
