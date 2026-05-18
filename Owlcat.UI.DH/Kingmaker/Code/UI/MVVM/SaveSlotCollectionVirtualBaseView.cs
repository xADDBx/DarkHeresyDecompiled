using Kingmaker.Code.UI.MVVM.View;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotCollectionVirtualBaseView : View<SaveSlotCollectionVM>
{
	[Header("Views")]
	[SerializeField]
	protected SaveSlotsExpandableTitleView m_ExpandableTitleView;

	[SerializeField]
	private SaveSlotBaseView m_SaveSlotPrefab;

	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	private readonly ReactiveCommand<Unit> m_AttachedFirstValidView = new ReactiveCommand<Unit>();

	public Observable<Unit> AttachedFirstValidView => m_AttachedFirstValidView;

	public ObservableList<VirtualListElementVMBase> Saves => base.ViewModel.AllTitlesAndSlots;

	protected override void OnBind()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<SaveSlotsExpandableTitleVM>(m_ExpandableTitleView), new VirtualListElementTemplate<SaveSlotVM>(m_SaveSlotPrefab));
		CreateSlotGroups();
	}

	private void CreateSlotGroups()
	{
		m_VirtualList.Subscribe(base.ViewModel.AllTitlesAndSlots).AddTo(this);
		ScrollToTop();
	}

	public void ScrollToTop()
	{
		m_VirtualList.ScrollController.ForceScrollToTop();
	}
}
