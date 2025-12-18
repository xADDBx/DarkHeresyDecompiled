using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.Code.Middleware.Metrics;

public class AlignmentMetricsEvent : MetricsEvent
{
	protected override string Name => "alignment";

	public AlignmentMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public AlignmentMetricsEvent Type(AlignmentAxis type)
	{
		AddParam("type", type switch
		{
			AlignmentAxis.Monodominance => "monodominance", 
			AlignmentAxis.None => "none", 
			AlignmentAxis.Torian => "torian", 
			AlignmentAxis.Xanthite => "xanthite", 
			AlignmentAxis.Xenophilia => "xenophilia", 
			_ => MetricsEvent.EnumToSnakeCase(type), 
		});
		return this;
	}

	public AlignmentMetricsEvent Value(int value)
	{
		AddParam("value", value.ToString());
		return this;
	}

	public AlignmentMetricsEvent CharacterLevel(int level)
	{
		AddParam("level", level.ToString());
		return this;
	}
}
