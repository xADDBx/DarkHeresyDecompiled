using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EvaluatedUnitDeathTrigger")]
[AllowMultipleComponents]
[TypeId("576c0defa521db5409ebace3b946bf6a")]
public class EvaluatedUnitDeathTrigger : EntityFactComponentDelegate, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	[HideIf("HasUnitEvaluator")]
	public bool AnyUnit;

	[HideIf("AnyUnit")]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[InfoBox(Text = "Не используйте этот триггер для получения доступа к инвентарю умершего unit'а (см. PF-61595).")]
	public ActionList Actions;

	private bool HasUnitEvaluator => Unit != null;

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity == null || (!AnyUnit && !Unit.Is(abstractUnitEntity)))
		{
			return;
		}
		using (ContextData<DeadUnitData>.Request().Setup(abstractUnitEntity))
		{
			Actions.Run();
		}
	}
}
