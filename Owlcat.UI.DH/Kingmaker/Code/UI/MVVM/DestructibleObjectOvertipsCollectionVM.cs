using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM;

public class DestructibleObjectOvertipsCollectionVM : BaseMapObjectOvertipsCollectionVM<OvertipDestructibleObjectVM>, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IDestructibleEntityHandler
{
	private static readonly float DeathEntityRemoveDelay = 4f;

	private readonly Dictionary<Entity, IDisposable> m_DelayedRemoveHandlers = new Dictionary<Entity, IDisposable>();

	protected override void Clear()
	{
		m_DelayedRemoveHandlers.Values.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_DelayedRemoveHandlers.Clear();
		base.Clear();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		return entityData is DestructibleEntity;
	}

	public void HandleObjectHighlightChange()
	{
		GetOvertip(GetRevealedMapObject())?.HighlightChanged();
	}

	public void HandleObjectInteractChanged()
	{
	}

	public void HandleObjectInteract()
	{
	}

	public void ShowBark(Entity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(Entity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	public void HandleDestructionStageChanged(DestructionStage stage)
	{
		switch (stage)
		{
		case DestructionStage.Destroyed:
			TryDelayedRemoveEntity(EventInvokerExtensions.GetEntity<DestructibleEntity>());
			break;
		case DestructionStage.Whole:
			AddEntity(EventInvokerExtensions.GetEntity<DestructibleEntity>());
			break;
		}
	}

	private void TryDelayedRemoveEntity(Entity entityData)
	{
		m_DelayedRemoveHandlers.Remove(entityData);
		float seconds = GetOvertip(entityData)?.DeathDelay ?? DeathEntityRemoveDelay;
		m_DelayedRemoveHandlers[entityData] = DelayedInvoker.InvokeInTime(delegate
		{
			RemoveEntity(entityData);
		}, seconds);
	}
}
