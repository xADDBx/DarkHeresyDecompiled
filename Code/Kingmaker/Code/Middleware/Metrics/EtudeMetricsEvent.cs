namespace Kingmaker.Code.Middleware.Metrics;

public class EtudeMetricsEvent : MetricsEvent
{
	public enum EtudeStates
	{
		Start,
		Complete
	}

	protected override string Name => "etude";

	public EtudeMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public EtudeMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public EtudeMetricsEvent EtudeState(EtudeStates state)
	{
		AddParam("state", state switch
		{
			EtudeStates.Start => "start", 
			EtudeStates.Complete => "complete", 
			_ => MetricsEvent.EnumToSnakeCase(state), 
		});
		return this;
	}
}
