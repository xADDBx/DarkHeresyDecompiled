using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Movement/PositionChangedTrigger")]
[TypeId("e4299da3e4c84af1ab00920bad668543")]
public sealed class PositionChangedTrigger : MechanicEntityFactComponentDelegate, IEntityPositionChangedHandler<EntitySubscriber>, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IEntityPositionChangedHandler, EntitySubscriber>
{
	private sealed class ComponentData : IEntityFactComponentTransientData
	{
		public GridNodeBase PreviousNode;
	}

	public bool TriggerIfCurrenNodeChanged;

	public ActionList Actions;

	void IEntityPositionChangedHandler.HandleEntityPositionChanged()
	{
		if (!TriggerIfCurrenNodeChanged)
		{
			Actions.Run();
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		GridNode currentUnwalkableNode = base.Owner.CurrentUnwalkableNode;
		if (componentData.PreviousNode != currentUnwalkableNode)
		{
			componentData.PreviousNode = currentUnwalkableNode;
			Actions.Run();
		}
	}
}
