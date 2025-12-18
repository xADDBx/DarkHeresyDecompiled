using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class ProgressionBreadcrumbsItemCommonView : View<ProgressionBreadcrumbsItemVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label);
		}
		m_Button.SetActiveLayer(base.ViewModel.IsCurrent ? "Current" : "Default");
		m_Button.SetInteractable(!base.ViewModel.IsCurrent);
		m_Label.text = base.ViewModel.Text;
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.HandleClick();
		}).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}
}
