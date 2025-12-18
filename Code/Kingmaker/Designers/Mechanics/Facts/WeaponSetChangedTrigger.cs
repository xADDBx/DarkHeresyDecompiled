using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Events/WeaponSetChangedTrigger")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFact))]
[TypeId("5b683ca29d3843829eb81362767ae7be")]
public class WeaponSetChangedTrigger : EntityFactComponentDelegate, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	[SerializeField]
	private ActionList m_ActionList;

	public void HandleUnitChangeActiveEquipmentSet()
	{
		if (!(base.Fact.Owner is BaseUnitEntity baseUnitEntity) || EventInvokerExtensions.BaseUnitEntity != baseUnitEntity)
		{
			return;
		}
		using (baseUnitEntity.Context.SetScope())
		{
			m_ActionList?.Run();
		}
	}
}
