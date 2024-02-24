
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace REviewer.Modules.Utils
{
    public static class CustomColors
    {
        public static readonly Color Yellow = Color.FromArgb(215, 191, 128);
        public static readonly Color Orange = Color.FromArgb(198, 155, 101);
        public static readonly Color Red = Color.FromArgb(205, 116, 118);
        public static readonly Color White = Color.FromArgb(250, 240, 216);
        public static readonly Color Default = Color.FromArgb(159, 185, 118);
    }
    public static class UniqueIdGenerator
    {
        public static byte[] GenerateUniqueId()
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
    public class Library
    {

        private static readonly string _configPath = ConfigurationManager.AppSettings["Config"];
        private static readonly Dictionary<string, string> _gamePaths = LoadGamePaths();
        private static readonly Dictionary<string, string> _keyValuePairs = new()
        {
            {"Bio", "RE1"},
        };

        public static nint GetModuleBaseAddress(nint processHandle, string moduleName)
        {
            // Get the base address of a module in the process
            var process = Process.GetProcessById((int)processHandle);
            var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(module => module.ModuleName == moduleName);
            return module?.BaseAddress ?? 0;
        }

        public static Process? GetProcessByName(string processName)
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }
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

            Logger.Instance.Error("Config path is null or file does not exist");
            return false;
        }

        private static Dictionary<string, string>? LoadGamePaths()
        {
            if (string.IsNullOrEmpty(_configPath) || !File.Exists(_configPath))
            {
                Logger.Instance.Error("Config path is null or file does not exist");
                return [];
            }

            var json = File.ReadAllText(_configPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static string GetSavePath(string gameName)
        {
            if (!_keyValuePairs.TryGetValue(gameName, out var gameKey))
            {
                Logger.Instance.Error($"Game name {gameName} not found in keyValuePairs dictionary");
                return "";
            }

            if (!_gamePaths.TryGetValue(gameKey, out var gamePath))
            {
                Logger.Instance.Error($"Game key {gameKey} not found in gamePaths dictionary");
                return "";
            }

            return gamePath;
        }


        public static string ToHexString(int value) => string.Join(" ", Enumerable.Range(0, 4).Select(i => value.ToString("X8").Substring(i * 2, 2)));

    }
}
