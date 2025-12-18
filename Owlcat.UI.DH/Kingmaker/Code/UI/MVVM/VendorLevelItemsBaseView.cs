using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorLevelItemsBaseView : View<VendorLevelItemsVM>
{
	[SerializeField]
	private VendorSlotView m_VendorSlotView;

	[SerializeField]
	private OwlcatMultiButton m_LockButton;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	protected TextMeshProUGUI m_LevelValue;

	[SerializeField]
	protected Image m_FillAmount;

	[SerializeField]
	protected GameObject m_LastItemMark;

	protected override void OnBind()
	{
		DrawEntities();
		m_LockButton.SetActiveLayer(base.ViewModel.ReputationLevelVM.Locked ? "Locked" : "Unlocked");
		m_LastItemMark.Or(null)?.SetActive(base.ViewModel.IsLastList);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.VendorSlots, m_VendorSlotView);
	}
}
