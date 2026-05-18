using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class TutorialWindowPCView<TViewModel> : TutorialWindowBaseView<TViewModel> where TViewModel : TutorialWindowVM
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	protected OwlcatMultiButton m_ConfirmButton;

	[SerializeField]
	protected TextMeshProUGUI m_ConfirmButtonText;

	[SerializeField]
	private float m_TitleDefaultSize = 23f;

	[SerializeField]
	private float m_TriggerDefaultSize = 24f;

	[SerializeField]
	private float m_MainTextsDefaultSize = 21f;

	[SerializeField]
	private float m_ConfirmButtonDefaultSize = 20f;

	private UITutorial m_UITutorial;

	protected override void OnBind()
	{
		base.OnBind();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		OwlcatR3UnitExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Hide();
		}).AddTo(this);
		m_DontShowToggle.OnPointerClickAsObservable().Subscribe(delegate
		{
			ModalWindowsSounds.Instance.Tutorial.BanTutorialType.Play();
		}).AddTo(this);
	}

	protected override void SetTextsSize(float multiplier)
	{
		m_Title.fontSize = m_TitleDefaultSize * multiplier;
		m_TriggerText.fontSize = m_TriggerDefaultSize * multiplier;
		m_TutorialText.fontSize = m_MainTextsDefaultSize * multiplier;
		m_SolutionText.fontSize = m_MainTextsDefaultSize * multiplier;
		m_ConfirmButtonText.fontSize = m_ConfirmButtonDefaultSize * multiplier;
		base.SetTextsSize(multiplier);
	}
}
