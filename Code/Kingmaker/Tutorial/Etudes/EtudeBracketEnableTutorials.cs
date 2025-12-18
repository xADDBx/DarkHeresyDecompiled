using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Etudes;

[TypeId("ed0fd01395234eedbe5de6a440b21626")]
public class EtudeBracketEnableTutorials : EtudeBracketTrigger
{
	[SerializeField]
	[ValidateNotEmpty]
	private BlueprintTutorial.Reference[] m_Tutorials;

	public ReferenceArrayProxy<BlueprintTutorial> Tutorials
	{
		get
		{
			BlueprintReference<BlueprintTutorial>[] tutorials = m_Tutorials;
			return tutorials;
		}
	}

	protected override void OnEnter()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BlueprintTutorial tutorial in Tutorials)
			{
				Game.Instance.TutorialSystem.Ensure(tutorial.GetTutorial()).EnableByEtude();
			}
		}
	}

	protected override void OnExit()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BlueprintTutorial tutorial in Tutorials)
			{
				Game.Instance.TutorialSystem.Ensure(tutorial.GetTutorial()).DisableByEtude();
			}
		}
	}

	protected override void OnResume()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BlueprintTutorial tutorial in Tutorials)
			{
				Game.Instance.TutorialSystem.Ensure(tutorial.GetTutorial()).EnableByEtude();
			}
		}
	}
}
