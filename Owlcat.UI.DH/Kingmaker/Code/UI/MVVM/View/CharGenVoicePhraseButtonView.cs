using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoicePhraseButtonView : View<CharGenVoicePhraseButtonVM>
{
	[SerializeField]
	private TextMeshProUGUI m_LabelText;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	protected override void OnBind()
	{
		m_LabelText.text = base.ViewModel.Label;
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.PlayPhrase();
		}).AddTo(this);
		base.ViewModel.IsPlaying.Subscribe(delegate(bool v)
		{
			m_Button.SetActiveLayer(v ? 1 : 0);
		}).AddTo(this);
	}
}
