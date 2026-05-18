namespace Kingmaker.Code.Middleware.Metrics;

public class ChapterMetricsEvent : MetricsEvent
{
	public enum ChapterStates
	{
		Start,
		Finish
	}

	protected override string Name => "chapter";

	public ChapterMetricsEvent Number(string number)
	{
		AddParam("number", number);
		return this;
	}

	public ChapterMetricsEvent ChapterState(ChapterStates state)
	{
		AddParam("state", state switch
		{
			ChapterStates.Start => "start", 
			ChapterStates.Finish => "finish", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}
}
