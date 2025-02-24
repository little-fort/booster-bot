using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace BoosterBot.Helpers
{
    public class LocalizationManager
    {
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;
        private readonly IConfiguration _configuration;

        public LocalizationManager(IConfiguration configuration)
        {
            // 确保 configuration 不为 null
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

            // Assumes resources are in a .resx file in the Resources folder
            _resourceManager = new ResourceManager("BoosterBot.Resources.Strings", typeof(LocalizationManager).Assembly);

            // Get culture from settings, default to English if not specified
            string cultureName = _configuration["appLanguage"] ?? "en-US";
            SetCulture(cultureName);
        }

        public string GetCulture() => _currentCulture.TwoLetterISOLanguageName;

        public void SetCulture(string cultureName)
        {
            try
            {
                _currentCulture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                // Fallback to English if culture not found
                _currentCulture = CultureInfo.GetCultureInfo("en-US");
            }

            // Set current thread culture (important for console apps)
            CultureInfo.CurrentUICulture = _currentCulture;
            CultureInfo.CurrentCulture = _currentCulture;
        }

        public string GetString(string key)
        {
            try
            {
                string? value = _resourceManager.GetString(key, _currentCulture);
                return value ?? key;
            }
            catch (MissingManifestResourceException)
            {
                return $"[Resource Error: {key}]";
            }
        }

        public string GetNeutralString(string key)
        {
            try
            {
                string? value = _resourceManager.GetString(key, CultureInfo.GetCultureInfo("en-US"));
                return value ?? key;
            }
            catch (MissingManifestResourceException)
            {
                return $"[Resource Error: {key}]";
            }
        }

        // Optional: Method to get all supported cultures
        public IEnumerable<CultureInfo> GetSupportedCultures()
        {
            // You would need to maintain this list based on your available .resx files
            return new[]
            {
                CultureInfo.GetCultureInfo("en-US"),
                CultureInfo.GetCultureInfo("zh-CN")
            };
        }
    }
}