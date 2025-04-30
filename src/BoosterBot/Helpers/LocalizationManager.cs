using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Configuration;

namespace BoosterBot.Helpers
{
    public class LocalizationManager
    {
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture = CultureInfo.InvariantCulture; 
        private readonly IConfiguration _configuration;

        public LocalizationManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

            _resourceManager = new ResourceManager("BoosterBot.Resources.Strings", typeof(LocalizationManager).Assembly);

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
                _currentCulture = CultureInfo.GetCultureInfo("en-US");
            }
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
        public IEnumerable<CultureInfo> GetSupportedCultures()
        {
            return new[]
            {
                CultureInfo.GetCultureInfo("en-US"),
                CultureInfo.GetCultureInfo("zh-CN")
            };
        }
    }
}