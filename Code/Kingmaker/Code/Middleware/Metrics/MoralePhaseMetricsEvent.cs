using Kingmaker.Code.Gameplay.Parts;

namespace Kingmaker.Code.Middleware.Metrics;

public class MoralePhaseMetricsEvent : MetricsEvent
{
	protected override string Name => "morale_phase";

	public MoralePhaseMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public MoralePhaseMetricsEvent Phase(MoralePhaseType phase)
	{
		AddParam("phase", phase switch
		{
			MoralePhaseType.Regular => "regular", 
			MoralePhaseType.Heroic => "heroic", 
			MoralePhaseType.Broken => "broken", 
			_ => MetricsUtils.EnumToSnakeCase(phase), 
		});
		return this;
	}
}
