using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[AllowMultipleComponents]
[TypeId("1711006cc1e74dbca3a18881ed67850e")]
public class TutorialTriggerUnitGainFact : TutorialTrigger, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public void HandleEntityGainFact(EntityFact fact)
	{
		Entity owner = fact.Owner;
		BaseUnitEntity unit = owner as BaseUnitEntity;
		if (unit != null && m_Unit.Get() == unit.Blueprint && fact is UnitFact unitFact && m_Fact.Get() == unitFact.Blueprint)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = unit;
				context.SourceFact = fact;
			});
		}
	}
}
