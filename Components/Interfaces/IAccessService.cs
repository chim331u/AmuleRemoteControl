using AmuleRemoteControl.Components.Data;

namespace AmuleRemoteControl.Components.Interfaces
{
    public interface IAccessService
    {
        bool IsAuthorized { get; set; }
        event Action? OnChange;
        Task<string> CheckUrl();
        //Task<bool> LoggedIn();
        //Task<bool> LoggedIn(string psw);
        Task<bool> LoggedIn(LoginData loginData);
        Task LoggedOut();
    }
}
