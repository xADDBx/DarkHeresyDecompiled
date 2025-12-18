using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("AddAbilityRestriction")]
[AllowMultipleComponents]
[TypeId("9ed84940fa824243a3922d86ae07aadc")]
public class AbilitySourceLimitation : UnitFactComponentDelegate
{
	public WarhammerAbilityParamsSource Sources;
}
