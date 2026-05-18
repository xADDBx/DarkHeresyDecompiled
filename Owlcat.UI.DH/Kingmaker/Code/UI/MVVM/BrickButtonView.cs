using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickButtonView : BrickBaseView<BrickButtonVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	protected TextMeshProUGUI m_Text;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_Text.text = base.ViewModel.Text;
		m_TextHelper.UpdateTextSize();
	}
}
