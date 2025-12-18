using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d9a2dd1e7974488298c6f908d8182004")]
public class WarhammerDodgePenaltyAgainstCaster : UnitBuffComponentDelegate
{
	public ContextValue DodgePenalty;
}
