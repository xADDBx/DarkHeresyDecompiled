using Kingmaker.Settings;
using Kingmaker.UI.Sound;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventAnswerPCView : BookEventAnswerView
{
	[SerializeField]
	private float m_DefaultFontSize = 18f;

	protected override void OnBind()
	{
		base.OnBind();
		m_AnswerText.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (data.button == PointerEventData.InputButton.Left)
			{
				Confirm();
			}
		}).AddTo(this);
		m_AnswerText.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ButtonsSounds.Instance.Default.Hover.Play();
		}).AddTo(this);
		m_AnswerText.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel?.PingAnswerHover(hover: false);
		}).AddTo(this);
		m_AnswerText.fontSize = m_DefaultFontSize * SettingsRoot.Accessiability.FontSizeMultiplier;
	}
}
