using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[TypeId("ed356467154b00b42be76150b4f42509")]
public class EquipmentRestrictionSpecialUnit : EquipmentRestriction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		return unit.Blueprint == Blueprint;
	}
}
