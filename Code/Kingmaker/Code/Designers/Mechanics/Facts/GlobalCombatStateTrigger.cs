using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Combat/GlobalCombatStateTrigger")]
[TypeId("5a95724975c24882a5efb0c5aed8c2ab")]
public class GlobalCombatStateTrigger : UnitFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public ActionList ActionsOnEnter;

	public ActionList ActionsOnLeave;

	public void HandleUnitJoinCombat()
	{
		if (base.Owner.IsPreviewUnit)
		{
			PFLog.Default.Log("[HandleUnitJoinCombat] Called on preview unit -- ignoring!");
			return;
		}
		CompanionState? companionState = base.Owner.GetCompanionOptional()?.State;
		if ((!companionState.HasValue || companionState.GetValueOrDefault() == CompanionState.InParty) && ContextData<EventInvoker>.Current?.InvokerEntity is UnitEntity target)
		{
			base.Fact.RunActionInContext(ActionsOnEnter, target);
		}
	}

	public void HandleUnitLeaveCombat()
	{
		CompanionState? companionState = base.Owner.GetCompanionOptional()?.State;
		if ((!companionState.HasValue || companionState.GetValueOrDefault() == CompanionState.InParty) && ContextData<EventInvoker>.Current?.InvokerEntity is UnitEntity target)
		{
			base.Fact.RunActionInContext(ActionsOnLeave, target);
		}
	}
}
