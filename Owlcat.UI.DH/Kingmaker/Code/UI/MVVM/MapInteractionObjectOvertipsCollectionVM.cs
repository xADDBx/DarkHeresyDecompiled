using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM;

public class MapInteractionObjectOvertipsCollectionVM : BaseMapObjectOvertipsCollectionVM<OvertipMapObjectVM>, IInteractionHighlightUIHandler, ISubscriber, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IUnitCommandEndHandler, IPartyCombatHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IItemsCollectionHandler, IInGameHandler, ISubscriber<IEntity>, IResetAwarenessHandler, IAwarenessHandler
{
	protected override void AddEntity(Entity entityData)
	{
		if (!NeedOvertip(entityData) || ContainsOvertip(entityData))
		{
			return;
		}
		try
		{
			OvertipMapObjectVM item = new OvertipMapObjectVM(entityData as MapObjectEntity);
			Overtips.Add(item);
		}
		catch (Exception ex)
		{
			PFLog.UI.Error("Cannot create overtip for " + ex.Source + "\n(" + ex.Message + ")");
		}
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		if (entityData.IsDisposed || entityData.Destroyed || !(entityData is MapObjectEntity { View: not null } mapObjectEntity))
		{
			return false;
		}
		return OvertipUtils.CheckNeedMapObjectOvertip(mapObjectEntity);
	}

	protected override void OnRescanEntities()
	{
		HandlePartyCombatStateChanged(Game.Instance.Player.IsInCombat);
		if (Game.Instance.Controllers.InteractionHighlightController != null)
		{
			HandleHighlightChange(Game.Instance.Controllers.InteractionHighlightController.IsGlobalHighlighting);
		}
	}

	public void ShowBark(Entity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(Entity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	public void HandleHighlightChange(bool isOn)
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleHighlightChange(isOn);
		});
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleCombatStateChanged();
		});
	}

	public void HandleObjectHighlightChange()
	{
		GetOvertip(GetRevealedMapObject())?.HighlightChanged();
	}

	public void HandleObjectInteractChanged()
	{
		GetOvertip(GetMapObject())?.UpdateObjectData();
	}

	public void HandleObjectInteract()
	{
		GetOvertip(GetRevealedMapObject())?.Interact();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateMapObjectInteraction(command, active: true);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateMapObjectInteraction(command, active: false);
	}

	private void UpdateMapObjectInteraction(AbstractUnitCommand command, bool active)
	{
		if (command is UnitInteractWithObject unitInteractWithObject)
		{
			GetOvertip(unitInteractWithObject.Interaction.Owner)?.UpdateInteraction(active);
		}
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (IsInteractionItem(item))
		{
			Overtips.Where((OvertipMapObjectVM o) => o.RequiredResourceCount.HasValue).ForEach(delegate(OvertipMapObjectVM o)
			{
				o?.TriggerInventoryChanged();
			});
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (IsInteractionItem(item))
		{
			Overtips.Where((OvertipMapObjectVM o) => o.RequiredResourceCount.HasValue).ForEach(delegate(OvertipMapObjectVM o)
			{
				o?.TriggerInventoryChanged();
			});
		}
	}

	private bool IsInteractionItem(ItemEntity item)
	{
		if (item.Blueprint != ConfigRoot.Instance.Consumables.MultikeyItem && item.Blueprint != ConfigRoot.Instance.Consumables.MeltaChargeItem)
		{
			return item.Blueprint == ConfigRoot.Instance.Consumables.RitualSetItem;
		}
		return true;
	}

	public void HandleAwarenessCheckReset()
	{
		GetOvertip(GetMapObject())?.UpdateObjectData();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleCombatStateChanged();
		});
	}

	public void HandleTurnBasedModeResumed()
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleCombatStateChanged();
		});
	}

	public void HandleObjectInGameChanged()
	{
		GetOvertip(GetMapObject())?.UpdateObjectData();
	}

	public void OnEntityNoticed(BaseUnitEntity spotter)
	{
		GetOvertip(GetMapObject())?.UpdateObjectData();
	}
}
