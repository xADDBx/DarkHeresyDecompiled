using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("b992ceba7aa1e6d48a26823cf282e744")]
public class FamiliarEquipped : AbstractFamiliarEquipped
{
	[NotNull]
	[SerializeReference]
	[AllowedEntityType(typeof(BaseUnitEntity))]
	public AbstractUnitEvaluator LeaderEvaluator;

	protected override BaseUnitEntity Leader => LeaderEvaluator.GetValue() as BaseUnitEntity;

	protected override string GetConditionCaption()
	{
		if (base.Unit != null)
		{
			return $"{LeaderEvaluator?.GetCaption()} has equipped {base.Unit} familiar";
		}
		return LeaderEvaluator?.GetCaption() + " has no equipped familiar";
	}
}
