using Kingmaker.Settings;

namespace Kingmaker.Code.Middleware.Metrics;

public class NewGameEvent : MetricsEvent
{
	protected override string Name => "new_game";

	public NewGameEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public NewGameEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public NewGameEvent Difficulty(GameDifficultyOption difficulty)
	{
		AddParam("difficulty", difficulty switch
		{
			GameDifficultyOption.Custom => "custom", 
			GameDifficultyOption.Story => "story", 
			GameDifficultyOption.Casual => "casual", 
			GameDifficultyOption.Normal => "normal", 
			GameDifficultyOption.Daring => "daring", 
			GameDifficultyOption.Core => "core", 
			GameDifficultyOption.Hard => "hard", 
			GameDifficultyOption.Unfair => "unfair", 
			_ => MetricsEvent.EnumToSnakeCase(difficulty), 
		});
		return this;
	}

	public NewGameEvent Preset(string preset)
	{
		AddParam("preset", preset);
		return this;
	}
}
