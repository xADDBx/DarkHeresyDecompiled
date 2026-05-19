using Kingmaker.GameInfo;
using Kingmaker.Networking;
using Kingmaker.Settings;
using Steamworks;

namespace Kingmaker.Code.Middleware.Metrics;

public class PlayerMetricsEvent : MetricsEvent
{
	protected override string Name => "player";

	protected override void AddSpecificEventDefaultParameters()
	{
		base.AddSpecificEventDefaultParameters();
		AddParam("coop", NetworkingManager.IsActive ? "coop" : "single");
		AddParam("difficulty", MetricsUtils.GameDifficultyToString(SettingsRoot.Difficulty.GameDifficulty.GetValue()));
		AddParam("controller", Game.Instance.IsControllerMouse ? "mouse" : "gamepad");
		AddParam("version", GameVersion.GetVersion());
		AddParam("steam_type", SteamUser.GetSteamID().GetEAccountType().ToString());
		AddParam("platform", "windows");
	}
}
