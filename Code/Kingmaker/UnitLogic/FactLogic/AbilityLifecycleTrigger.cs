using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowMultipleComponents]
[TypeId("dba9dbd41a904614809881497ddebe3b")]
public abstract class AbilityLifecycleTrigger : MechanicEntityFactComponentDelegate
{
	private class TransientData : IEntityFactComponentTransientData
	{
		public readonly List<string> AppliedFacts = new List<string>();
	}

	public PropertyCalculator Condition;

	public ActionList StartActions;

	public ActionList EndActions;

	[InfoBox("Факты, которые будут висеть на кастере на всём времени выполнения абилки")]
	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintMechanicEntityFact.Reference[] m_Facts = new BlueprintMechanicEntityFact.Reference[0];

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected void RunStartActions(AbilityExecutionContext context)
	{
		if (Condition.Any && !CheckCondition(context))
		{
			return;
		}
		TransientData transientData = RequestTransientData<TransientData>();
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			EntityFact entityFact = base.Owner.AddFact(fact, base.Fact.MaybeContext);
			if (entityFact != null)
			{
				transientData.AppliedFacts.Add(entityFact.UniqueId);
			}
		}
		StartActions.Run();
	}

	protected void RunEndActions(AbilityExecutionContext context)
	{
		if (Condition.Any && !CheckCondition(context))
		{
			return;
		}
		TransientData transientData = RequestTransientData<TransientData>();
		foreach (string appliedFact in transientData.AppliedFacts)
		{
			base.Owner.Facts.RemoveById(appliedFact);
		}
		transientData.AppliedFacts.Clear();
		EndActions.Run();
	}

	protected bool CheckCondition(AbilityExecutionContext context)
	{
		if (base.Fact.Blueprint is BlueprintAbility blueprint && !context.Ability.Blueprint.SameAbility(blueprint))
		{
			return false;
		}
		return Condition.GetBoolValue(base.Owner, context, context.ClickedTarget.Entity);
	}
}
