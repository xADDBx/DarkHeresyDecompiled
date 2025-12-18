using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class NewInteractionVariantToggleView : SelectionGroupEntityView<NewInteractionVariantToggleVM>
{
	[Header("Images")]
	[SerializeField]
	private TextMeshProUGUI m_ActionName;

	[Header("Hint")]
	[SerializeField]
	private FadeAnimator m_HintAnimator;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	[SerializeField]
	private LayoutElement m_HintLayoutElement;

	[SerializeField]
	private float m_HintMaxWidth = 380f;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.InteractionName.Subscribe(delegate(string text)
		{
			m_ActionName.text = text;
		}).AddTo(this);
		m_Button.Interactable = !base.ViewModel.Disabled;
		base.gameObject.name = "ToggleVariantView " + base.ViewModel.InteractionName.CurrentValue + " " + base.ViewModel.ResourceName;
		m_HintAnimator.CanvasGroup.alpha = 0f;
		ObservableSubscribeExtensions.Subscribe(m_Button.OnSingleLeftClickAsObservable(), delegate
		{
			base.ViewModel.Interact();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnSingleLeftClickNotInteractableAsObservable(), delegate
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
