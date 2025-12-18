using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("68a754d320793344bbf8376833fa27e1")]
public class StarshipSpendUnusedAP : BlueprintComponent
{
	[SerializeField]
	private BlueprintBuffReference m_EvasionBuff;

	public BlueprintBuff EvasionBuff => m_EvasionBuff?.Get();
}
