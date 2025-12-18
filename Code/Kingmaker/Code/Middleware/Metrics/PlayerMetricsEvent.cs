using Kingmaker.GameInfo;
using Kingmaker.Networking;
using Kingmaker.Settings;

namespace Kingmaker.Code.Middleware.Metrics;

public class PlayerMetricsEvent : MetricsEvent
{
	protected override string Name => "player";

	public PlayerMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	protected override void AddSpecificEventDefaultParameters()
	{
		base.AddSpecificEventDefaultParameters();
		AddParam("coop", NetworkingManager.IsActive ? "coop" : "single");
		AddParam("difficulty", GameDifficultyToString(SettingsController.Instance.DifficultySettingsController.ExtractFromSettings().GameDifficulty));
		AddParam("controller", Game.Instance.IsControllerMouse ? "mouse" : "gamepad");
		AddParam("version", GameVersion.GetVersion());
		AddParam("platform", "windows");
	}

	public static string GameDifficultyToString(GameDifficultyOption difficulty)
	{
		return difficulty switch
		{
			GameDifficultyOption.Casual => "casual", 
			GameDifficultyOption.Core => "core", 
			GameDifficultyOption.Custom => "custom", 
			GameDifficultyOption.Daring => "daring", 
			GameDifficultyOption.Hard => "hard", 
			GameDifficultyOption.Normal => "normal", 
			GameDifficultyOption.Story => "story", 
			GameDifficultyOption.Unfair => "unfair", 
			_ => MetricsEvent.EnumToSnakeCase(difficulty), 
		};
	}
}
