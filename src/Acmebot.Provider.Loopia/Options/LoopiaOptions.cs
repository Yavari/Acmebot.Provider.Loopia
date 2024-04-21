namespace Acmebot.Provider.Loopia.Options;

public record LoopiaOptions(string Username, string Password)
{
    public const string Option = "Loopia";
}