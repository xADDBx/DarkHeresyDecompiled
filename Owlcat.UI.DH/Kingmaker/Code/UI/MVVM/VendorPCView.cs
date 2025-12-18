using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorPCView : VendorView<InventoryStashPCView, ItemsFilterBaseView, VendorLevelItemsPCView, VendorTransitionWindowPCView>
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	protected override void OnBind()
	{
		base.OnBind();
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
	}
}
