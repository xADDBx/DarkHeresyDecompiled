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
[TypeId("15415d6ca28148d2a017989e562f7cbc")]
public class CompanionRecruitTrigger : EntityFactComponentDelegate, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public ActionList Actions;

	public BlueprintUnit CompanionBlueprint => m_CompanionBlueprint?.Get();

	public void HandleRecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.Blueprint == CompanionBlueprint)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void HandleUnrecruit()
	{
	}
}
