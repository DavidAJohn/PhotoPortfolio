namespace PhotoPortfolio.Shared.Models;

public enum LightboxConfirmAction
{
    Navigate, // 0
    NavigateForceLoad, // 1
    CloseOnly // 2
}

public class LightboxConfirmOptions
{
    public bool ShowButton { get; set; } = true;
    public string ButtonText { get; set; } = "Confirm";
    public LightboxConfirmAction ButtonAction { get; set; } = LightboxConfirmAction.CloseOnly;
    public string NavigateUri { get; set; } = "#";
}
