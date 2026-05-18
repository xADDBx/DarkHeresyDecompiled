using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Localization;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapPaperListVM : ViewModel
{
	public readonly LocalizedString Name;

	public readonly List<TransitionMapEntityVM> Entities;

	public LocalMapPaperListVM(BlueprintMultiEntrance entrance, Action onClick)
	{
		LocalMapPaperListVM localMapPaperListVM = this;
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		Entities = new List<TransitionMapEntityVM>
		{
			new TransitionMapEntityVM(currentlyLoadedArea.AreaName, MapEntityKind.Area, MapEntityState.Enabled, null, onClick)
		};
		Entities.AddRange(from e in entrance.Entries
			where e.IsVisible
			select localMapPaperListVM.GetMapEntity(e, onClick));
		Name = entrance.Name;
	}

	private TransitionMapEntityVM GetMapEntity(BlueprintMultiEntranceEntry entry, Action onClick)
	{
		MapEntityState state = ((!entry.IsInteractable) ? MapEntityState.NotInteractable : MapEntityState.Enabled);
		return new TransitionMapEntityVM(entry.Name, MapEntityKind.Entrance, state, Enter, onClick, entry.GetComponent<MultiEntranceGlobalMapMarker>()?.Zone);
		void Enter()
		{
			if (entry.IsInteractable)
			{
				entry.Enter();
			}
		}
	}
}
