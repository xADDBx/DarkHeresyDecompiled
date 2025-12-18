using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickButtonView : TooltipBaseBrickView<TooltipBrickButtonVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	protected TextMeshProUGUI m_Text;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_Text.text = base.ViewModel.Text;
	}
}
