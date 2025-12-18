using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("a893455da46e3b54c8b935af823d32fd")]
public class ReplaceAsksList : UnitBuffComponentDelegate
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Asks")]
	private BlueprintUnitAsksListReference m_Asks;

	public BlueprintUnitAsksList Asks => m_Asks?.Get();

	protected override void OnActivate()
	{
		base.Owner.Asks.SetOverride(Asks);
		ObjectExtensions.Or(base.Owner.View, null)?.UpdateAsks();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Asks.SetOverride(null);
		ObjectExtensions.Or(base.Owner.View, null)?.UpdateAsks();
	}
}
