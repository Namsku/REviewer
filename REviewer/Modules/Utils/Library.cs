
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Newtonsoft.Json;
using REviewer.Modules.RE.Json;


namespace REviewer.Modules.Utils
{
    public class Library
    {

        private static readonly string? _configPath = ConfigurationManager.AppSettings["Config"];
        private static readonly Dictionary<string, string> _keyValuePairs = new()
        {
            {"Bio", "RE1"},
            {"Bio2 1.10", "RE2"},
            {"BIOHAZARD(R) 3 PC", "RE3" }
        };

        public static Dictionary<string, string> _gameList = new Dictionary<string, string>
            {
                { "Bio", "RE1" },
                { "bio", "RE1" },
                { "Biohazard", "RE1" },
                { "biohazard", "RE1" },
                { "Bio2", "RE2" },
                { "Bio2 1.10", "RE2" },
                { "bio2 1.10", "RE2" },
                { "bio2 1.1", "RE2" },
                { "bio2 v1.1", "RE2" },
                { "re2mm", "RE2"},
                { "RE2MM", "RE2"},
                { "REVisited", "RE2" },
                { "UEv3", "RE2"},
                { "toos_ver2_0_0", "RE2" },
                { "toos(hard)_ver2_0_0", "RE2" },
                { "bunny", "RE2" },
                { "bunny2", "RE2" },
                { "BIOHAZARD(R) 3 PC", "RE3" },
                { "biohazard(r) 3 pc", "RE3" },
                { "bio3", "RE3" },
                { "Bio3", "RE3" }
            };

        public static Dictionary<string, List<string>> _gameVersions = new Dictionary<string, List<string>>
        {
            { "Bio", new List<string> { "Bio", "bio", "Biohazard", "biohazard" } },
            { "bio2 1.10", new List<string> { "Bio2 1.10", "bio2 1.10", "bio2 1.1", "bio2", "bio2 v1.1", "bunny", "bunny2" , "re2mm", "RE2MM", "REVisited", "UEv3", "toos_ver2_0_0", "toos(hard)_ver2_0_0" } },
            { "BIOHAZARD(R) 3 PC", new List<string> { "BIOHAZARD(R) 3 PC","biohazard(r) 3 pc","Bio3", "bio3" } }
        };

        private static readonly Dictionary<string, string> _correctProccessName = new Dictionary<string, string>()
        {
            { "Bio", "Bio" },
            { "bio", "Bio" },
            { "Biohazard", "Bio" },
            { "biohazard", "Bio" },
            { "Bio2", "Bio2 1.10" },
            { "Bio2 1.10", "Bio2 1.10" },
            { "bio2 1.10", "Bio2 1.10" },
            { "bio2 1.1", "Bio2 1.10" },
            { "bio2 v1.1", "Bio2 1.10" },
            { "re2mm", "Bio2 1.10" },
            { "RE2MM", "Bio2 1.10" },
            { "REVisited", "Bio2 1.10" },
            { "UEv3", "Bio2 1.10" },
            { "toos_ver2_0_0", "Bio2 1.10" },
            { "toos(hard)_ver2_0_0", "Bio2 1.10" },
            { "bunny", "Bio2 1.10" },
            { "bunny2", "Bio2 1.10" },
            { "BIOHAZARD(R) 3 PC", "BIOHAZARD(R) 3 PC" },
            { "biohazard(r) 3 pc", "BIOHAZARD(R) 3 PC" },
            { "bio3", "BIOHAZARD(R) 3 PC" },
            { "Bio3", "BIOHAZARD(R) 3 PC" }
        };

        public static Dictionary<string, string> GetGameList()
        {
            return _gameList;
        }

        public static List<string> GetGameVersions(string key)
        {
            return _gameVersions.TryGetValue(key, out List<string>?  versions) ? versions : new List<string>();
        }

        public static Dictionary<string, string> GetGameProcesses()
        {
            return _correctProccessName;
        }

        public static string GetGameName(string processName)
        {
            if (_keyValuePairs.TryGetValue(processName, out var gameName))
            {
                return gameName;
            }
            else
            {
                throw new KeyNotFoundException($"The process name {processName} does not exist in the dictionary.");
            }
        }

