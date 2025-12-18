using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("d611bb4cba634bc4bb6e5057b07ffc97")]
public class CheckPassed : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Check")]
	private BlueprintCheckReference m_Check;

	public BlueprintCheck CheckBlueprint => m_Check?.Get();

	protected override string GetConditionCaption()
	{
		return $"Check Passed ({CheckBlueprint})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Controllers.DialogController.LocalPassedChecks.Contains(CheckBlueprint);
	}
}
