using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service
{
    public class AccessService : IAccessService
    {
        /// <summary>
        /// Thread safety lock object for IsAuthorized property.
        /// Prevents race conditions when multiple threads check or modify authorization state.
        /// </summary>
        private readonly object _authLock = new object();

        private bool isAuthorized;

        /// <summary>
        /// Gets or sets the authorization status. Thread-safe via lock.
        /// Setting this property triggers the OnChange event.
        /// </summary>
        public bool IsAuthorized
        {
            get
            {
                lock (_authLock)
                {
                    return isAuthorized;
                }
            }
            set
            {
                lock (_authLock)
                {
                    isAuthorized = value;
                    // Fire event inside lock to ensure consistency
                    NotifyStateChanged();
                }
            }
        }

        /// <summary>
        /// Event fired when authorization status changes.
        /// Follows standard EventHandler pattern for consistency across the application.
        /// </summary>
        public event EventHandler? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke(this, EventArgs.Empty);

        ILogger<AccessService> _logger;

        private readonly IUtilityServices _utilityServices;
        private readonly INetworkHelper _networkHelperServices;

        public AccessService(ILogger<AccessService> logger, IUtilityServices utilityServices, INetworkHelper networkHelperServices)
        {
            _logger = logger;
            _utilityServices = utilityServices;
            _networkHelperServices = networkHelperServices;
        }

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
