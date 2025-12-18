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

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EvaluatedUnitCombatTrigger")]
[AllowMultipleComponents]
[TypeId("f77f7470b5b4ccf489fa052f95c399a1")]
public class EvaluatedUnitCombatTrigger : EntityFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public class UnitData : SingleUnitData<UnitData>
	{
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public ActionList Actions;

	public bool TriggerOnExit;

	public void HandleUnitJoinCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (Unit.Is(baseUnitEntity) && !TriggerOnExit)
		{
			using (ContextData<UnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void HandleUnitLeaveCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (Unit.Is(baseUnitEntity) && TriggerOnExit)
		{
			using (ContextData<UnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}
}
