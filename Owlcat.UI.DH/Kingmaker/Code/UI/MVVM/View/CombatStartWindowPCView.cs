using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Utility.GameConst;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatStartWindowPCView : CombatStartWindowView
{
	[SerializeField]
	protected CombatStartPartyPCView m_PartyView;

	[SerializeField]
	private Image m_StartCombatButtonFillImage;

	[SerializeField]
	private TextMeshProUGUI m_StartCombatButtonBindText;

	[SerializeField]
	private float m_StartCombatButtonHoldDuration = 1f;

	private Coroutine m_StartCombatHoldCoroutine;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.PartyVM.Subscribe(m_PartyView.Bind);
		m_StartBattleButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.StartBattle).AddTo(this);
		SetStartBattleSettings();
		m_StartCombatButtonBindText.text = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(UISettingsRoot.Instance.UIKeybindGeneralSettings.EndPreparationTurn.name + UIConsts.SuffixOn));
	}

	private void SetStartBattleSettings()
	{
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.EndPreparationTurn.name + UIConsts.SuffixOn, delegate
		{
			HoldToStartBattle(isHold: true);
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.EndPreparationTurn.name + UIConsts.SuffixOff, delegate
		{
			HoldToStartBattle(isHold: false);
		}).AddTo(this);
	}

	private void HoldToStartBattle(bool isHold)
	{
		if (!base.ViewModel.CanStartCombat.CurrentValue)
		{
			return;
		}
		if (isHold)
		{
			m_StartBattleButton.SetActiveLayer("Hold");
			m_StartCombatHoldCoroutine = StartCoroutine(StartCombatHoldCoroutine());
			return;
		}
		if (m_StartCombatHoldCoroutine != null)
		{
			StopCoroutine(m_StartCombatHoldCoroutine);
			m_StartCombatHoldCoroutine = null;
		}
		m_StartCombatButtonFillImage.fillAmount = 0f;
		m_StartBattleButton.SetActiveLayer("Interactable");
	}

	private IEnumerator StartCombatHoldCoroutine()
	{
		float timer = 0f;
		while (timer < m_StartCombatButtonHoldDuration)
		{
			m_StartCombatButtonFillImage.fillAmount = Mathf.Clamp01(timer / m_StartCombatButtonHoldDuration);
			timer += Time.deltaTime;
			yield return null;
		}
		base.ViewModel.StartBattle();
		m_StartCombatButtonFillImage.fillAmount = 0f;
		m_StartBattleButton.SetActiveLayer("Interactable");
	}
}
