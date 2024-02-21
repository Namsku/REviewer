using System;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace REviewer.Modules
{
    public class UniqueIdGenerator
    {
        public byte[] GenerateUniqueId()
        {
            // Get the current timestamp as a byte array
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // Generate a random 6-byte number
            byte[] randomBytes = new byte[6];
            RandomNumberGenerator.Fill(randomBytes);

            // Combine the timestamp and random number to get a 14-byte unique ID
            byte[] id = new byte[14];
            Buffer.BlockCopy(timestampBytes, 0, id, 0, 8);
            Buffer.BlockCopy(randomBytes, 0, id, 8, 6);

            return id;
        }
    }
    public class Common
    {
        // Get the whole process list, and find the process with the given name
        public static Process? GetProcessByName(string processName)
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }

        // Check if ddraw.dll is loaded in the process
        public static bool IsDdrawLoaded(Process process)
        {
            return process.Modules.Cast<ProcessModule>().Any(module => module.ModuleName == "ddraw.dll");
        }

        public static bool IsSavePathPresent(int index)
        {
            var gameNames = new[] { "RE1", "RE2", "RE3", "RECVX" };

            // Load the json file that contains the save paths
            var configPath = ConfigurationManager.AppSettings["Config"];
            if (configPath != null && File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var gamePaths = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                return gamePaths.ContainsKey(gameNames[index]);
            }

            throw new ArgumentNullException(nameof(configPath));
        }

        public string GetSavePath(string gameName)
        {
            Dictionary<string, string> keyValuePairs = new()
            {
                {"Bio", "RE1"},
            };

            if (!keyValuePairs.ContainsKey(gameName))
            {
                throw new ArgumentException($"Game name {gameName} not found in keyValuePairs dictionary", nameof(gameName));
            }

            // Load the json file that contains the save paths
            var configPath = ConfigurationManager.AppSettings["Config"];
            if (configPath != null && File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var gamePaths = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var gameKey = keyValuePairs[gameName];
                if (gamePaths.ContainsKey(gameKey))
                {
                    return gamePaths[gameKey];
                }
            }

            throw new ArgumentNullException(nameof(configPath), "Config path is null or file does not exist");
        }

        // Get the base address of a module in the process
        public static nint GetModuleBaseAddress(nint processHandle, string moduleName)
        {
            var process = Process.GetProcessById((int)processHandle);
            var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(module => module.ModuleName == moduleName);
            return module?.BaseAddress ?? 0;
        }
    }
}
