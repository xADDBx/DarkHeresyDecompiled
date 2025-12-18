using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatEndWindowView : View<CombatEndWindowVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[SerializeField]
	private TextMeshProUGUI m_XpLabel;

	protected virtual void Awake()
	{
		m_Animator.Initialize();
		m_HeaderLabel.text = UICombatEndWindowTexts.Instance.VictoryTitle.Text;
	}

	protected override void OnBind()
	{
		m_Animator.AppearAnimation();
		base.ViewModel.CombatEndReason.Subscribe(SetCombatEndReason).AddTo(this);
		base.ViewModel.GainedXp.Subscribe(SetGainedXp).AddTo(this);
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnUnbind()
	{
		m_Animator.DisappearAnimation();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	protected void Close(bool endCombat)
	{
		base.ViewModel.Close(endCombat);
	}

	private void SetCombatEndReason(CombatEndReason combatEndReason)
	{
		m_DescriptionLabel.text = UICombatEndWindowTexts.Instance.GetDescriptionText(combatEndReason);
		SetCombatEndReasonImpl(combatEndReason);
	}

	protected virtual void SetCombatEndReasonImpl(CombatEndReason combatEndReason)
	{
	}

	private void SetGainedXp(int gainedXp)
	{
		m_XpLabel.text = "";
	}
}
