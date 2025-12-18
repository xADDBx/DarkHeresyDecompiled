using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;

namespace Kingmaker.RuleSystem.Rules.Interfaces;

public struct RerollData
{
	public readonly IMechanicEntityFact Source;

	public RerollData(IMechanicEntityFact source)
	{
		Source = source;
	}
}
