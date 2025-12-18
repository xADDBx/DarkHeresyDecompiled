using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class NestedSelectionGroupVM<TViewModel> : ViewModel where TViewModel : NestedSelectionGroupEntityVM
{
	private readonly ReactiveProperty<TViewModel> m_LastChangedEntity = new ReactiveProperty<TViewModel>();

	public ObservableDictionary<INestedListSource, List<NestedSelectionGroupEntityVM>> NestedEntityCollections = new ObservableDictionary<INestedListSource, List<NestedSelectionGroupEntityVM>>();

	public ReadOnlyReactiveProperty<TViewModel> LastChangedEntity => m_LastChangedEntity;

	public NestedSelectionGroupVM(INestedListSource nestedListSource)
	{
		TryAddNestedList(nestedListSource);
	}

	protected virtual void SubscribeNewItem(TViewModel entityViewModel)
	{
		entityViewModel.IsSelected.Where((bool value) => value).Subscribe(delegate
		{
			TrySelectEntity(entityViewModel);
		}).AddTo(this);
		entityViewModel.IsSelected.Where((bool value) => !value).Subscribe(delegate
		{
			TryUnselectEntity(entityViewModel);
		}).AddTo(this);
		if (entityViewModel.HasNesting)
		{
			entityViewModel.IsExpanded.Where((bool value) => value).Subscribe(delegate
			{
				TryAddNestedList(entityViewModel.NextSource);
			}).AddTo(this);
			entityViewModel.IsExpanded.Where((bool value) => !value).Subscribe(delegate
			{
				TryRemoveNestedList(entityViewModel.NextSource);
			}).AddTo(this);
		}
	}

	protected override void OnDispose()
	{
		m_LastChangedEntity.Value = null;
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
		if (!viewModel.IsAvailable.CurrentValue && !viewModel.IsSelected.CurrentValue)
		{
			return false;
		}
		if (TryDoSelect(viewModel))
		{
			m_LastChangedEntity.Value = viewModel;
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
		if (!viewModel.IsAvailable.CurrentValue && !viewModel.IsSelected.CurrentValue)
		{
			return false;
		}
		if (TryDoUnselectSelect(viewModel))
		{
			m_LastChangedEntity.Value = viewModel;
			return true;
		}
		return false;
	}

	protected abstract bool TryDoUnselectSelect(TViewModel viewModel);

	private bool TryAddNestedList(INestedListSource source)
	{
		if (!source.HasNesting)
		{
			return false;
		}
		if (source is NestedSelectionGroupEntityVM nestedSelectionGroupEntityVM && !nestedSelectionGroupEntityVM.IsAvailable.CurrentValue)
		{
			return false;
		}
		List<NestedSelectionGroupEntityVM> list = source.ExtractNestedEntities();
		if (list == null || !list.Any())
		{
			return false;
		}
		foreach (NestedSelectionGroupEntityVM item in list)
		{
			SubscribeNewItem(item as TViewModel);
		}
		NestedEntityCollections[source] = list.Select((NestedSelectionGroupEntityVM entity) => entity).ToList();
		DoAddNestedList(source);
		return true;
	}

	protected abstract void DoAddNestedList(INestedListSource source);

	private bool TryRemoveNestedList(INestedListSource source)
	{
		if (source is NestedSelectionGroupEntityVM nestedSelectionGroupEntityVM && !nestedSelectionGroupEntityVM.IsAvailable.CurrentValue)
		{
			return false;
		}
		if (!NestedEntityCollections.ContainsKey(source))
		{
			return false;
		}
		foreach (NestedSelectionGroupEntityVM item in NestedEntityCollections[source])
		{
			if (item.HasNesting && item.IsExpanded.CurrentValue)
			{
				item.SetExpanded(state: false);
			}
			item.Dispose();
		}
		NestedEntityCollections.Remove(source);
		DoRemoveNestedList(source);
		return true;
	}

	protected abstract void DoRemoveNestedList(INestedListSource viewModel);
}
