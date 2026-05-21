namespace Kingmaker.Code.Middleware.Metrics;

public class ChargenMetricsEvent : MetricsEvent
{
	protected override string Name => "chargen";

	public ChargenMetricsEvent Tab(string tab)
	{
		AddParam("tab", tab);
		return this;
	}

	public ChargenMetricsEvent Next(string next)
	{
		AddParam("next", next);
		return this;
	}

	public ChargenMetricsEvent Back(string back)
	{
		AddParam("back", back);
		return this;
	}

	public ChargenMetricsEvent First(string first)
	{
		AddParam("first", first);
		return this;
	}

	public ChargenMetricsEvent Last(string last)
	{
		AddParam("last", last);
		return this;
	}

	public ChargenMetricsEvent Select(string select)
	{
		AddParam("select", select);
		return this;
	}

	public ChargenMetricsEvent Close(string close)
	{
		AddParam("close", close);
		return this;
	}

	public ChargenMetricsEvent Finish()
	{
		AddParam("finish", "finish");
		return this;
	}
}
