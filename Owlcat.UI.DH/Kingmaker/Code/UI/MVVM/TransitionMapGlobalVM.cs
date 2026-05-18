using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Globalmap.Blueprints;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class TransitionMapGlobalVM : TransitionMapBoardVM
{
	public readonly ReactiveProperty<TransitionMapEntityVM> CurrentEntity;

	public TransitionMapGlobalVM(BlueprintMultiEntrance entrance, Action close)
		: base(entrance, close, BuildScreenEntities(entrance, close))
	{
		CurrentEntity = new ReactiveProperty<TransitionMapEntityVM>(ResolveStartingEntity(entrance.GetComponent<MultiEntranceGlobalMapSettings>()?.CurrentMap));
	}

	private TransitionMapEntityVM ResolveStartingEntity(BlueprintMultiEntranceMap? startingZone)
	{
		if (startingZone.HasValue)
		{
			TransitionMapEntityVM transitionMapEntityVM = ScreenEntities.FirstOrDefault((TransitionMapEntityVM e) => e.Zone == startingZone.Value);
			if (transitionMapEntityVM != null)
			{
				return transitionMapEntityVM;
			}
		}
		return ScreenEntities.FirstOrDefault();
	}

	private static List<TransitionMapEntityVM> BuildScreenEntities(BlueprintMultiEntrance entrance, Action close)
	{
		MultiEntranceGlobalMapSettings component = entrance.GetComponent<MultiEntranceGlobalMapSettings>();
		BlueprintMultiEntranceMap? currentMap = component?.CurrentMap;
		List<TransitionMapEntityVM> list = (from e in entrance.Entries
			where e.IsVisible || IsShipZoneEntry(e, currentMap)
			select TransitionMapBoardVM.CreateEntry(e, (e.HasPartySelectionAction || !e.IsInteractable) ? null : close)).ToList();
		if (ShouldOfferGuncutterExit(entrance, component))
		{
			list.Add(new TransitionMapEntityVM(UIStrings.Instance.Transition.ToGuncutterText, MapEntityKind.Entrance, MapEntityState.Enabled, null, close, BlueprintMultiEntranceMap.Global));
		}
		return list;
	}

	private static bool IsShipZoneEntry(BlueprintMultiEntranceEntry entry, BlueprintMultiEntranceMap? currentMap)
	{
		if (!currentMap.HasValue)
		{
			return false;
		}
		MultiEntranceGlobalMapMarker component = entry.GetComponent<MultiEntranceGlobalMapMarker>();
		if (component != null)
		{
			return component.Zone == currentMap.Value;
		}
		return false;
	}

	private static bool ShouldOfferGuncutterExit(BlueprintMultiEntrance entrance, MultiEntranceGlobalMapSettings globalSettings)
	{
		if (entrance.Entries.Any(delegate(BlueprintMultiEntranceEntry e)
		{
			if (e.IsVisible)
			{
				MultiEntranceGlobalMapMarker component = e.GetComponent<MultiEntranceGlobalMapMarker>();
				if (component == null)
				{
					return false;
				}
				return component.Zone == BlueprintMultiEntranceMap.Global;
			}
			return false;
		}))
		{
			return false;
		}
		if (globalSettings?.CanExitToGunCutter != null)
		{
			return globalSettings.CanExitToGunCutter.Check();
		}
		return false;
	}
}
