namespace Kingmaker.Code.Middleware.Metrics;

public class RecruitMetricsEvent : MetricsEvent
{
	public enum CompanionRecruitStates
	{
		Recruit,
		Dismiss
	}

	protected override string Name => "recruitment";

	public RecruitMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public RecruitMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public RecruitMetricsEvent RecruitState(CompanionRecruitStates type)
	{
		AddParam("type", type switch
		{
			CompanionRecruitStates.Recruit => "recruit", 
			CompanionRecruitStates.Dismiss => "dismiss", 
			_ => MetricsEvent.EnumToSnakeCase(type), 
		});
		return this;
	}
}
