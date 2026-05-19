namespace Kingmaker.Code.Middleware.Metrics;

public class DialogMetricsEvent : MetricsEvent
{
	public enum DialogState
	{
		Open,
		Close
	}

	protected override string Name => "dialog";

	public DialogMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public DialogMetricsEvent State(DialogState state)
	{
		AddParam("state", state switch
		{
			DialogState.Open => "open", 
			DialogState.Close => "close", 
			_ => MetricsUtils.EnumToSnakeCase(state), 
		});
		return this;
	}
}
