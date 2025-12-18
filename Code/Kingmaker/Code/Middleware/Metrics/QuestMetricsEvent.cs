namespace Kingmaker.Code.Middleware.Metrics;

public class QuestMetricsEvent : MetricsEvent
{
	public enum States
	{
		Started,
		Completed,
		Failed
	}

	protected override string Name => "quest";

	public QuestMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public QuestMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public QuestMetricsEvent State(States state)
	{
		AddParam("state", state switch
		{
			States.Started => "started", 
			States.Completed => "completed", 
			States.Failed => "failed", 
			_ => MetricsEvent.EnumToSnakeCase(state), 
		});
		return this;
	}
}
