using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ServoSkullBarkView : View<ServoSkullBarkVM>
{
	[SerializeField]
	private TMP_Text m_BarkText;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void OnBind()
	{
		m_FadeAnimator.DisappearInstant();
		base.ViewModel.Bark.Subscribe(HandleBarkTextChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearInstant();
		base.gameObject.SetActive(value: false);
	}

	private void HandleBarkTextChanged(string text)
	{
		if (text != null)
		{
			m_BarkText.text = text;
			m_FadeAnimator.PlayAnimation(value: true);
		}
		else
		{
			m_FadeAnimator.PlayAnimation(value: false);
		}
	}
}
