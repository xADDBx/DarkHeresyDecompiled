using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatEndWindowPCView : CombatEndWindowView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatMultiButton m_CombatEndButton;

	[SerializeField]
	private TextMeshProUGUI m_CombatEndButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_CombatContinueButton;

	[SerializeField]
	private TextMeshProUGUI m_CombatContinueButtonLabel;

	protected override void Awake()
	{
		base.Awake();
		m_CombatEndButtonLabel.text = UICombatEndWindowTexts.Instance.CombatEndButton.Text;
		m_CombatContinueButtonLabel.text = UICombatEndWindowTexts.Instance.CombatContinueButton.Text;
	}

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_CombatEndButton.OnLeftClickAsObservable(), delegate
		{
			Close(endCombat: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CombatContinueButton.OnLeftClickAsObservable(), delegate
		{
			Close(endCombat: false);
		}).AddTo(this);
	}

	protected override void SetCombatEndReasonImpl(CombatEndReason combatEndReason)
	{
		m_CombatContinueButton.gameObject.SetActive(combatEndReason == CombatEndReason.MoraleVictory);
	}
}
