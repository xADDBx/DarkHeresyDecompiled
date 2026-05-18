namespace Kingmaker.Code.Middleware.Metrics;

public class QuestObjectiveMetricsEvent : MetricsEvent
{
	public enum States
	{
		Started,
		Completed,
		Failed
	}

	protected override string Name => "quest_objective";

	public QuestObjectiveMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public QuestObjectiveMetricsEvent QuestId(string quest_id)
	{
		AddParam("quest_id", quest_id);
		return this;
	}

	public QuestObjectiveMetricsEvent State(States state)
	{
		AddParam("state", state switch
		{
			States.Started => "started", 
			States.Completed => "completed", 
			States.Failed => "failed", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}
}
