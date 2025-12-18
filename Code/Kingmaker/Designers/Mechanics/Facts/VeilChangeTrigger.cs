using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Veil/VeilChangeTrigger")]
[TypeId("25e8cd92c4864e0eab9af83d27a65b52")]
public class VeilChangeTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleCalculateVeilDamage>, IRulebookHandler<RuleCalculateVeilDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public int OldVeil { get; set; }
	}

	public ActionList ActionsOnVeilChange;

	public ActionList ActionsOnMoreVeil;

	public ActionList ActionsOnLessVeil;

	public ActionList ActionsOnUnchaingedVeil;

	public bool AssingVeilChangeInitiatorAsTarget;

	public void OnEventAboutToTrigger(RuleCalculateVeilDamage evt)
	{
		RequestTransientData<ComponentData>().OldVeil = Game.Instance.LoadedArea.Veil.Damage;
	}

	public void OnEventDidTrigger(RuleCalculateVeilDamage evt)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (evt.ResultDamage == componentData.OldVeil)
		{
			base.Fact.RunActionInContext(ActionsOnUnchaingedVeil, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
			return;
		}
		base.Fact.RunActionInContext(ActionsOnVeilChange, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
		if (evt.ResultDamage > componentData.OldVeil)
		{
			base.Fact.RunActionInContext(ActionsOnMoreVeil, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
		}
		if (evt.ResultDamage < componentData.OldVeil)
		{
			base.Fact.RunActionInContext(ActionsOnLessVeil, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
		}
	}
}
