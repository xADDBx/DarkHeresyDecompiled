using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SelectionGroupViewWithFilterPCView<TToggleGroupVM, TEntityVM, TEntityView> : View<TToggleGroupVM> where TToggleGroupVM : SelectionGroupVM<TEntityVM> where TEntityVM : SelectionGroupEntityVM where TEntityView : SelectionGroupEntityView<TEntityVM>
{
	[Header("Own Rect")]
	[SerializeField]
	[UsedImplicitly]
	protected RectTransform OwnRect;

	[Header("Virtual List")]
	[SerializeField]
	[UsedImplicitly]
	protected VirtualListComponent VirtualList;

	[SerializeField]
	[UsedImplicitly]
	public TEntityView SlotPrefab;

	protected ObservableList<TEntityVM> VisibleCollection = new ObservableList<TEntityVM>();

	private bool m_IsInit;

	public abstract bool HasSorter { get; }

	public abstract bool HasFilter { get; }

	private void Initialize()
	{
		if (!m_IsInit)
		{
			VirtualList.Initialize(new VirtualListElementTemplate<TEntityVM>(SlotPrefab));
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Initialize();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			OnCollectionChanged();
		}).AddTo(this);
		OnCollectionChanged();
		VirtualList.Subscribe(VisibleCollection).AddTo(this);
		TryScrollToSelectedElement();
	}

	protected void TryScrollToSelectedElement()
	{
		TEntityVM val = base.ViewModel.EntitiesCollection.FirstOrDefault((TEntityVM el) => el.IsSelected.Value);
		if (val != null && VisibleCollection.Contains(val))
		{
			VirtualList.ScrollController.ForceScrollToElement(val);
		}
		else
		{
			VirtualList.ScrollController.ForceScrollToTop();
		}
	}

	protected void OnCollectionChanged()
	{
		List<TEntityVM> list = base.ViewModel.EntitiesCollection.Select((TEntityVM ent) => ent).ToList();
		if (HasFilter)
		{
			list = list.Where((TEntityVM entity) => IsVisible(entity)).ToList();
		}
		if (HasSorter)
		{
			list.Sort(EntityComparer);
		}
		List<TEntityVM> list2 = VisibleCollection.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			TEntityVM item = list[i];
			int num = VisibleCollection.IndexOf(item);
			if (num < 0)
			{
				VisibleCollection.Insert(i, item);
				continue;
			}
			list2.Remove(item);
			VisibleCollection.Move(num, i);
		}
		foreach (TEntityVM item2 in list2)
		{
			VisibleCollection.Remove(item2);
		}
	}

	protected abstract bool IsVisible(TEntityVM entity);

	protected abstract int EntityComparer(TEntityVM a, TEntityVM b);

	public virtual void SelectFirstValidEntity()
	{
		base.ViewModel.TrySelectFirstValidEntity();
	}

	public virtual void SelectLastValidEntity()
	{
		base.ViewModel.TrySelectLastValidEntity();
	}

	public bool TryScrollToLastSelectedEntity()
	{
		return TryScrollToEntity(base.ViewModel.LastChangedEntity.Value);
	}

	private bool TryScrollToEntity(TEntityVM entityVM)
	{
		VirtualList.ScrollController.ForceScrollToElement(entityVM);
		return true;
	}
}
