using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service
{
    public class AccessService : IAccessService
    {
        public bool IsAuthorized { get => isAuthorized; set { isAuthorized = value; NotifyStateChanged(); } }

        private bool isAuthorized;

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        ILogger<AccessService> _logger;

        private readonly IUtilityServices _utilityServices;
        private readonly INetworkHelper _networkHelperServices;

        public AccessService(ILogger<AccessService> logger, IUtilityServices utilityServices, INetworkHelper networkHelperServices)
        {
            _logger = logger;
            _utilityServices = utilityServices;
            _networkHelperServices = networkHelperServices;
        }

        //public async Task<bool> LoggedIn()
        //{
        //    var result = await _networkHelperServices.SendRequest("?pass=amule");
        //    //TODO retrive from setting

        //    if (result.Contains($"Enter password :"))
        //    {
        //        //login fail
        //        IsAuthorized = false;
        //        _logger.LogWarning("Login Fail");
        //        return false;

        //    }
        //    else
        //    {
        //        IsAuthorized = true;
        //        _logger.LogInformation("Logged in");
        //        return true;
        //    }
        //}        
        


        //public async Task<bool> LoggedIn(string psw)
        //{
        //    var result = await _networkHelperServices.SendRequest($"?pass={psw}");

        //    if (result.Contains($"Enter password :"))
        //    {
        //        //login fail
        //        IsAuthorized = false;
        //        _logger.LogWarning("Login Fail");
        //        return false;

        //    }
        //    else
        //    {
        //        IsAuthorized = true;
        //        _logger.LogInformation("Logged in");
        //        return true;
        //    }
        //}

        /// <summary>
        /// Log into amule - save login data
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns>True if loggedId, false if Login Fail</returns>
        public async Task<bool> LoggedIn(LoginData loginData)
        {
            var result = await _networkHelperServices.SendRequest($"?pass={loginData.Password}");

            if (result.Contains($"Enter password :"))
            {
                //login fail
                IsAuthorized = false;
                _logger.LogWarning("Login Fail");
                return false;

            }
            else
            {
                //logged in
                IsAuthorized = true;
                _logger.LogInformation("Logged in");
                
                //if (loginData.RememberPsw || loginData.UseBiometric)
                //{
                //    //write psw
                //    _utilityServices.WriteLoginSettingData(loginData);
                //}
                //else
                //{
                //    //psw null - no save
                //    loginData.Password = string.Empty;
                //    _utilityServices.WriteLoginSettingData(loginData);
                //}

                return true;
            }
        }

        public async Task LoggedOut()
        {
            var result = await _networkHelperServices.SendRequest("login.php");
            _utilityServices.WriteLastLogin(DateTime.MinValue);
            IsAuthorized = false;
        }

        public async Task<string> CheckUrl()
        {
            var result = await _networkHelperServices.SendRequest();

            return result;
        }
    }
}
