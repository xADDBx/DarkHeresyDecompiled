using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Visual.Critters;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("96520c14c04b949448dc4873eef92b0e")]
public class AddFamiliar : UnitFactComponentDelegate, IAreaHandler, ISubscriber
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnit.Reference m_Blueprint;

	public bool SpawnBaseUnit;

	public BlueprintUnit Unit => m_Blueprint;

	protected override void OnActivate()
	{
		TrySpawnFamiliar();
	}

	protected override void OnDeactivate()
	{
		TryDestroyFamiliar();
	}

	public void OnAreaBeginUnloading()
	{
		TryDestroyFamiliar(immediately: true);
	}

	public void OnAreaDidLoad()
	{
		TrySpawnFamiliar();
	}

	private void TrySpawnFamiliar()
	{
		if (!ContextData<UnitHelper.PreviewUnit>.Current && base.Owner.IsInGame && base.Owner.IsInState)
		{
			UnitPartFamiliarLeader orCreate = base.Owner.GetOrCreate<UnitPartFamiliarLeader>();
			AbstractUnitEntity abstractUnitEntity = orCreate.GetEquippedFamiliar(m_Blueprint, new EntityFactSource(base.Fact, this))?.Unit;
			if (abstractUnitEntity == null)
			{
				abstractUnitEntity = FamiliarHelper.SpawnFamiliar(base.Owner, Unit, SpawnBaseUnit);
				orCreate.AddEquippedFamiliar(abstractUnitEntity, new EntityFactSource(base.Fact, this));
			}
		}
	}

	private void TryDestroyFamiliar(bool immediately = false)
	{
		UnitPartFamiliarLeader orCreate = base.Owner.GetOrCreate<UnitPartFamiliarLeader>();
		AbstractUnitEntity abstractUnitEntity = orCreate.GetEquippedFamiliar(m_Blueprint, new EntityFactSource(base.Fact, this))?.Unit;
		if (abstractUnitEntity != null)
		{
			FamiliarHelper.DeSpawnFamiliar(base.Owner, abstractUnitEntity, immediately);
			orCreate.RemoveEquippedFamiliar(abstractUnitEntity);
		}
	}
}
