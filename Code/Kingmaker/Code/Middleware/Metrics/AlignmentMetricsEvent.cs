using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.Code.Middleware.Metrics;

public class AlignmentMetricsEvent : MetricsEvent
{
	protected override string Name => "alignment";

	public AlignmentMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public AlignmentMetricsEvent Axis(AlignmentAxis axis)
	{
		AddParam("axis", axis switch
		{
			AlignmentAxis.Monodominance => "monodominance", 
			AlignmentAxis.None => "none", 
			AlignmentAxis.Torian => "torian", 
			AlignmentAxis.Xanthite => "xanthite", 
			AlignmentAxis.Xenophilia => "xenophilia", 
			_ => MetricsUtils.EnumToSnakeCase(axis), 
		});
		return this;
	}

	public AlignmentMetricsEvent Shift(int shift)
	{
		AddParam("shift", shift.ToString());
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

	public AlignmentMetricsEvent Mark(int mark)
	{
		AddParam("mark", mark.ToString());
		return this;
	}
}
