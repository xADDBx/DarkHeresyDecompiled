using System;

namespace Kingmaker.Code.Middleware.Metrics;

public class SettingsMetricsEvent : MetricsEvent
{
	public enum SettingTypes
	{
		Sound,
		AnimationSpeed,
		GameDifficulty
	}

	protected override string Name => "settings";

	public SettingsMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	protected override void AddSpecificEventDefaultParameters()
	{
		base.AddSpecificEventDefaultParameters();
		if (!Game.IsInMainMenu)
		{
			AddParam("game_id", Game.Instance.Player.GameId);
			AddParam("time_game", Convert.ToInt64(Game.Instance.Controllers.TimeController.GameTime.TotalMilliseconds).ToString());
		}
	}

	public SettingsMetricsEvent SettingType(SettingTypes type)
	{
		AddParam("type", type switch
		{
			SettingTypes.Sound => "sound", 
			SettingTypes.AnimationSpeed => "animation_speed", 
			SettingTypes.GameDifficulty => "difficulty", 
			_ => MetricsEvent.EnumToSnakeCase(type), 
		});
		return this;
	}

	public SettingsMetricsEvent Value(string value)
	{
		AddParam("value", value);
		return this;
	}
}
