using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("98fa0fcd1b23fec4e850cc468f0b01b8")]
public class EquipmentRestrictionPlayerHasFacts : EquipmentRestriction
{
	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts = new BlueprintUnitFactReference[0];

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		return Facts.Any((BlueprintUnitFact p) => GameHelper.GetPlayerCharacter().Facts.Contains(p));
	}
}
