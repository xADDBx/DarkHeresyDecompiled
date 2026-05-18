using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Combat/CombatStateTrigger")]
[TypeId("dbb7058d31be12446942310a6ab86b83")]
public class CombatStateTrigger : UnitFactComponentDelegate, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>
{
	public ActionList CombatStartActions;

	public ActionList CombatEndActions;

	[SerializeField]
	private bool UnitMustBeConscious;

	public void HandleUnitJoinCombat()
	{
		base.Fact.RunActionInContext(CombatStartActions, base.Owner);
	}

	public void HandleUnitLeaveCombat()
	{
		if (!UnitMustBeConscious || !base.Owner.IsDeadOrUnconscious)
		{
			base.Fact.RunActionInContext(CombatEndActions, base.Owner);
		}
	}
}
