using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("eb3243f248c346be85d64832d7aae8ee")]
public class TutorialWindowTrigger : TutorialTrigger, ITutorialWindowClosedHandler, ISubscriber
{
	[SerializeField]
	private ActionList m_ActionsOnHide;

	private bool m_ActionsDone;

	public void HandleHideTutorial(TutorialData data)
	{
		if (base.Fact.Blueprint != data.Blueprint || !m_ActionsOnHide.HasActions || m_ActionsDone)
		{
			return;
		}
		using (ContextData<TutorialIsActiveContext>.Request())
		{
			m_ActionsOnHide.Run();
			m_ActionsDone = true;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		m_ActionsDone = false;
	}
}
