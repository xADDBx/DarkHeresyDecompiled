using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Code.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("81a5aaa95ba04ac3a228ff856a96e424")]
public class AddBuffOnCombatRandomEncounter : BlueprintComponent
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	private BlueprintBuff Buff => m_Buff?.Get();
}
