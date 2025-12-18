using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[TypeId("2749dee00d6b9944da786857cf476773")]
public class IncreaseResourceAmount : UnitFactComponentDelegate, IResourceAmountBonusHandler<EntitySubscriber>, IResourceAmountBonusHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IResourceAmountBonusHandler, EntitySubscriber>
{
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public int Value = 1;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
	{
		if (base.Fact.Active && resource == Resource)
		{
			bonus += Value * base.Fact.GetRank();
		}
	}
}
