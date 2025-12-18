using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItemEquipment))]
[AllowMultipleComponents]
[TypeId("65221a9a6133bd0408b019b86642d97e")]
public class AddFactToEquipmentWielder : BlueprintComponent
{
	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	public void Editor_SetFact(BlueprintUnitFact fact)
	{
		m_Fact = fact.ToReference<BlueprintUnitFactReference>();
	}
}
