using System.Collections.Generic;

namespace REviewer.Core.Constants
{
    public static class GameConstants
    {
        // Game Identifiers
        public const int BIOHAZARD_1 = 100;
        public const int BIOHAZARD_2 = 200;
        public const int BIOHAZARD_3 = 300;
        public const int BIOHAZARD_CVX = 400;

        // Game Selection Indices
        public const int BIOHAZARD_1_MK = 0;
        public const int BIOHAZARD_2_SC = 1;
        public const int BIOHAZARD_2_PC = 2;
        public const int BIOHAZARD_2_PL = 3;
        public const int BIOHAZARD_3_RB = 4;
        public const int BIOHAZARD_3_CH = 5;
        public const int BIOHAZARD_CV_X = 6;

        // Emulator Process Names
        public const string RPCS3 = "rpcs3";
        public const string PCSX2 = "pcsx2";
        public const string PCSX2QT = "pcsx2-qt";
        public const string PCSX264QT = "pcsx2-qtx64";
        public const string PCSX264QTAV = "pcsx2-qtx64-avx2";
        public const string PCSX264WX = "pcsx2x64";
        public const string PCSX264WXAV = "pcsx2x64-avx2";
        public const string DOLPHIN = "dolphin";

        // Game Names
        public static readonly IReadOnlyList<string> GameList = new[]
        {
            "Bio", "bio2 1.10", "bio2 chn claire", "bio2 chn leon",
            "BIOHAZARD(R) 3 PC", "Bio3 CHN/TWN", "CVX PS2 US"
        };

        public static readonly IReadOnlyList<string> GameSelection = new[]
        {
            "RE1", "RE2", "RE2C", "RE2C", "RE3", "RE3C", "RECVX"
        };

        // Monitoring Interval
        public const int MONITORING_INTERVAL_MS = 55;
    }
}
