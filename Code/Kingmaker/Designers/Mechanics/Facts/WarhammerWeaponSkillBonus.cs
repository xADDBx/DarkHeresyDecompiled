using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("b9754822222d431189bcbfe02218fc6c")]
public class WarhammerWeaponSkillBonus : BlueprintComponent
{
	public ContextValue WeaponSkillBonus;

	public bool BonusAgainstCastersPriorityTarget;

	public bool BonusAgainstAlliedCastersPriorityTarget;

	[ShowIf("PriorityTarget")]
	private BlueprintBuffReference m_TargetBuff;

	[UsedImplicitly]
	private bool PriorityTarget
	{
		get
		{
			if (!BonusAgainstCastersPriorityTarget)
			{
				return BonusAgainstAlliedCastersPriorityTarget;
			}
			return true;
		}
	}

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();
}
