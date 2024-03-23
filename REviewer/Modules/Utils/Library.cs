
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Newtonsoft.Json;


namespace REviewer.Modules.Utils
{
    public class Library
    {

        private static readonly string? _configPath = ConfigurationManager.AppSettings["Config"];
        private static readonly Dictionary<string, string> _keyValuePairs = new()
        {
            {"Bio", "RE1"},
            {"Bio2 1.10", "RE2"},
            { "BIOHAZARD(R) 3 PC", "RE3" }
        };

        public static Dictionary<string, string> _gameList = new Dictionary<string, string>
            {
                { "Bio", "RE1" },
                { "bio", "RE1" },
                { "Biohazard", "RE1" },
                { "biohazard", "RE1" },
                { "Bio2 1.10", "RE2" },
                { "bio2 1.10", "RE2" },
                { "BIOHAZARD(R) 3 PC", "RE3" },
                { "biohazard(r) 3 pc", "RE3" },
                { "bio3", "RE3" },
                { "Bio3", "RE3" }
            };

        public static Dictionary<string, string> GetGameList()
        {
            return _gameList;
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
            if (process.MainModule == null)
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
