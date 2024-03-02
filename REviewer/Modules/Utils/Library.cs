
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace REviewer.Modules.Utils
{
    public class Library
    {

        private static readonly string? _configPath = ConfigurationManager.AppSettings["Config"];
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
            var _gamePaths = LoadGamePaths();

            if (_gamePaths != null && _gamePaths.ContainsKey(gameNames[index]))
            {
                return true;
            }

            Logger.Instance.Error("Config path is null or file does not exist");
            return false;
        }

        private static Dictionary<string, string>? LoadGamePaths()
        {
            if (string.IsNullOrEmpty(_configPath) || !File.Exists(_configPath))
            {
                Logger.Instance.Error("Config path is null or file does not exist");
                return null;
            }

            var json = File.ReadAllText(_configPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        public static string GetSavePath(string gameName)
        {
            var _gamePaths = LoadGamePaths();

            if (!_keyValuePairs.TryGetValue(gameName, out var gameKey))
            {
                Logger.Instance.Error($"Game name {gameName} not found in keyValuePairs dictionary");
                return "";
            }

            if (_gamePaths != null && _gamePaths.TryGetValue(gameKey, out string? gamePath))
            {
                return gamePath;
            }

            Logger.Instance.Error($"Game key {gameKey} not found in gamePaths dictionary");
            return "";
        }

        public static Dictionary<byte, List<int>>? ConvertDictionnary(Dictionary<string, List<string>>? originalDict)
        {
            if (originalDict == null)
            {
                return null;
            }

            var newDict = new Dictionary<byte, List<int>>();

            foreach (var pair in originalDict)
            {
                var intList = pair.Value.Select(int.Parse).ToList();
                newDict.Add(byte.Parse(pair.Key), intList);
            }

            return newDict;
        }

        public static nint HexToNint(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = hex[2..];
            }

            long value = Convert.ToInt64(hex, 16);
            return (nint)value;
        }
        
        public static string ToHexString(int value) => string.Join(" ", Enumerable.Range(0, 4).Select(i => value.ToString("X8").Substring(i * 2, 2)));
    }
}
