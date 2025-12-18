using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("d3b57a2b4d875634a82e4c33ccc14b31")]
public class TutorialTriggerOnLevelUp : TutorialTrigger, ILevelUpManagerUIHandler, ISubscriber
{
	private LevelUpManager m_Manager;

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		m_Manager = manager;
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
		TryToTrigger(null, delegate(TutorialContext context)
		{
			context.SolutionUnit = m_Manager.TargetUnit;
		});
	}

	public void HandleUISelectionChanged()
	{
	}
}
