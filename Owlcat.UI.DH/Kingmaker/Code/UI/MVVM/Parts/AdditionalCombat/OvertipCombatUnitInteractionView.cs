using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Interaction;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Parts.AdditionalCombat;

public class OvertipCombatUnitInteractionView : View<OvertipCombatUnitInteractionVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private CanvasGroup m_NewIcon;

	[SerializeField]
	private CanvasGroup m_LabelGroup;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Interaction.CombineLatest(base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, (IUnitInteraction interaction, bool hover, bool key) => new { interaction, hover, key }).Subscribe(value =>
		{
			bool flag = (value.key || value.hover) && value.interaction != null && !string.IsNullOrEmpty(value.interaction.DisplayName?.Text);
			m_LabelGroup.alpha = (flag ? 1f : 0f);
			m_Text.text = value.interaction?.DisplayName?.Text;
		}).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			m_NewIcon.alpha = (value ? 1f : 0f);
		}).AddTo(this);
		m_Animator.AppearAnimation();
	}

	protected override void OnUnbind()
	{
		HideInstant();
	}

	public void HideInstant()
	{
		m_Animator.DisappearInstant();
	}
}
