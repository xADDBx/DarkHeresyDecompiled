using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4628eabc04834caabc2fcd52e1bc9f3b")]
public class WarhammerDodgeAndParryBonus : BlueprintComponent
{
	public ContextValue DodgeBonus;

	public ContextValue ParryBonus;
}
