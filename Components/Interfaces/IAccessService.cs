using AmuleRemoteControl.Components.Data;

namespace AmuleRemoteControl.Components.Interfaces
{
    public interface IAccessService
    {
        bool IsAuthorized { get; set; }
        /// <summary>
        /// Event fired when authorization status changes.
        /// Follows standard EventHandler pattern.
        /// </summary>
        event EventHandler? OnChange;
        Task<string> CheckUrl();
        Task<bool> LoggedIn(LoginData loginData);
        Task LoggedOut();
    }
}
