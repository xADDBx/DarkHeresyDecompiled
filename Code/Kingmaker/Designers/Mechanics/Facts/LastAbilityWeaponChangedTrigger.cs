using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Events/LastAbilityWeaponChangedTrigger")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFact))]
[TypeId("0592e6edc97d472cbbc6b58c0127cf7c")]
public class LastAbilityWeaponChangedTrigger : EntityFactComponentDelegate, ILastAbilityWeaponChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	[SerializeField]
	private ActionList m_ActionList;

	public void HandleLastAbilityWeaponChange(ItemEntityWeapon oldWeapon, ItemEntityWeapon newWeapon)
	{
		if (!(base.Fact.Owner is BaseUnitEntity baseUnitEntity) || EventInvokerExtensions.BaseUnitEntity != baseUnitEntity || oldWeapon == null || newWeapon == null)
		{
			return;
		}
		using (EvalContext.PushContext(baseUnitEntity.Context))
		{
			m_ActionList?.Run();
		}
	}
}
