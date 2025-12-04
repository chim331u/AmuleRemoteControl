using System.ComponentModel.DataAnnotations;

namespace AmuleRemoteControl.Components.Data.AmuleModel
{
    public enum SearchType
    {
        [Display(Description = "Global")]
        Global,
        [Display(Description = "Local")]
        Local,
        [Display(Description = "Kad")]
        Kad
    }
}
