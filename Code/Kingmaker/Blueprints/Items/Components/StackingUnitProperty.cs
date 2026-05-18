using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintItemEquipment))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("fb8ad0331874456ab1aaac562c259879")]
public class StackingUnitProperty : BlueprintComponent
{
	[SerializeField]
	private BlueprintStackingUnitProperty.Reference m_Property;

	[SerializeField]
	private ContextValue m_Value;

	public BlueprintStackingUnitProperty Property => m_Property?.Get();

	public int GetValue(IEvalContext context)
	{
		return m_Value.Calculate(context);
	}
}
