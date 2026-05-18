using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Globalmap.Blueprints;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public sealed class TransitionMapLocalVM : TransitionMapBoardVM
{
	public readonly LocalMapPaperListVM PaperListVM;

	public TransitionMapLocalVM(BlueprintMultiEntrance entrance, Action close)
		: base(entrance, close, BuildScreenEntities(entrance, close))
	{
		PaperListVM = new LocalMapPaperListVM(entrance, close).AddTo(this);
	}

	private static List<TransitionMapEntityVM> BuildScreenEntities(BlueprintMultiEntrance entrance, Action close)
	{
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		List<TransitionMapEntityVM> list = new List<TransitionMapEntityVM>();
		list.Add(new TransitionMapEntityVM(currentlyLoadedArea.AreaName, MapEntityKind.Area, MapEntityState.Enabled, null, close));
		list.AddRange(from e in entrance.Entries
			where e.IsVisible
			select TransitionMapBoardVM.CreateEntry(e, close));
		return list;
	}
}
