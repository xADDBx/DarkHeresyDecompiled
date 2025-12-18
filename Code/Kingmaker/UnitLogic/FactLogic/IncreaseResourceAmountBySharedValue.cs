using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[TypeId("162624b2a4aa4c141ba4b3c7ed8afc04")]
public class IncreaseResourceAmountBySharedValue : UnitFactComponentDelegate, IResourceAmountBonusHandler<EntitySubscriber>, IResourceAmountBonusHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IResourceAmountBonusHandler, EntitySubscriber>
{
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public ContextValue Value;

	public bool Decrease;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
	{
		if (base.Fact.Active && resource == Resource)
		{
			if (!Decrease)
			{
				bonus += Value.Calculate(base.Context);
			}
			else
			{
				bonus -= Value.Calculate(base.Context);
			}
		}
	}
}
