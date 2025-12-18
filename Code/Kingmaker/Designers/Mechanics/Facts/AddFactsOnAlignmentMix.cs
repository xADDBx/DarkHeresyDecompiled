using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Facts And Buffs/AddFactsOnAlignmentMix")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("e8a12138c7c94153af51573f69a067e7")]
public class AddFactsOnAlignmentMix : MechanicEntityFactComponentDelegate, IAlignmentReachMixHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	[SerializeField]
	private AlignmentMix Mix;

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_Facts;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	public void HandleAlignmentReachedMix(AlignmentMix mix)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity == base.Owner && mix == Mix)
		{
			TryAddFacts();
		}
	}

	private void TryAddFacts()
	{
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			if (!base.Owner.Facts.Contains(fact))
			{
				EntityFact entityFact = base.Owner.AddFact(fact);
				if (entityFact == null)
				{
					break;
				}
				entityFact.AddSource(base.Fact, this);
			}
		}
	}
}
