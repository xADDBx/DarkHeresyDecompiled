namespace Kingmaker.Code.Middleware.Metrics;

public class GameOverMetricsEvent : MetricsEvent
{
	protected override string Name => "game_over";

	public GameOverMetricsEvent Reason(Player.GameOverReasonType reason)
	{
		AddParam("reason", reason switch
		{
			Player.GameOverReasonType.Won => "won", 
			Player.GameOverReasonType.PartyIsDefeated => "party_is_defeated", 
			Player.GameOverReasonType.EssentialUnitIsDead => "essential_unit_is_dead", 
			Player.GameOverReasonType.KingdomIsDestroyed => "kingdom_is_destroyed", 
			Player.GameOverReasonType.QuestFailed => "quest_failed", 
			_ => MetricsUtils.EnumToSnakeCase(reason), 
		});
		return this;
	}

	public GameOverMetricsEvent Difficulty(string difficulty)
	{
		AddParam("difficulty", difficulty);
		return this;
	}
}
