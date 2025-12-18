using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d000fc1491094c8e93f912c02cb86ecc")]
public class FreeUltimateBuff : UnitBuffComponentDelegate
{
	[SerializeField]
	private bool m_NoMomentumCost;

	public bool NoMomentumCost => m_NoMomentumCost;
}
