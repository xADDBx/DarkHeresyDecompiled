using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[AllowMultipleComponents]
[TypeId("4b7112837abb21f448d0438fbf18efd8")]
public class CompanionUnrecruitTrigger : EntityFactComponentDelegate, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public bool TriggerOnDeath;

	public ActionList Actions;

	public BlueprintUnit CompanionBlueprint => m_CompanionBlueprint?.Get();

	public void HandleRecruit()
	{
	}

	public void HandleUnrecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.Blueprint == CompanionBlueprint)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (TriggerOnDeath && baseUnitEntity != null && baseUnitEntity.Blueprint == CompanionBlueprint && baseUnitEntity.LifeState.IsFinallyDead)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}
}
