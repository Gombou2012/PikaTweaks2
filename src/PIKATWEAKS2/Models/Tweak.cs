namespace PIKATWEAKS2.Models;
public sealed record Tweak(string Id, string Name, string Description, string Category, string Command, bool Recommended = false, bool RequiresRestart = false, bool Maintenance = false);
