using System.Globalization;
using System.Text.Json;
using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.Setting;
using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AmuleRemoteControl.Components.Service
{
    public class UtilityServices : IUtilityServices
    {
        public string ApiUrl { get; set; }
        public string NetworkSettingFullPath { get; set; }
        public string LoginSettingFullPath { get; set; }
        public string globalSettingFullPath { get; set; }
        public string customSettingFullPath { get; set; }


        private readonly IConfiguration _config;
        private readonly IJSRuntime _runtime;
        private ILogger<UtilityServices> _logger;

        private readonly SessionStorageAccessor SessionStorageAccessor;

        public UtilityServices(IConfiguration config, IJSRuntime jSRuntime, SessionStorageAccessor sessionStorageAccessor, ILogger<UtilityServices> logger)
        {
            _config = config;
            _runtime = jSRuntime;
            SessionStorageAccessor = sessionStorageAccessor;
            _logger = logger;

            globalSettingFullPath = Path.Combine(FileSystem.Current.AppDataDirectory, "GlobalSettings.json");
            customSettingFullPath = Path.Combine(FileSystem.Current.AppDataDirectory, "CustomSettings.json");

            SetNetworkSettingFullPath();
            SetLoginSettingFullPath();
            SetApiUrl();
        }

        #region Format

        public string FormatAsEUR(object value)
        {
            if (value == null)
            {
                return "00";
            }

            return ((double)value).ToString("C0", CultureInfo.CreateSpecificCulture("it-IT"));
        }

        public string FormatAsGeneral(string value)
        {
            if (value == null)
            {
                return "0";
            }

            try
            {
                var number = Convert.ToDouble(value);
                return number.ToString("N0", CultureInfo.CreateSpecificCulture("it-IT"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail to convert as general format: {ex.Message}");

                return value;
            }


        }

        public string FormatAsDate(object value)
        {
            if (value != null)
            {
                return Convert.ToDateTime(value).ToString("MMM yyyy");
            }

            return "--";
        }

        public string FormatAsCurrency(double amountValue, string currency)
        {
            switch (currency)
            {
                case "EUR":
                    return ((double)amountValue).ToString("C0", CultureInfo.CreateSpecificCulture("it-IT"));

                case "CHF":
                    return ((double)amountValue).ToString("C0", CultureInfo.CreateSpecificCulture("ch-CH"));


                default:
                    return ((double)amountValue).ToString("C0", CultureInfo.CreateSpecificCulture("us-US"));

            }


        }

        public string FileSizeFormatted(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };

            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = string.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        #endregion

        #region Setting

        //Network Settings

        public IList<NetworkSetting> ReadNetworkSettingJson()
        {
            if (string.IsNullOrEmpty(NetworkSettingFullPath))
            {
                _logger.LogWarning($"Network Setting path must be valued");
                return null;
            }

            if (!File.Exists(NetworkSettingFullPath))
            {
                _logger.LogWarning($"Network Setting file do not exist.");

                if (WriteNetworkSettingJson(CreateDefaultNetworkSetting()))
                {
                    _logger.LogInformation($"Network Setting file Added.");
                }
                else
                {
                    _logger.LogInformation($"No Network Setting file to read.");
                    return null;
                }
            }

            var _settings = JsonSerializer.Deserialize<List<NetworkSetting>>(File.ReadAllText(NetworkSettingFullPath));

            return _settings;

        }

        public bool WriteNetworkSettingJson(IList<NetworkSetting> settings)
        {
            if (string.IsNullOrEmpty(NetworkSettingFullPath))
            {
                _logger.LogWarning($"Network Setting path must be valued");
                return false;
            }

            try
            {
                File.WriteAllText(NetworkSettingFullPath, JsonSerializer.Serialize(settings));
                _logger.LogInformation($"Network Setting file saved");
                SetApiUrl();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Network Setting file: {ex.Message}");
                return false;
            }
        }

        public string SetApiUrl()
        {

            var _settingList = ReadNetworkSettingJson();

            var activeSetting = _settingList.Where(x => x.IsActive).FirstOrDefault();

            if (activeSetting != null)
            {
                ApiUrl = $"http://{activeSetting.Address}:{activeSetting.Port}/";

            }
            else
            {
                var firstSetting = _settingList.FirstOrDefault();
                ApiUrl = $"http://{firstSetting.Address}:{firstSetting.Port}/";
            }

            _logger.LogInformation($"Api Url: {ApiUrl}");
            return ApiUrl;
        }

        private IList<NetworkSetting> CreateDefaultNetworkSetting()
        {
            var networks = new List<NetworkSetting>();

            networks.Add(new NetworkSetting
            {
                Name = "AmuleWeb",
                Address = "myLocalaMuleAddress",
                Port = "4711",
                IsActive = true
            });

            networks.Add(new NetworkSetting
            {
                Name = "AmuleWebExternal",
                Address = "myRemoteMuleAddress",
                Port = "4712",
                IsActive = false
            });

            _logger.LogInformation($"Created Default network setting");
            return networks;

        }

        private string GetNetworkSettingJsonFullPath()
        {
            var _networkSettingFileName = ReadGlobalSettingJson().Where(x => x.Key == "NetworkSettingFileName").FirstOrDefault().Value;
            return Path.Combine(FileSystem.Current.AppDataDirectory, _networkSettingFileName);
        }

        public void SetNetworkSettingFullPath()
        {
            NetworkSettingFullPath = GetNetworkSettingJsonFullPath();
        }

        //Global Settings

        public IList<GlobalSetting> ReadGlobalSettingJson()
        {
            //File.Delete(globalSettingFullPath);

            if (string.IsNullOrEmpty(globalSettingFullPath))
            {
                _logger.LogWarning($"Global path must be valued");
                return null;
            }

            if (!File.Exists(globalSettingFullPath))
            {
                _logger.LogWarning($"Global setting file do not exist.");

                if (WriteGlobalSettingJson(CreateDefaultGlobalSetting()))
                {
                    _logger.LogInformation($"Global Setting file Added.");
                }
                else
                {
                    _logger.LogInformation($"No Global Setting file to read.");
                    return null;
                }
            }

            var _globalSettings = JsonSerializer.Deserialize<List<GlobalSetting>>(File.ReadAllText(globalSettingFullPath));

            return _globalSettings;

        }

        public bool WriteLastLogin(DateTime lastLoginDateTime)
        {
            if (string.IsNullOrEmpty(customSettingFullPath))
            {
                _logger.LogWarning($"Custom Setting path must be valued");
                return false;
            }

            try
            {
                var _customSettings = JsonSerializer.Deserialize<List<GlobalSetting>>(File.ReadAllText(customSettingFullPath));

                if (_customSettings.Any(x=>x.Key.Equals("LastLoginDateTime")))
                {
                    _customSettings.Where(x => x.Key.Equals("LastLoginDateTime")).FirstOrDefault().Value = lastLoginDateTime.ToString();
                }
                else
                {
                    _customSettings.Add(new GlobalSetting { Key = "LastLoginDateTime", Value = lastLoginDateTime.ToString() });
                }

                File.WriteAllText(customSettingFullPath, JsonSerializer.Serialize(_customSettings));
                _logger.LogInformation($"Last login saved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Last Login: {ex.Message}");
                return false;
            }
        }

        public DateTime ReadLastLogin()
        {

            if (string.IsNullOrEmpty(customSettingFullPath))
            {
                _logger.LogWarning($"Custom path must be valued");

            }

            if (!File.Exists(customSettingFullPath))
            {
                _logger.LogWarning($"Custom setting file do not exist.");

                if (WriteCustomSettingJson(CreateDefaultCustomSetting()))
                {
                    _logger.LogInformation($"Custom Setting file Added.");
                }
                else
                {
                    _logger.LogInformation($"No Custom Setting file to read.");
                    return DateTime.MinValue;
                }
            }

            try
            {
                var _globalSettings = JsonSerializer.Deserialize<List<GlobalSetting>>(File.ReadAllText(customSettingFullPath));

                var _lastLogin = _globalSettings.Where(x => x.Key.Equals("LastLoginDateTime")).FirstOrDefault();

                return Convert.ToDateTime(_lastLogin.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading last logged date: {ex.Message}");
            }

            return DateTime.MinValue;

        }

        public bool WriteGlobalSettingJson(IList<GlobalSetting> globalSettings)
        {
            if (string.IsNullOrEmpty(globalSettingFullPath))
            {
                _logger.LogWarning($"Global Setting path must be valued");
                return false;
            }

            try
            {
                File.WriteAllText(globalSettingFullPath, JsonSerializer.Serialize(globalSettings));
                _logger.LogInformation($"Global settings file saved");
                SetNetworkSettingFullPath();
                SetApiUrl();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Global Settings file: {ex.Message}");
                return false;
            }
        }        
        
        public bool WriteCustomSettingJson(IList<GlobalSetting> globalSettings)
        {
            if (string.IsNullOrEmpty(customSettingFullPath))
            {
                _logger.LogWarning($"Custom Setting path must be valued");
                return false;
            }

            try
            {
                File.WriteAllText(customSettingFullPath, JsonSerializer.Serialize(globalSettings));
                _logger.LogInformation($"Custom settings file saved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Custom Settings file: {ex.Message}");
                return false;
            }
        }

        private IList<GlobalSetting> CreateDefaultGlobalSetting()
        {
            var _globalSettings = new List<GlobalSetting>();

            _globalSettings.Add(new GlobalSetting { Key = "NetworkSettingFileName", Value = "NetworkSetting.json" });
            _globalSettings.Add(new GlobalSetting { Key = "LoginSettings", Value = "LoginSettings.json" });

            _logger.LogInformation($"Created Global setting file");
            return _globalSettings;

        }

        private IList<GlobalSetting> CreateDefaultCustomSetting()
        {
            var _globalSettings = new List<GlobalSetting>();

            _globalSettings.Add(new GlobalSetting { Key = "LastLoginDateTime", Value = DateTime.MinValue.ToString() });

            _logger.LogInformation($"Created Custom setting file");
            return _globalSettings;

        }

        //LoginSettings

        public bool WriteLoginSettingJson(IList<GlobalSetting> settings)
        {
            if (string.IsNullOrEmpty(LoginSettingFullPath))
            {
                _logger.LogWarning($"Login file path must be valued");
                return false;
            }

            try
            {
                File.WriteAllText(LoginSettingFullPath, JsonSerializer.Serialize(settings));
                _logger.LogInformation($"Login file saved");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Login file: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> WriteLoginSettingData(LoginData loginData)
        {
            if (string.IsNullOrEmpty(LoginSettingFullPath))
            {
                _logger.LogWarning($"Login setting path must be valued");
                return false;
            }
            _logger.LogInformation("Path is present");
            try
            {
                if (!string.IsNullOrEmpty(loginData.Password))
                {
                    //loginData.Password = EncryptString(loginData.Password);
                    if (await SetSavedPassword(loginData.Password))
                    {
                        loginData.Password = string.Empty;
                    }
                }
                _logger.LogInformation("Writing loggin settings on file...");

                try
                {
                    File.WriteAllText(LoginSettingFullPath, JsonSerializer.Serialize(loginData));
                    _logger.LogInformation($"Login data saved");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error writing file: {ex.Message}");
                    return false;
                }                             


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Login setting file: {ex.Message}");
                return false;
            }
        }
               
        public async Task<LoginData> ReadLoginSettingData()
        {
            _logger.LogInformation($"Path: {LoginSettingFullPath}");

            if (string.IsNullOrEmpty(LoginSettingFullPath))
            {
                _logger.LogWarning($"Login path must be valued");
                return null;
            }

            if (!File.Exists(LoginSettingFullPath))
            {
                _logger.LogWarning($"Login setting file do not exist.");
                //TODO add json file
                if (await WriteLoginSettingData(new LoginData()))
                {
                    _logger.LogInformation($"Login Setting file Created.");
                }
                else
                {
                    _logger.LogInformation($"No Login Setting Json file to read.");
                    return null;
                }
                _logger.LogError($"No Login Setting file to read.");
                return null;
            }

            var _loginSettings = JsonSerializer.Deserialize<LoginData>(File.ReadAllText(LoginSettingFullPath));

            if (_loginSettings != null)
            {
                //_logger.LogInformation($"Psw: {_loginSettings.Password}");

                //_loginSettings.Password = DecryptString(_loginSettings.Password);
                _loginSettings.Password = await GetSavedPassword();
            }

            return _loginSettings;

        }

        private string GetLoginSettingJsonFullPath()
        {
            var _loginSettingFileName = ReadGlobalSettingJson().Where(x => x.Key == "LoginSettings").FirstOrDefault().Value;
            return Path.Combine(FileSystem.Current.AppDataDirectory, _loginSettingFileName);
        }

        public void SetLoginSettingFullPath()
        {
            LoginSettingFullPath = GetLoginSettingJsonFullPath();
        }

        private IList<GlobalSetting> CreateDefaultLoginSetting()
        {
            var logins = new List<GlobalSetting>();

            logins.Add(new GlobalSetting { Key = "UseBiometric", Value = "false" });

            _logger.LogInformation($"Created Default login setting");
            return logins;

        }

        #endregion

        #region Security

        protected async Task<string?> GetSavedPassword()
        {
            return await SecureStorage.Default.GetAsync("amule_psw");
        }

        protected async Task<bool> SetSavedPassword(string selectedPassword)
        {
            try
            {
                await SecureStorage.Default.SetAsync("amule_psw", selectedPassword);
                _logger.LogInformation("Password saved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR in saving password: {ex.Message}");
                return false;
            }

        }

        #endregion

        public async Task CopyToClipboard(string text)
        {
            try
            {
                //await _runtime.InvokeVoidAsync("navigator.clipboard.writeText", text);

                await Clipboard.Default.SetTextAsync(text);
            }
            catch (Exception ex)
            {

            }

        }

        #region Log
        public async Task WriteLog(string value)
        {
            var _text = string.Concat(DateTime.Now.ToString(), " - ", value, Environment.NewLine);
            var log = string.Concat(await SessionStorageAccessor.GetValueAsync<string>("LOG"), _text);

            await SessionStorageAccessor.SetValueAsync("LOG", log);
        }

        public async Task<string> GetLog()
        {
            var StoredValue = await SessionStorageAccessor.GetValueAsync<string>("LOG");
            return StoredValue;

        }

        public async Task ClearLog()
        {
            await SessionStorageAccessor.RemoveAsync("LOG");

        }

        #endregion


    }
}
