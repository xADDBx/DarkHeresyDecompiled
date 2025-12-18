using Kingmaker.Settings;
using Kingmaker.UI.Sound;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class DialogAnswerPCView : DialogAnswerBaseView
{
	[SerializeField]
	private float m_DefaultFontSize = 20f;

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
			base.ViewModel?.PingAnswerHover(hover: true);
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}).AddTo(this);
		m_AnswerText.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel?.PingAnswerHover(hover: false);
		}).AddTo(this);
		SetTextFontSize(SettingsRoot.Accessiability.FontSizeMultiplier);
	}

	private void SetTextFontSize(float multiplier)
	{
		m_AnswerText.fontSize = m_DefaultFontSize * multiplier;
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public override void UpdateTextSize(float multiplier)
	{
		SetTextFontSize(multiplier);
		base.UpdateTextSize(multiplier);
	}

	[ContextMenu("DebugAnswer")]
	private void DebugAnswer()
	{
		base.ViewModel.DebugAnswer();
	}
}
