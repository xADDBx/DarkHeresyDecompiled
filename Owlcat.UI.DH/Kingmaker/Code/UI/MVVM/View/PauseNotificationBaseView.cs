using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class PauseNotificationBaseView : View<PauseNotificationVM>
{
	[SerializeField]
	private CanvasGroup m_PauseBlock;

	[SerializeField]
	private TextMeshProUGUI m_PauseText;

	private Tweener m_Animation;

	protected override void OnBind()
	{
		m_PauseText.text = UIStrings.Instance.CommonTexts.Pause;
		ChangeAlphaPause(base.ViewModel.IsPaused.CurrentValue);
		base.ViewModel.IsPaused.Subscribe(PlayPause).AddTo(this);
		base.ViewModel.ChangeAlphaPause.Subscribe(ChangeAlphaPause).AddTo(this);
	}

	private void PlayPause(bool state)
	{
		if (state)
		{
			UISounds.Instance.Sounds.Systems.PauseSound.Play();
		}
		m_PauseBlock.interactable = state;
		m_PauseBlock.blocksRaycasts = state;
		m_Animation?.Kill();
		m_Animation = m_PauseBlock.DOFade(state ? 1f : 0f, 0.2f).SetUpdate(isIndependentUpdate: true).SetDelay(state ? 0.2f : 0.0001f);
	}

	private void ChangeAlphaPause(bool state)
	{
		m_PauseBlock.interactable = state;
		m_PauseBlock.blocksRaycasts = state;
		m_PauseBlock.alpha = (state ? 1f : 0f);
	}
}
