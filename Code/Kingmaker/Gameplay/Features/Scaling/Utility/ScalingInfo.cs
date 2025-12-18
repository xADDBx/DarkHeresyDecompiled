using Kingmaker.EntitySystem.Properties;
using Kingmaker.Localization;

namespace Kingmaker.Gameplay.Features.Scaling.Utility;

public readonly struct ScalingInfo
{
	public readonly PropertyCalculator Calculator;

	public readonly LocalizedString Description;

	public ScalingInfo(PropertyCalculator calculator, LocalizedString description)
	{
		Calculator = calculator;
		Description = description;
	}
}
