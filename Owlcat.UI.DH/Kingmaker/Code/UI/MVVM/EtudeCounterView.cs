using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EtudeCounterView : View<EtudeCounterVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void OnBind()
	{
		base.ViewModel.CounterText.Subscribe(delegate(string text)
		{
			m_Label.text = text;
			if (!string.IsNullOrEmpty(text))
			{
				m_FadeAnimator.AppearAnimation();
			}
			else
			{
				m_FadeAnimator.DisappearAnimation();
			}
		}).AddTo(this);
	}
}
