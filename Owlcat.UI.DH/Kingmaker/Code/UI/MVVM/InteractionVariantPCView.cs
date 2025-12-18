using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionVariantPCView : InteractionVariantView
{
	[Header("Hint")]
	[SerializeField]
	private FadeAnimator m_HintAnimator;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	[SerializeField]
	private LayoutElement m_HintLayoutElement;

	[SerializeField]
	private CombatHintEntityView m_CombatHintEntityView;

	[SerializeField]
	private float m_HintMaxWidth = 380f;

	protected override void OnBind()
	{
		base.OnBind();
		m_HintAnimator.CanvasGroup.alpha = 0f;
		ObservableSubscribeExtensions.Subscribe(m_Button.OnSingleLeftClickAsObservable(), delegate
		{
			base.ViewModel.Interact();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateHint, delegate
		{
			m_HintText.text = base.ViewModel.GetHint();
		}).AddTo(this);
		m_HintText.text = base.ViewModel.GetHint();
		this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ShowHint();
		}).AddTo(this);
		this.OnPointerExitAsObservable().Subscribe(delegate
		{
			HideHint();
		}).AddTo(this);
		base.ViewModel.HaveNotEnoughAPinTBM.CombineLatest(base.ViewModel.CanInteract, (bool ap, bool canInteract) => new { ap, canInteract }).Subscribe(value =>
		{
			m_Button.Interactable = !value.ap && value.canInteract;
		}).AddTo(this);
		base.ViewModel.CombatHintEntityVM.Subscribe(m_CombatHintEntityView.Bind).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		HideHint();
	}

	private void ShowHint()
	{
		if (!string.IsNullOrEmpty(m_HintText.text))
		{
			m_HintLayoutElement.preferredWidth = ((m_HintLayoutElement.GetComponent<RectTransform>().rect.width >= m_HintMaxWidth) ? m_HintMaxWidth : (-1f));
			m_HintAnimator.AppearAnimation();
		}
	}

	private void HideHint()
	{
		if (!string.IsNullOrEmpty(m_HintText.text))
		{
			m_HintAnimator.DisappearAnimation();
		}
	}
}
