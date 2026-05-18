namespace Kingmaker.Code.Middleware.Metrics;

public class SettingsMetricsEvent : MetricsEvent
{
	protected override string Name => "settings";

	public SettingsMetricsEvent SettingKey(string key)
	{
		AddParam("key", key);
		return this;
	}

	public SettingsMetricsEvent From(string from_value)
	{
		AddParam("from_value", from_value);
		return this;
	}

	public SettingsMetricsEvent To(string to_value)
	{
		AddParam("to_value", to_value);
		return this;
	}
}
