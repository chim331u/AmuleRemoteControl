using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.Setting;

namespace AmuleRemoteControl.Components.Interfaces
{
    public interface IUtilityServices
    {
        //string GetRestUrl();
        Task CopyToClipboard(string text);
        string FormatAsEUR(object value);
        string FormatAsDate(object value);
        string FormatAsCurrency(double amountValue, string currency);
        string FormatAsGeneral(string value);

        Task WriteLog(string value);
        Task<string> GetLog();
        Task ClearLog();
        string FileSizeFormatted(double len);

        //string GetConfigValue(string key);
        //string SetRestUrl(string address, string port, string schema);

        IList<NetworkSetting> ReadNetworkSettingJson();
        bool WriteNetworkSettingJson(IList<NetworkSetting> settings);

        string ApiUrl { get; set; }
        string SetApiUrl();

        IList<GlobalSetting> ReadGlobalSettingJson();
        bool WriteGlobalSettingJson(IList<GlobalSetting> globalSettings);

        bool WriteLastLogin(DateTime lastLoginDateTime);
        DateTime ReadLastLogin();

        bool IsOnboardingCompleted();
        bool SetOnboardingCompleted();

        //login setting
        bool WriteLoginSettingJson(IList<GlobalSetting> settings);
        //IList<GlobalSetting> ReadLoginSettingJson();
        void SetLoginSettingFullPath();
        Task<bool> WriteLoginSettingData(LoginData loginData);
        Task<LoginData> ReadLoginSettingData();
        //bool UseBiometric();
        //bool SetBiometricLogin(bool isRemember);
        //bool SetBiometricLoginData(AuthRequest authRequest);
        //AuthRequest GetBiometricLogin();

        //crypto
        //string EncryptString(string plainText);
        //string DecryptString(string cipherText);
    }
}
