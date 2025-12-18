using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem;
using Kingmaker.Gameplay.Utility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.Controllers;

public class GridNodeToEntityCacheController : IControllerStart, IController, IControllerStop, IControllerReset, IEntityHoldingStateChangedHandler, ISubscriber<IEntity>, ISubscriber, IEntityPositionChangedHandler, IInGameHandler
{
	private GridNodeToEntityCache Cache => Game.Instance.GridNodeToEntityCache;

	void IControllerStart.OnStart()
	{
		InitializeCache();
	}

	void IControllerReset.OnReset()
	{
		Cache.Drop();
	}

	void IControllerStop.OnStop()
	{
		Cache.Drop();
	}

	private void InitializeCache()
	{
		using (ProfileScope.New("Initialize Cache"))
		{
			Cache.Drop();
			foreach (MechanicEntity mechanicEntity in Game.Instance.EntityPools.MechanicEntities)
			{
				Cache.UpdateEntity(mechanicEntity);
			}
		}
	}

	void IEntityHoldingStateChangedHandler.HandleEntityHoldingStateChanged()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null)
		{
			Cache.UpdateEntity(mechanicEntity);
		}
	}

	void IEntityPositionChangedHandler.HandleEntityPositionChanged()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null)
		{
			Cache.UpdateEntity(mechanicEntity);
		}
	}

	void IInGameHandler.HandleObjectInGameChanged()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null)
		{
			Cache.UpdateEntity(mechanicEntity);
		}
	}
}
