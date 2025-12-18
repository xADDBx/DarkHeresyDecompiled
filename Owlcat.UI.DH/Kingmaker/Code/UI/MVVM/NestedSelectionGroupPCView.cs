using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class NestedSelectionGroupPCView<TToggleGroupVM, TEntityVM, TEntityView> : View<TToggleGroupVM> where TToggleGroupVM : NestedSelectionGroupVM<TEntityVM> where TEntityVM : NestedSelectionGroupEntityVM where TEntityView : NestedSelectionGroupEntityPCView<TEntityVM>
{
	[Header("Virtual List")]
	[SerializeField]
	[UsedImplicitly]
	protected VirtualListComponent VirtualList;

	[SerializeField]
	[UsedImplicitly]
	public List<TEntityView> SlotPrefabs;

	protected ObservableList<TEntityVM> VisibleCollection = new ObservableList<TEntityVM>();

	private bool m_IsInit;

	public abstract bool HasSorter { get; }

	public abstract bool HasFilter { get; }

	private void Initialize()
	{
		if (!m_IsInit)
		{
			IVirtualListElementTemplate[] array = SlotPrefabs.Select((TEntityView prefab) => new VirtualListElementTemplate<TEntityVM>(prefab, SlotPrefabs.IndexOf(prefab))).ToArray();
			IVirtualListElementTemplate[] templates = array;
			VirtualList.Initialize(templates);
			m_IsInit = true;
		}
	}

	protected void ClearListView()
	{
		VisibleCollection.Clear();
	}

	protected override void OnBind()
	{
		Initialize();
		ClearListView();
		base.ViewModel.NestedEntityCollections.ObserveCountChanged().Subscribe(delegate
		{
			OnCollectionChanged();
		}).AddTo(this);
		OnCollectionChanged();
		VirtualList.Subscribe(VisibleCollection).AddTo(this);
		TryScrollToSelectedElement();
	}

	protected void TryScrollToSelectedElement()
	{
		NestedSelectionGroupEntityVM nestedSelectionGroupEntityVM = base.ViewModel.NestedEntityCollections.Last().Value?.FirstOrDefault((NestedSelectionGroupEntityVM el) => el.IsSelected.CurrentValue);
		if (nestedSelectionGroupEntityVM != null && VisibleCollection.Contains(nestedSelectionGroupEntityVM))
		{
			VirtualList.ScrollController.ForceScrollToElement(nestedSelectionGroupEntityVM);
		}
		else
		{
			VirtualList.ScrollController.ForceScrollToTop();
		}
	}

	protected void OnCollectionChanged()
	{
		List<TEntityVM> list = new List<TEntityVM>();
		foreach (KeyValuePair<INestedListSource, List<NestedSelectionGroupEntityVM>> nestedEntityCollection in base.ViewModel.NestedEntityCollections)
		{
			int num = list.IndexOf(nestedEntityCollection.Key as TEntityVM);
			List<TEntityVM> list2 = nestedEntityCollection.Value.Select((NestedSelectionGroupEntityVM ent) => (TEntityVM)ent).ToList();
			if (HasFilter)
			{
				list2 = list2.Where((TEntityVM entity) => IsVisible(entity)).ToList();
			}
			if (HasSorter)
			{
				list2.Sort(EntityComparer);
			}
			list.InsertRange(num + 1, list2);
		}
		List<TEntityVM> list3 = VisibleCollection.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			TEntityVM item = list[i];
			int num2 = VisibleCollection.IndexOf(item);
			if (num2 < 0)
			{
				VisibleCollection.Insert(i, item);
				continue;
			}
			list3.Remove(item);
			VisibleCollection.Move(num2, i);
		}
		foreach (TEntityVM item2 in list3)
		{
			VisibleCollection.Remove(item2);
		}
	}

	protected abstract bool IsVisible(TEntityVM entity);

	protected abstract int EntityComparer(TEntityVM a, TEntityVM b);

	public bool TryScrollToLastSelectedEntity()
	{
		return TryScrollToEntity(base.ViewModel.LastChangedEntity.CurrentValue);
	}

	private bool TryScrollToEntity(TEntityVM entityVM)
	{
		VirtualList.ScrollController.ForceScrollToElement(entityVM);
		return true;
	}
}
