using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[ComponentName("Events/SpawnUnit Trigger")]
[AllowMultipleComponents]
[TypeId("1feb4f631b88d6542919d967f9da9234")]
public class SpawnUnitTrigger : EntityFactComponentDelegate, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	[ValidateNotNull]
	[SerializeField]
	public BlueprintUnitReference m_TargetUnit;

	public ActionList Actions;

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.Blueprint.Equals(m_TargetUnit.Get()))
		{
			Actions.Run();
		}
	}
}
