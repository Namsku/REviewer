using System;
using System.Collections.Generic;
using REviewer.Modules.Utils;

namespace REviewer.Services
{
    public class ConfigurationService
    {
        public Dictionary<string, string> GetGameList()
        {
            return Library.GetGameList();
        }

        public Dictionary<string, bool> GetOptions()
        {
            return Library.GetOptions();
        }

        public void UpdateConfig(string key, string value)
        {
            Library.UpdateConfigFile(key, value);
        }

        public Dictionary<string, string> GetReviewerConfig()
        {
            return Library.GetReviewerConfig();
        }
    }
}
