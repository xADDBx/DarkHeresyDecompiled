using Kingmaker.Framework.DetectiveSystem;

namespace Kingmaker.Code.Middleware.Metrics;

public class DetectiveCaseMetricsEvent : MetricsEvent
{
	protected override string Name => "detective_case";

	public DetectiveCaseMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public DetectiveCaseMetricsEvent State(CaseStatus state)
	{
		AddParam("state", state switch
		{
			CaseStatus.None => "none", 
			CaseStatus.Opened => "opened", 
			CaseStatus.Closed => "closed", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}

	public DetectiveCaseMetricsEvent Question(string question)
	{
		AddParam("question", question);
		return this;
	}

	public DetectiveCaseMetricsEvent Answer(string answer)
	{
		AddParam("answer", answer);
		return this;
	}
}
