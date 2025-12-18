using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("a091db175cce8f94f815f98e9e353a84")]
public class AnswerSelected : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Answer")]
	private BlueprintAnswerReference m_Answer;

	[Tooltip("Only check selected answers in current dialog instance. By default whole game history matters.")]
	public bool CurrentDialog;

	public BlueprintAnswer Answer => m_Answer?.Get();

	protected override string GetConditionCaption()
	{
		return $"Answer Selected ({Answer})";
	}

	protected override bool CheckCondition()
	{
		if (!CurrentDialog)
		{
			return Game.Instance.DialogState.SelectedAnswersContains(Answer);
		}
		return Game.Instance.Controllers.DialogController.LocalSelectedAnswers.Contains(Answer);
	}
}
