using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("`t|SourceUnit` - unit (or pet's master if IsPet enabled)\n`t|Descriptor` - pet if IsPet enabled")]
[TypeId("4838bc21e4086864d948fc3f1bb29d91")]
public class TutorialTriggerUnitDeath : TutorialTrigger, IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	public bool IsPet;

	public void OnUnitDie()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit.IsPet != IsPet || (unit.Blueprint.Faction != ConfigRoot.Instance.SystemMechanics.PlayerFaction && unit.Master?.Blueprint.Faction != ConfigRoot.Instance.SystemMechanics.PlayerFaction))
		{
			return;
		}
		UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.ExCompanion)
		{
			return;
		}
		if (IsPet)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit.Master;
				context[TutorialContextKey.Descriptor] = unit;
			});
		}
		else
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}
}
