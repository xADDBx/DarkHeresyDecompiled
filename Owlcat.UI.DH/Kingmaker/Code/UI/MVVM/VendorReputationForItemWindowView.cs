using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationForItemWindowView : View<VendorReputationForItemWindowVM>
{
	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[SerializeField]
	private VirtualListElementViewBase<VendorReputationForItemVM> m_ReputationForItemPrefab;

	public void Initialize()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<VendorReputationForItemVM>(m_ReputationForItemPrefab));
	}

	protected override void OnBind()
	{
		m_VirtualList.Subscribe(base.ViewModel.AcceptItems).AddTo(this);
	}
}
