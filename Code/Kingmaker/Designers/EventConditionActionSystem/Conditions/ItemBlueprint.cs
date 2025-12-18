using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("f6bc601673bf7a44ca8c92d984578125")]
public class ItemBlueprint : Condition
{
	[SerializeReference]
	public AbstractItemEvaluator Item;

	[ValidateNotNull]
	public BlueprintItemReference Blueprint;

	protected override string GetConditionCaption()
	{
		return $"Item {Item} blueprint is {Blueprint.NameSafe()}";
	}

	protected override bool CheckCondition()
	{
		if (!Item.CanEvaluate())
		{
			return false;
		}
		return Blueprint.Is(Item.GetValue().Blueprint);
	}
}
