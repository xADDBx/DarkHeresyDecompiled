using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete("Unused")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Combat/BonusActionPoints")]
[TypeId("54ca5dc35f1c41189b3bf1833ae2b98f")]
public class BonusActionPoints : UnitFactComponentDelegate
{
	public int MaxPointsBonus;

	public int RegenBonus;

	public bool SetUpperLimit;

	[ShowIf("SetUpperLimit")]
	public int UpperLimitValue = 1000;

	public ContextValue MaxPointsValue = new ContextValue();

	public ContextValue RegenValue = new ContextValue();
}