        public static void UpdateTextBlock(TextBlock textBlock, string? text = null, SolidColorBrush? color = null, bool? isBold = null, string? font = null, double? size = null)
        {
            textBlock.Dispatcher.Invoke(() =>
            {
                if (text != null)
                {
                    textBlock.Text = text;
                }

                if (color != null)
                {
                    textBlock.Foreground = color;
                }

                if (isBold != null)
                {
                    textBlock.FontWeight = isBold.Value ? FontWeights.Bold : FontWeights.Normal;
                }

                if (font != null)
                {
                    textBlock.FontFamily = new FontFamily(font);
                }

                if (size != null)
                {
                    textBlock.FontSize = size.Value;
                }
            });
        }

        public static void UpdateTextBox(TextBox textBox, string? text = null, SolidColorBrush? color = null, bool? isBold = null, string? font = null, double? size = null)
        {
            textBox.Dispatcher.Invoke(() =>
            {
                if (text != null)
                {
                    textBox.Text = text;
                }

                if (color != null)
                {
                    textBox.Foreground = color;
                }

                if (isBold != null)
                {
                    textBox.FontWeight = isBold.Value ? FontWeights.Bold : FontWeights.Normal;
                }

                if (font != null)
                {
                    textBox.FontFamily = new FontFamily(font);
                }

                if (size != null)
                {
                    textBox.FontSize = size.Value;
                }
            });
        }

        public static nint GetModuleBaseAddress(nint processHandle, string moduleName)
        {
            // Get the base address of a module in the process
            var process = Process.GetProcessById((int)processHandle);
            var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(module => module.ModuleName == moduleName);
            return module?.BaseAddress ?? (IntPtr) 0;
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

        public static Dictionary<string, string> GetReviewerConfig()
        {
            var configPath = ConfigurationManager.AppSettings["Config"];

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath), "Configuration file path is invalid or missing.");
            }

            var json = File.ReadAllText(configPath);
            var reJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                         ?? throw new ArgumentNullException("The game data is null");

            return reJson;
        }

        public static Dictionary<string, bool> GetOptions()
        {
            var reJson = GetReviewerConfig();

            return new Dictionary<string, bool>
            {
                { "isBiorandMode", reJson["isBiorandMode"] == "true" },
                { "isNormalMode", reJson["isNormalMode"] == "true"},
                { "isHealthBarChecked", reJson["isHealthBarChecked"] == "true" },
                { "isItemBoxChecked", reJson["isItemBoxChecked"] == "true" },
                { "isChrisInventoryChecked", reJson["isChrisInventoryChecked"] == "true" },
                { "isSherryChecked", reJson["isSherryChecked"] == "true" },
                { "isMinimalistChecked", reJson["isMinimalistChecked"] == "true" },
                { "isNoSegmentsTimerChecked", reJson["isNoSegmentsTimerChecked"] == "true" },
                { "isNoStatsChecked", reJson["isNoStatsChecked"] == "true" },
                { "isNoKeyItemsChecked", reJson["isNoKeyItemsChecked"] == "true" }
            };
        }
        public static void UpdateConfigFile(string key, string value)
        {
            var configPath = ConfigurationManager.AppSettings["Config"];
            var reJson = Library.GetReviewerConfig();

            reJson[key] = value;
            File.WriteAllText(configPath, JsonConvert.SerializeObject(reJson, Formatting.Indented));
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

        public static string GetProcessMD5Hash(Process process)
        {
            if (process.MainModule?.FileName == null)
            {
                throw new ArgumentNullException("The process has no main module");
            }

            using var md5 = MD5.Create();
            using var stream = File.OpenRead(process.MainModule.FileName);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

    public static int HexToInt(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = hex[2..];
            }

            long value = Convert.ToInt64(hex, 16);
            return (int)value;
        }
        
        public static string ToHexString(int value) => string.Join(" ", Enumerable.Range(0, 4).Select(i => value.ToString("X8").Substring(i * 2, 2)));


    }
}
