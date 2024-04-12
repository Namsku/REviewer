
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Newtonsoft.Json;
using REviewer.Modules.RE.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace REviewer.Modules.Utils
{
    public enum StringEnumType
    {
        AutoDetect,
        ASCII,
        UTF8,
        UTF16
    }
    public static class NativeWrappers
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, ref int lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcmp(byte[] b1, byte[] b2, long count);
        public static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }
    }
    public static class ExtensionMethods
    {
        public static bool Is64Bit(this Process process)
        {
            bool procWow64;
            NativeWrappers.IsWow64Process(process.Handle, out procWow64);
            if (Environment.Is64BitOperatingSystem && !procWow64)
                return true;
            return false;
        }

        public static bool IsRunning(this Process process)
        {
            int exitCode = 0;
            return NativeWrappers.GetExitCodeProcess(process.Handle, ref exitCode) && exitCode == 259;
        }

        public static int ExitCode(this Process process)
        {
            int exitCode = 0;
            NativeWrappers.GetExitCodeProcess(process.Handle, ref exitCode);
            return exitCode;
        }

        public static T ReadValue<T>(this Process process, IntPtr addr, bool swap = false, T default_ = default)
            where T : struct
        {
            T value;

            if (!process.ReadValue(addr, out value, swap))
                value = default_;

            return value;
        }

        public static bool ReadValue<T>(this Process process, IntPtr addr, out T value, bool swap = false)
            where T : struct
        {
            byte[] bytes;

            object val;
            value = default;

            Type type = typeof(T);
            type = type.IsEnum ? Enum.GetUnderlyingType(type) : type;

            int size = type == typeof(bool) ? 1 : Marshal.SizeOf(type);

            if (!ReadBytes(process, addr, size, out bytes, swap))
                return false;

            val = ResolveToType(bytes, type);
            value = (T)val;

            return true;
        }

        public static byte[] ReadBytes(this Process process, IntPtr addr, int size, bool swap = false)
        {
            byte[] bytes;

            if (!process.ReadBytes(addr, size, out bytes, swap))
                return new byte[size];

            return bytes;
        }

        public static bool ReadBytes(this Process process, IntPtr addr, int size, out byte[] value, bool swap = false)
        {
            var bytes = new byte[size];
            IntPtr read = IntPtr.Zero;

            value = null;

            if (!NativeWrappers.ReadProcessMemory(process.Handle, addr, bytes, size, out read))
                return false;

            if (swap)
                Array.Reverse(bytes);

            value = bytes;

            return true;
        }

        public static string ReadString(this Process process, IntPtr addr, int size, bool swap = false, string default_ = null)
        {
            string str;

            if (!process.ReadString(addr, size, out str, swap))
                return default_;

            return str;
        }

        public static string ReadString(this Process process, IntPtr addr, StringEnumType type, int size, bool swap = false, string default_ = null)
        {
            string str;

            if (!process.ReadString(addr, type, size, out str, swap))
                return default_;

            return str;
        }

        public static bool ReadString(this Process process, IntPtr addr, int size, out string value, bool swap = false) =>
            ReadString(process, addr, StringEnumType.AutoDetect, size, out value, swap);

        public static bool ReadString(this Process process, IntPtr addr, StringEnumType type, int size, out string value, bool swap = false)
        {
            var bytes = new byte[size];
            IntPtr read = IntPtr.Zero;

            value = null;

            if (!NativeWrappers.ReadProcessMemory(process.Handle, addr, bytes, size, out read))
                return false;

            if (swap)
                Array.Reverse(bytes);

            if (type == StringEnumType.AutoDetect)
                if (read.ToInt64() >= 2 && bytes[1] == '\x0')
                    value = Encoding.Unicode.GetString(bytes);
                else
                    value = Encoding.UTF8.GetString(bytes);
            else if (type == StringEnumType.UTF8)
                value = Encoding.UTF8.GetString(bytes);
            else if (type == StringEnumType.UTF16)
                value = Encoding.Unicode.GetString(bytes);
            else
                value = Encoding.ASCII.GetString(bytes);

            return true;
        }

        private static object ResolveToType(byte[] bytes, Type type)
        {
            object val;

            if (type == typeof(int))
                val = BitConverter.ToInt32(bytes, 0);
            else if (type == typeof(uint))
                val = BitConverter.ToUInt32(bytes, 0);
            else if (type == typeof(float))
                val = BitConverter.ToSingle(bytes, 0);
            else if (type == typeof(double))
                val = BitConverter.ToDouble(bytes, 0);
            else if (type == typeof(byte))
                val = bytes[0];
            else if (type == typeof(bool))
                if (bytes == null)
                    val = false;
                else
                    val = (bytes[0] != 0);
            else if (type == typeof(short))
                val = BitConverter.ToInt16(bytes, 0);
            else // probably a struct
            {
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                try { val = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type); }
                finally { handle.Free(); }
            }

            return val;
        }
    }
    public class Library
    {

        private static readonly string? _configPath = ConfigurationManager.AppSettings["Config"];
        private static readonly Dictionary<string, string> _keyValuePairs = new()
        {
            {"Bio", "RE1"},
            {"Bio2 1.10", "RE2"},
            {"bio2 chn claire", "RE2C"},
            {"BIOHAZARD(R) 3 PC", "RE3" },
            {"CVX PS2 US", "RECVX" }
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
                { "CLAIRE", "RE2C" },
                { "LEON", "RE2C" },
                { "leon", "RE2C" },
                { "bio2 chn claire", "RE2C" },
                { "bio2 chn leon", "RE2C" },
                {"Irregular1.8", "RE2C"},
                { "RE2", "RE2" },
                { "RE3", "RE3" },
                { "RECVX", "RECVX"},
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
                { "Bio3", "RE3" },
                { "CVX PS2 US", "RECVX" },
            };

        public static Dictionary<string, List<string>> _gameVersions = new Dictionary<string, List<string>>
        {
            { "Bio", new List<string> { "Bio", "bio", "Biohazard", "biohazard" } },
            { "bio2 1.10", new List<string> { "Bio2 1.10", "bio2 1.10", "bio2 1.1", "bio2", "bio2 v1.1", "bunny", "bunny2" , "re2mm", "RE2MM", "REVisited", "UEv3", "toos_ver2_0_0", "toos(hard)_ver2_0_0" } },
            { "bio2 chn claire", new List<string> { "CLAIRE", "claire", "bio2 chn claire" } },
            { "bio2 chn leon", new List<string> {"LEON", "leon", "Irregular1.8", "bio2 chn leon" } },
            { "BIOHAZARD(R) 3 PC", new List<string> { "BIOHAZARD(R) 3 PC","biohazard(r) 3 pc","Bio3", "bio3" } },
            { "CVX PS2 US", new List<string> { "pcsx2", "pcsx2-qt", "pcsx2-qtx64", "pcsx2-qtx64-avx2", "pcsx2x64", "pcsx2x64-avx2" } }
        };

        private static readonly Dictionary<string, string> _correctProccessName = new Dictionary<string, string>()
        {
            { "Bio", "Bio" },
            { "bio", "Bio" },
            { "Biohazard", "Bio" },
            { "biohazard", "Bio" },
            { "bio2", "Bio2 1.10" },
            { "Bio2", "Bio2 1.10" },
            { "Bio2 1.10", "Bio2 1.10" },
            { "bio2 1.10", "Bio2 1.10" },
            { "bio2 1.1", "Bio2 1.10" },
            { "bio2 v1.1", "Bio2 1.10" },
            { "Biohazard 2", "Bio2 1.10" },
            { "biohazard 2", "Bio2 1.10" },
            { "CLAIRE", "Bio2 chn claire" },
            { "leon", "Bio2 chn leon" },
            { "bio2 chn claire", "Bio2 chn claire" },
            { "bio2 chn leon", "Bio2 chn leon" },
            { "Irregular1.8", "Bio2 chn leon" },
            { "Irregular", "Bio2 chn leon" },
            { "Irregular2", "Bio2 chn leon" },
            { "Irregular2.0", "Bio2 chn leon" },
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
            { "Bio3", "BIOHAZARD(R) 3 PC" },
            { "pcsx2", "CVX PS2 US" },
            { "pcsx2-qt", "CVX PS2 US" },
            { "pcsx2-qtx64", "CVX PS2 US" },
            { "pcsx2-qtx64-avx2", "CVX PS2 US" },
            { "pcsx2x64", "CVX PS2 US" },
            { "pcsx2x64-avx2", "CVX PS2 US" }
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

            if (!File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath), "Configuration file path is invalid or missing.");
            }

            var json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                   ?? throw new ArgumentNullException("The game data is null");
        }

        public static Dictionary<string, bool> GetOptions()
        {
            var reJson = GetReviewerConfig();

            return new Dictionary<string, bool>
            {
                { "isBiorandMode", reJson["isBiorandMode"] == "true" },
                { "isHealthBarChecked", reJson["isHealthBarChecked"] == "true" },
                { "isItemBoxChecked", reJson["isItemBoxChecked"] == "true" },
                { "isChrisInventoryChecked", reJson["isChrisInventoryChecked"] == "true" },
                { "isSherryChecked", reJson["isSherryChecked"] == "true" },
                { "isMinimalistChecked", reJson["isMinimalistChecked"] == "true" },
                { "isNoSegmentsTimerChecked", reJson["isNoSegmentsTimerChecked"] == "true" },
                { "isNoStatsChecked", reJson["isNoStatsChecked"] == "true" },
                { "isNoKeyItemsChecked", reJson["isNoKeyItemsChecked"] == "true" },
                { "OneHPChallenge", reJson["OneHPChallenge"] == "true" },
                { "NoDamageChallenge", reJson["NoDamageChallenge"] == "true" },
                { "NoItemBoxChallenge", reJson["NoItemBoxChallenge"] == "true"},
                { "DebugMode", reJson["DebugMode"] == "true" }
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
            if (!File.Exists(_configPath))
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

            if (_gamePaths?.TryGetValue(gameKey, out string? gamePath) == true)
            {
                return gamePath;
            }

            Logger.Instance.Error($"Game key {gameKey} not found in gamePaths dictionary");
            return "";
        }

        public static Dictionary<byte, List<int>>? ConvertDictionnary(Dictionary<string, List<string>>? originalDict)
        {
            if (originalDict == null) return null;

            return originalDict
                .Where(pair => byte.TryParse(pair.Key, out _))
                .ToDictionary(
                    pair => byte.Parse(pair.Key),
                    pair => pair.Value
                        .Select(str => int.TryParse(str, out var num) ? num : (int?)null)
                        .Where(num => num.HasValue)
                        .Select(num => num.Value)
                        .ToList()
                );
        }

        public static string GetProcessMD5Hash(Process process)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(process.MainModule.FileName);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static int HexToInt(string hex)
        {
            return Convert.ToInt32(hex, 16);
        }

        public static string ToHexString(int value)
        {
            var hex = value.ToString("X8");
            return Regex.Replace(hex, ".{2}", "$0 ");
        }

    }
}
