using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("418c6aca22a4068439964d3f1060abea")]
public class TutorialTriggerUnitLevelUp : TutorialTrigger, ILevelUpManagerUIHandler, ISubscriber
{
	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private int m_Level;

	private BlueprintUnit Unit => m_Unit.Get();

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		BaseUnitEntity unit = manager.TargetUnit;
		PartUnitProgression required = unit.Parts.GetRequired<PartUnitProgression>();
		if (unit.Blueprint == Unit && required.ExperienceLevel >= m_Level)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = unit;
			});
		}
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
	}

	public void HandleUISelectionChanged()
	{
	}
}
