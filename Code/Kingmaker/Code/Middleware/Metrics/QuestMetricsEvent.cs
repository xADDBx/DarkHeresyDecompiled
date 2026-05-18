using Kingmaker.AreaLogic.QuestSystem;

namespace Kingmaker.Code.Middleware.Metrics;

public class QuestMetricsEvent : MetricsEvent
{
	protected override string Name => "quest";

	public QuestMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public QuestMetricsEvent State(QuestState state)
	{
		AddParam("state", state switch
		{
			QuestState.Started => "started", 
			QuestState.Completed => "completed", 
			QuestState.Failed => "failed", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}
}
