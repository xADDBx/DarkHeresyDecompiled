using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace Owlcat.UI;

public abstract class SelectionGroupVM<TViewModel> : ViewModel where TViewModel : SelectionGroupEntityVM
{
	public ObservableList<TViewModel> EntitiesCollection;

	public ReactiveProperty<TViewModel> LastChangedEntity = new ReactiveProperty<TViewModel>();

	public SelectionGroupVM(List<TViewModel> visibleCollection)
	{
		Disposable.Create(DisposeImplementation).AddTo(this);
		EntitiesCollection = new ObservableList<TViewModel>(visibleCollection);
		SubscribeItems();
	}

	public SelectionGroupVM(ObservableList<TViewModel> entitiesCollection)
	{
		Disposable.Create(DisposeImplementation).AddTo(this);
		EntitiesCollection = entitiesCollection;
		SubscribeItems();
	}

	private void SubscribeItems()
	{
		foreach (TViewModel item in EntitiesCollection)
		{
			SubscribeNewItem(item);
		}
		EntitiesCollection.ObserveAdd().Subscribe(delegate(CollectionAddEvent<TViewModel> newItem)
		{
			SubscribeNewItem(newItem.Value);
		}).AddTo(this);
		EntitiesCollection.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<TViewModel> newItem)
		{
			DisposeRemovedItem(newItem.Value);
		}).AddTo(this);
	}

	public void InsertEntityAtIndex(int index, TViewModel viewModel)
	{
		EntitiesCollection.Insert(index, viewModel);
	}

	public void RemoveEntity(TViewModel viewModel)
	{
		DisposeRemovedItem(viewModel);
		EntitiesCollection.Remove(viewModel);
	}

	public void ClearFromIndex(int index)
	{
		while (EntitiesCollection.Count > index)
		{
			EntitiesCollection.RemoveAt(EntitiesCollection.Count - 1);
		}
	}

	protected virtual void SubscribeNewItem(TViewModel viewModel)
	{
		viewModel.IsSelected.Where((bool value) => value).Subscribe(delegate
		{
			TrySelectEntity(viewModel);
		}).AddTo(this);
		viewModel.IsSelected.Where((bool value) => !value).Subscribe(delegate
		{
			TryUnselectEntity(viewModel);
		}).AddTo(this);
	}

	private void DisposeRemovedItem(TViewModel viewModel)
	{
		viewModel.Dispose();
	}

	public bool TrySelectEntity(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (!viewModel.IsAvailable.CurrentValue && !viewModel.IsSelected.Value)
		{
			return false;
		}
		if (TryDoSelect(viewModel))
		{
			LastChangedEntity.Value = viewModel;
			return true;
		}
		return false;
	}

	protected abstract bool TryDoSelect(TViewModel viewModel);

	private bool TryUnselectEntity(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (!viewModel.IsAvailable.CurrentValue && !viewModel.IsSelected.Value)
		{
			return false;
		}
		if (TryDoUnselectSelect(viewModel))
		{
			LastChangedEntity.Value = viewModel;
			return true;
		}
		return false;
	}

	protected abstract bool TryDoUnselectSelect(TViewModel viewModel);

	protected virtual void DisposeImplementation()
	{
		LastChangedEntity.Value = null;
	}

	public bool TrySelectFirstValidEntity()
	{
		TViewModel val = EntitiesCollection.FirstOrDefault((TViewModel ent) => ent.IsAvailable.CurrentValue);
		if (val == null)
		{
			return false;
		}
		if (val.IsSelected.Value)
		{
			return true;
		}
		return TrySelectEntity(val);
	}

	public bool TrySelectLastValidEntity()
	{
		TViewModel val = EntitiesCollection.LastOrDefault((TViewModel ent) => ent.IsAvailable.CurrentValue);
		if (val == null)
		{
			return false;
		}
		if (val.IsSelected.Value)
		{
			return true;
		}
		return TrySelectEntity(val);
	}

	public abstract void SetupSelectedState();

	public void AddDisposable(IDisposable disposable)
	{
		disposable.AddTo(this);
	}
}
