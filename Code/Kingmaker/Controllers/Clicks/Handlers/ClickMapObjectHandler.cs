using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.Scene.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickMapObjectHandler : IClickEventHandler
{
	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		bool isOvertip = false;
		MapObjectView mapObjectView = gameObject?.GetComponentNonAlloc<MapObjectView>();
		return new HandlerPriorityResult((CheckMapObject(mapObjectView) && (CheckInteractions(mapObjectView, out isOvertip) || CheckDestructible(mapObjectView) || UtilityInteracts.VariativeInteractionCount(mapObjectView) > 0)) ? 1f : 0f, isOvertip);
	}

	private static bool CheckMapObject(MapObjectView mapObj)
	{
		if (mapObj == null)
		{
			return false;
		}
		if (!mapObj.Data.IsRevealed)
		{
			return false;
		}
		if (!mapObj.Data.IsAwarenessCheckPassed)
		{
			return false;
		}
		return true;
	}

	private bool CheckInteractions(MapObjectView mapObj, out bool isOvertip)
	{
		EntityPartsManager.PartsByTypeEnumerable<AbstractInteractionPart> all = mapObj.Data.Parts.GetAll<AbstractInteractionPart>();
		bool flag = false;
		isOvertip = false;
		foreach (AbstractInteractionPart item in all)
		{
			if (!flag && item.CanInteract())
			{
				flag = true;
			}
			if (!isOvertip && item.ShowOvertip)
			{
				isOvertip = true;
			}
			if (flag & isOvertip)
			{
				break;
			}
		}
		if (flag)
		{
			return !isOvertip;
		}
		return false;
	}

	private bool CheckDestructible(MapObjectView mapObj)
	{
		if (mapObj is AbstractDestructibleEntityView abstractDestructibleEntityView)
		{
			if (!abstractDestructibleEntityView.VisibleInExploration)
			{
				return TurnController.IsInTurnBasedCombat();
			}
			return true;
		}
		return false;
	}

	public bool OnClick(GameObject gameObject, Vector3 _, int button, bool simulate = false, bool muteEvents = false)
	{
		DestructibleEntityView destructibleEntityView = gameObject.Or(null)?.GetComponent<DestructibleEntityView>();
		if (destructibleEntityView != null && destructibleEntityView.VisibleInExploration)
		{
			if (destructibleEntityView.ExplorationBark != null)
			{
				BarkPlayer.Bark(destructibleEntityView.EntityData, destructibleEntityView.ExplorationBark, VoiceOverType.Bark, string.Empty);
			}
			EventBus.RaiseEvent(delegate(IForceShowActionBarUIHandler h)
			{
				h.HandleForceShowActionBar(state: true);
			});
		}
		MapObjectView mapObject = gameObject.Or(null)?.GetComponent<MapObjectView>();
		if (UtilityInteracts.VariativeInteractionCount(mapObject) > 0)
		{
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(mapObject?.Data);
			});
			return true;
		}
		List<BaseUnitEntity> units = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList();
		return Interact(gameObject, units, forceOvertipInteractions: false, muteEvents);
	}

	public static bool HasAvailableInteractions(GameObject gameObject)
	{
		MapObjectView componentNonAlloc = gameObject.GetComponentNonAlloc<MapObjectView>();
		if (!CheckMapObject(componentNonAlloc))
		{
			return false;
		}
		foreach (AbstractInteractionPart item in componentNonAlloc.Data.Parts.GetAll<AbstractInteractionPart>())
		{
			if (item.CanInteract())
			{
				return true;
			}
		}
		return false;
	}

	public static bool Interact(GameObject gameObject, List<BaseUnitEntity> units, bool forceOvertipInteractions = false, bool muteEvents = false)
	{
		foreach (AbstractInteractionPart item in gameObject.GetComponent<MapObjectView>().Data.Parts.GetAll<AbstractInteractionPart>())
		{
			if ((!Game.Instance.IsControllerMouse || !item.ShowOvertip || forceOvertipInteractions) && TryInteract(item, units, muteEvents))
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryInteract(AbstractInteractionPart interaction, List<BaseUnitEntity> users, bool muteEvents = false, IInteractionVariantActor variantActor = null)
	{
		BaseUnitEntity baseUnitEntity = interaction.SelectUnit(users, muteEvents);
		if (baseUnitEntity == null)
		{
			return false;
		}
		ShowWarningIfNeeded(baseUnitEntity, interaction);
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return false;
		}
		if ((interaction.Type != 0 && interaction.Type != InteractionType.Approach && interaction.Type != InteractionType.Variant) || !interaction.CanInteract())
		{
			return false;
		}
		if ((interaction.Type == InteractionType.Direct || interaction.Type == InteractionType.Variant) && !muteEvents)
		{
			UnitCommandsRunner.DirectInteract(baseUnitEntity, interaction);
			return true;
		}
		if (interaction.HasVisibleTrap() || interaction is DisableTrapInteractionPart)
		{
			foreach (BaseUnitEntity user in users)
			{
				user.Commands.InterruptMove(byPlayer: true);
			}
		}
		if (interaction.Type == InteractionType.Approach)
		{
			UnitInteractWithObject.ApproachAndInteract(baseUnitEntity, interaction, variantActor);
		}
		return true;
	}

	public static void ShowWarningIfNeeded(BaseUnitEntity unit, AbstractInteractionPart interaction)
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			return;
		}
		string warning = null;
		ReasonStrings reasons = LocalizedTexts.Instance.Reasons;
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			warning = reasons.UnavailableGeneric;
		}
		if (interaction.AlreadyInteractedInThisCombatRound && !interaction.CanInteract())
		{
			warning = reasons.AlreadyInteractedInThisCombatRound.Text;
		}
		if (unit != null && !interaction.HasEnoughActionPoints(unit))
		{
			warning = reasons.NotEnoughActionPoints.Text;
		}
		if (unit != null && !interaction.IsEnoughCloseForInteractionFromDesiredPosition(unit))
		{
			warning = reasons.InteractionIsTooFar.Text;
		}
		if (!string.IsNullOrEmpty(warning))
		{
			EventBus.RaiseEvent(delegate(ICursorNotificationUIHandler h)
			{
				h.HandleNotification(warning, WarningNotificationFormat.Attention);
			});
		}
	}
}
