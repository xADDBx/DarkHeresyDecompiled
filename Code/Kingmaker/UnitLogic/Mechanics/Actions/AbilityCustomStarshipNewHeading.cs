using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("5071af137ae772f459a84bfb952f644f")]
public class AbilityCustomStarshipNewHeading : BlueprintComponent
{
	[SerializeField]
	private BlueprintBuffReference m_VariationBuff;

	[SerializeField]
	private BuffEndCondition buffDuration = BuffEndCondition.CombatEnd;

	[SerializeField]
	private ActionList Actions;

	public BlueprintBuff VariationBuff => m_VariationBuff?.Get();
}
