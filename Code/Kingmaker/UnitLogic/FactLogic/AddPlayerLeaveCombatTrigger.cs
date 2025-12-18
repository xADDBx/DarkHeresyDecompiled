using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("5515c32f67244b15a1c95d2f7628b596")]
public class AddPlayerLeaveCombatTrigger : EntityFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool InCombat;
	}

	public ActionList Actions;

	protected override void OnActivateOrPostLoad()
	{
		RequestTransientData<ComponentData>().InCombat = Game.Instance.Player.IsInCombat;
	}

	public void HandleUnitJoinCombat()
	{
		RequestTransientData<ComponentData>().InCombat |= Game.Instance.Player.IsInCombat;
	}

	public void HandleUnitLeaveCombat()
	{
		if (RequestTransientData<ComponentData>().InCombat && !Game.Instance.Player.MainCharacter.Entity.IsInCombat)
		{
			if (base.Fact.MaybeContext != null)
			{
				base.Fact.RunActionInContext(Actions);
			}
			else
			{
				Actions.Run();
			}
		}
	}
}
