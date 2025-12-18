using Kingmaker.AreaLogic.Etudes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Etudes;

[TypeId("d7f523092176498cb0b0c2e30ed8ac81")]
public class EtudeBracketEnableTutorialSingle : EtudeBracketTrigger
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintTutorial.Reference m_Tutorial;

	public BlueprintTutorial Tutorial => m_Tutorial.Get().GetTutorial();

	protected override void OnEnter()
	{
		Game.Instance.TutorialSystem.Ensure(Tutorial).EnableByEtude();
	}

	protected override void OnExit()
	{
		Game.Instance.TutorialSystem.Ensure(Tutorial).DisableByEtude();
	}

	protected override void OnResume()
	{
		Game.Instance.TutorialSystem.Ensure(Tutorial).EnableByEtude();
	}
}
