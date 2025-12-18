using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FeedbackPopupItemPCView : View<FeedbackPopupItemVM>
{
	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel.Label;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.HandleClick();
		}).AddTo(this);
	}
}
