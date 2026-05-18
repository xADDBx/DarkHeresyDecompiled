using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Owlcat.UI;

public abstract class SelectionGroupView<TToggleGroupVM, TEntityVM, TEntityView> : View<TToggleGroupVM> where TToggleGroupVM : SelectionGroupVM<TEntityVM> where TEntityVM : SelectionGroupEntityVM where TEntityView : SelectionGroupEntityView<TEntityVM>
{
	[Header("Virtual List")]
	[SerializeField]
	[UsedImplicitly]
	protected VirtualListComponent VirtualList;

	[SerializeField]
	[UsedImplicitly]
	public TEntityView SlotPrefab;

	[SerializeField]
	[UsedImplicitly]
	public bool HasSorter;

	private List<TEntityVM> m_SortedCollection = new List<TEntityVM>();

	private bool m_IsInit;

	public virtual void Initialize()
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
		if (HasSorter)
		{
			base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
			{
				ApplySorting();
			}).AddTo(this);
			ApplySorting();
			VirtualList.Subscribe(new ObservableList<TEntityVM>(m_SortedCollection)).AddTo(this);
		}
		else
		{
			VirtualList.Subscribe(base.ViewModel.EntitiesCollection).AddTo(this);
		}
		VirtualList.ScrollController.ForceScrollToTop();
	}

	public void ApplySorting()
	{
		m_SortedCollection = base.ViewModel.EntitiesCollection.ToList();
		m_SortedCollection.Sort(EntityComparer);
	}

	protected virtual int EntityComparer(TEntityVM x, TEntityVM y)
	{
		return 0;
	}

	public virtual bool TrySelectEntity(TEntityView entityView)
	{
		return base.ViewModel.TrySelectEntity(entityView.ViewModel);
	}

	public virtual bool SelectFirstValidEntity()
	{
		return base.ViewModel.TrySelectFirstValidEntity();
	}

	public virtual bool SelectLastValidEntity()
	{
		return base.ViewModel.TrySelectLastValidEntity();
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

	public IConsoleEntity GetView<TData>(TData viewModel)
	{
		return VirtualList.TryGetNavigationEntity(viewModel);
	}
}
