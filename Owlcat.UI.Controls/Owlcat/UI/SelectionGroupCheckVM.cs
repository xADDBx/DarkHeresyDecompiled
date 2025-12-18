using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace Owlcat.UI;

public class SelectionGroupCheckVM<TViewModel> : SelectionGroupVM<TViewModel> where TViewModel : SelectionGroupEntityVM
{
	public ObservableList<TViewModel> SelectedEntitiesCollection;

	public SelectionGroupCheckVM(List<TViewModel> visibleCollection, ObservableList<TViewModel> selectedEntitiesCollection)
		: base(visibleCollection)
	{
		SelectedEntitiesCollection = selectedEntitiesCollection;
		SelectedEntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			SetupSelectedState();
		}).AddTo(this);
	}

	public SelectionGroupCheckVM(ObservableList<TViewModel> entitiesCollection, ObservableList<TViewModel> selectedEntitiesCollection)
		: base(entitiesCollection)
	{
		SelectedEntitiesCollection = selectedEntitiesCollection;
		SelectedEntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			SetupSelectedState();
		}).AddTo(this);
	}

	protected override bool TryDoSelect(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (SelectedEntitiesCollection == null)
		{
			return false;
		}
		if (!SelectedEntitiesCollection.Contains(viewModel))
		{
			SelectedEntitiesCollection.Add(viewModel);
			return true;
		}
		return false;
	}

	protected override bool TryDoUnselectSelect(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (SelectedEntitiesCollection == null)
		{
			return false;
		}
		if (SelectedEntitiesCollection.Contains(viewModel))
		{
			SelectedEntitiesCollection.Remove(viewModel);
			return true;
		}
		return false;
	}

	public override void SetupSelectedState()
	{
		foreach (TViewModel item in EntitiesCollection)
		{
			item.SetSelected(SelectedEntitiesCollection.Contains(item));
		}
	}
}
