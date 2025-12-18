using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("f833804310724fc8826ddc9b7340a38d")]
public class EquipItemTrigger : EntityFactComponentDelegate<ItemEntity>, IEquipItemHandler<EntitySubscriber>, IEquipItemHandler, ISubscriber<IItemEntity>, ISubscriber, IEventTag<IEquipItemHandler, EntitySubscriber>
{
	[SerializeField]
	private ActionList m_OnDidEquipped;

	[SerializeField]
	private ActionList m_OnWillUnequip;

	public void OnDidEquipped()
	{
		ActionList onDidEquipped = m_OnDidEquipped;
		if (onDidEquipped != null && onDidEquipped.HasActions)
		{
			m_OnDidEquipped.Run();
		}
	}

	public void OnWillUnequip()
	{
		ActionList onWillUnequip = m_OnWillUnequip;
		if (onWillUnequip != null && onWillUnequip.HasActions)
		{
			m_OnWillUnequip.Run();
		}
	}
}
