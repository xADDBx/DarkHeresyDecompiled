using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TransitionMapBoardVM : ViewModel
{
	public readonly BlueprintMultiEntranceMap Map;

	public readonly List<TransitionMapEntityVM> ScreenEntities;

	private readonly Action m_Close;

	protected TransitionMapBoardVM(BlueprintMultiEntrance entrance, Action close, List<TransitionMapEntityVM> screenEntities)
	{
		Map = entrance.Map;
		ScreenEntities = screenEntities;
		m_Close = close;
	}

	public void Close()
	{
		m_Close?.Invoke();
	}

	protected static TransitionMapEntityVM CreateEntry(BlueprintMultiEntranceEntry entry, Action close)
	{
		MapEntityState state = ((!entry.IsInteractable) ? MapEntityState.NotInteractable : MapEntityState.Enabled);
		BlueprintMultiEntranceMap? zone = entry.GetComponent<MultiEntranceGlobalMapMarker>()?.Zone;
		return new TransitionMapEntityVM(entry.Name, MapEntityKind.Entrance, state, Enter, close, zone);
		void Enter()
		{
			if (entry.IsInteractable)
			{
				entry.Enter();
			}
		}
	}
}
