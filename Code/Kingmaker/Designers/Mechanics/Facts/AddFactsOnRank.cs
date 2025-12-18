using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Facts And Buffs/AddFactsOnRank")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("1e5d3dccdc3e487e94d2a5ba37a598e3")]
public class AddFactsOnRank : MechanicEntityFactComponentDelegate
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public int PrevRankValue;
	}

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[KDB("Если true - ранг факта с этим компонентом должен быть строго равен Rank Value; если false - ранг факта с этим компонентом должен быть больше либо равен Rank Value")]
	[SerializeField]
	private bool m_StrictlyOnRank;

	[SerializeField]
	private ContextValue m_RankValue = 1;

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_Facts;

	public ContextValue RankValue => m_RankValue;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		Update();
	}

	protected override void OnDeactivate()
	{
		if (base.IsReapplying)
		{
			return;
		}
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			base.Owner.Facts.Remove(fact);
		}
		RequestTransientData<ComponentData>().PrevRankValue = 0;
	}

	private void Update()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		int rank = base.Fact.GetRank();
		int prevRankValue = componentData.PrevRankValue;
		if (rank == prevRankValue)
		{
			return;
		}
		componentData.PrevRankValue = rank;
		int num = m_RankValue.Calculate(base.Context);
		if (m_StrictlyOnRank)
		{
			if (rank != prevRankValue && rank == num && m_Restrictions.IsPassed(base.Context))
			{
				TryAddFacts();
			}
			else if (rank != num)
			{
				RemoveFacts();
			}
		}
		else if (rank > prevRankValue && rank >= num && m_Restrictions.IsPassed(base.Context))
		{
			TryAddFacts();
		}
		else if (rank < num)
		{
			RemoveFacts();
		}
	}

	private void RemoveFacts()
	{
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			if (base.Owner.Facts.Contains(fact))
			{
				base.Owner.Facts.Remove(fact);
			}
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
