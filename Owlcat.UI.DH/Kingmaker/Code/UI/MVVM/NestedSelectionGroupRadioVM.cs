using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class NestedSelectionGroupRadioVM<TViewModel> : NestedSelectionGroupVM<TViewModel> where TViewModel : NestedSelectionGroupEntityVM
{
	public ObservableDictionary<INestedListSource, ReactiveProperty<NestedSelectionGroupEntityVM>> NestedSelectedEntities = new ObservableDictionary<INestedListSource, ReactiveProperty<NestedSelectionGroupEntityVM>>();

	public NestedSelectionGroupRadioVM(INestedListSource nestedListSource)
		: base(nestedListSource)
	{
	}

	protected override void SubscribeNewItem(TViewModel entityViewModel)
	{
		base.SubscribeNewItem(entityViewModel);
		entityViewModel.IsSelected.Subscribe(entityViewModel.SetExpanded).AddTo(this);
		entityViewModel.IsExpanded.Subscribe(entityViewModel.SetSelected).AddTo(this);
	}

	protected override bool TryDoSelect(TViewModel viewModel)
	{
		if (NestedSelectedEntities.ContainsKey(viewModel.Source))
		{
			ReactiveProperty<NestedSelectionGroupEntityVM> reactiveProperty = NestedSelectedEntities[viewModel.Source];
			if (reactiveProperty.Value != viewModel)
			{
				reactiveProperty.Value = viewModel;
				return true;
			}
			return false;
		}
		return false;
	}

	protected override bool TryDoUnselectSelect(TViewModel viewModel)
	{
		if (NestedSelectedEntities.ContainsKey(viewModel.Source))
		{
			ReactiveProperty<NestedSelectionGroupEntityVM> reactiveProperty = NestedSelectedEntities[viewModel.Source];
			if (reactiveProperty.Value == viewModel)
			{
				reactiveProperty.Value = null;
				return true;
			}
			return false;
		}
		return false;
	}

	protected override void DoAddNestedList(INestedListSource source)
	{
		ReactiveProperty<NestedSelectionGroupEntityVM> selectedReactiveProperty = source.GetSelectedReactiveProperty();
		NestedSelectedEntities[source] = selectedReactiveProperty;
		selectedReactiveProperty.Subscribe(delegate
		{
			SetupNestedSelectedState(source);
		}).AddTo(this);
	}

	private void SetupNestedSelectedState(INestedListSource viewModel)
	{
		if (!NestedEntityCollections.ContainsKey(viewModel) || !NestedSelectedEntities.ContainsKey(viewModel))
		{
			return;
		}
		List<NestedSelectionGroupEntityVM> list = NestedEntityCollections[viewModel];
		ReactiveProperty<NestedSelectionGroupEntityVM> reactiveProperty = NestedSelectedEntities[viewModel];
		foreach (NestedSelectionGroupEntityVM item in list)
		{
			item.SetSelected(reactiveProperty.Value == item);
		}
	}

	protected override void DoRemoveNestedList(INestedListSource viewModel)
	{
		if (NestedSelectedEntities.ContainsKey(viewModel))
		{
			NestedSelectedEntities[viewModel].Dispose();
			NestedSelectedEntities.Remove(viewModel);
		}
	}
}
