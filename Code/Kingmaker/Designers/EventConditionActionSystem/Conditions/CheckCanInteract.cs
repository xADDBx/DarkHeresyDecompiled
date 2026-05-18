using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("415c4b797464452aa0e747d80f014102")]
public class CheckCanInteract : Condition
{
	[SerializeReference]
	public InteractionActionEvaluator Interaction;

	protected override bool CheckCondition()
	{
		return Interaction.GetValue().CanInteract();
	}

	protected override string GetConditionCaption()
	{
		return $"Check if can interact with {Interaction}";
	}
}
