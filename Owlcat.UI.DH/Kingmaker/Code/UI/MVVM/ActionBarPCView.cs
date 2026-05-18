using System;
using System.Collections;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.UI.MVVM.Morale;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using Kingmaker.View;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPCView : View<ActionBarVM>
{
	[SerializeField]
	private MoveSequenceAnimator m_Animator;

	[SerializeField]
	private FadeAnimator m_AdditionalAnimator;

	[SerializeField]
	private CanvasGroup m_PlayerTurnComponents;

	[SerializeField]
	private CanvasGroup m_EnemyTurnComponents;

	[SerializeField]
	private FadeAnimator m_CommonComponentsAnimator;

	[Header("End Turn")]
	[SerializeField]
	private OwlcatMultiButton m_EndTurnButton;

	[SerializeField]
	private FadeAnimator m_EndTurnButtonFadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_EndTurnButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_EndTurnBindText;

	[SerializeField]
	private Image m_EndTurnButtonFillImage;

	[SerializeField]
	private float m_EndTurnButtonHoldDuration = 1f;

	private Coroutine m_EndTurnHoldCoroutine;

	[Header("Speed Up")]
	[SerializeField]
	private OwlcatMultiButton m_SpeedUpButton;

	[SerializeField]
	private FadeAnimator m_SpeedUpButtonFadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_SpeedUpButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_SpeedingUpLabel;

	[SerializeField]
	private TextMeshProUGUI m_SpeedUpBindText;

	[SerializeField]
	private CanvasGroup m_SpeedUpButtonDefaultGroup;

	[SerializeField]
	private CanvasGroup m_SpeedUpButtonSpeedingUpGroup;

	[Header("Exploration")]
	[SerializeField]
	private OwlcatMultiButton m_CompassButton;

	[SerializeField]
	private OwlcatMultiButton m_ForceShowActionBarButton;

	[SerializeField]
	private FadeAnimator m_ExplorationButtonsFadeAnimator;

	[SerializeField]
	private RectTransform m_Arrow;

	[Header("Parts")]
	[SerializeField]
	private ActionBarUnitView m_SurfaceCombatCurrentUnitView;

	[SerializeField]
	private ActionBarPartConsumablesPCView m_ConsumablesView;

	[SerializeField]
	private ActionBarPartWeaponsPCView m_WeaponsView;

	[SerializeField]
	private ActionBarPartAbilitiesPCView m_AbilitiesView;

	[SerializeField]
	private VeilThicknessPCView m_VeilThicknessView;

	[SerializeField]
	private ActionBarMoraleView m_ActionBarMoraleView;

	[SerializeField]
	private GameObject m_ContainerForMarkers;

	[Header("Alerts")]
	[SerializeField]
	private Image m_ClearMPAlertGroup;

	[SerializeField]
	private Image m_AttackAbilityGroupCooldownAlertGroup;

	[Header("Other")]
	[SerializeField]
	private TextMeshProUGUI m_AnotherPlayerTurnLabel;

	private SettingsEntityKeyBindingPair m_PauseBind;

	private float m_InitialAnimatorDissapearPosX;

	private void Awake()
	{
		m_EndTurnButtonLabel.text = UIStrings.Instance.Tooltips.EndTurn.Text;
		m_SpeedUpButtonLabel.text = UIStrings.Instance.Tooltips.SpeedUpEnemies.Text;
		m_SpeedingUpLabel.text = UIStrings.Instance.Tooltips.SpeedingUp.Text;
		m_InitialAnimatorDissapearPosX = m_Animator.GetDisappearPosition().x;
		m_AdditionalAnimator.SetAlwaysActive(state: true);
		m_ConsumablesView.Initialize();
		m_WeaponsView.Initialize();
		m_AbilitiesView.Initialize();
		m_VeilThicknessView.Initialize();
		m_PlayerTurnComponents.gameObject.SetActive(value: true);
		m_PlayerTurnComponents.alpha = 0f;
		m_PlayerTurnComponents.interactable = false;
		m_EnemyTurnComponents.gameObject.SetActive(value: true);
		m_EnemyTurnComponents.alpha = 0f;
		m_EnemyTurnComponents.interactable = false;
	}

	protected override void OnBind()
	{
		m_ConsumablesView.Bind(base.ViewModel.Consumables);
		m_WeaponsView.Bind(base.ViewModel.Weapons);
		m_AbilitiesView.Bind(base.ViewModel.Abilities);
		m_VeilThicknessView.Bind(base.ViewModel.VeilThickness);
		m_ActionBarMoraleView.Bind(base.ViewModel.Morale);
		EventBus.Subscribe(this).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(OnVisibleChanged).AddTo(this);
		base.ViewModel.ForceHideCurrentUnit.CombineLatest(base.ViewModel.HUDContext.IsPlayer, delegate(bool forceHide, bool isPlayer)
		{
			bool item = isPlayer && !forceHide;
			bool item2 = !isPlayer && !forceHide;
			return (showPlayer: item, showEnemy: item2);
		}).Subscribe(delegate((bool showPlayer, bool showEnemy) t)
		{
			SetTurnComponentsVisible(t.showPlayer, t.showEnemy);
		}).AddTo(this);
		base.ViewModel.CurrentCombatUnit.Subscribe(delegate(CombatMechanicEntityVM currentUnit)
		{
			m_SurfaceCombatCurrentUnitView.Bind(currentUnit);
			if (base.ViewModel.IsVisible.CurrentValue)
			{
				CombatSounds.Instance.ActionBar.ActionBarSwitch.Play();
			}
		}).AddTo(this);
		RootUIContext.Instance.GameUIState.GameMode.Subscribe(delegate(GameModeType value)
		{
			if (value == GameModeType.Cutscene)
			{
				m_CommonComponentsAnimator.DisappearAnimation();
			}
			else
			{
				m_CommonComponentsAnimator.AppearAnimation();
			}
		}).AddTo(this);
		base.ViewModel.HUDContext.IsTurnBasedActive.And(base.ViewModel.HUDContext.CanEndTurn).Subscribe(delegate(bool val)
		{
			m_EndTurnButton.SetInteractable(val);
			m_EndTurnButton.SetActiveLayer(val ? "Interactable" : "NonInteractable");
		}).AddTo(this);
		m_EndTurnButton.OnLeftClickAsObservable().Subscribe(DoEndTurn).AddTo(this);
		base.ViewModel.EndTurnText.Subscribe(delegate(string value)
		{
			m_ClearMPAlertGroup.gameObject.SetActive(!string.IsNullOrEmpty(value));
		}).AddTo(this);
		base.ViewModel.CompassAngle.Subscribe(delegate(float value)
		{
			m_Arrow.eulerAngles = new Vector3(0f, 0f, 0f - value);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CompassButton.OnLeftClickAsObservable(), delegate
		{
			CameraRig.Instance.ResetCameraRotate();
		}).AddTo(this);
		base.ViewModel.IsAttackAbilityGroupCooldownAlertActive.Subscribe(m_AttackAbilityGroupCooldownAlertGroup.gameObject.SetActive).AddTo(this);
		m_AttackAbilityGroupCooldownAlertGroup.SetHint(UIStrings.Instance.Tooltips.AttackAbilityGroupCooldown).AddTo(this);
		m_ClearMPAlertGroup.SetHint(UIStrings.Instance.Tooltips.SpendAllMovementPoints).AddTo(this);
		base.ViewModel.IsNotControllableCharacter.CombineLatest(base.ViewModel.ControllablePlayerNickname, (bool notControllable, string playerNickName) => new { notControllable, playerNickName }).Subscribe(value =>
		{
			m_AnotherPlayerTurnLabel.transform.parent.parent.gameObject.SetActive(value.notControllable);
			m_AnotherPlayerTurnLabel.text = ((!string.IsNullOrWhiteSpace(value.playerNickName)) ? string.Format(UIStrings.Instance.ActionBar.AnotherPlayerTurnWithNickname, value.playerNickName) : ((string)UIStrings.Instance.ActionBar.AnotherPlayerTurn));
		}).AddTo(this);
		base.ViewModel.HUDContext.IsTurnBasedActive.CombineLatest(base.ViewModel.HUDContext.DeploymentPhase, base.ViewModel.HUDContext.ShowEndTurn, base.ViewModel.HUDContext.PartyUnitIsRunningCommand, (bool isTurnBased, bool deploymentPhase, bool showEndTurn, bool currentUnitIsRunningCommand) => new { isTurnBased, deploymentPhase, showEndTurn, currentUnitIsRunningCommand }).Subscribe(v =>
		{
			SpeedUp(state: false);
			EndTurn(isHold: false);
			ShowButtons(v.isTurnBased, v.deploymentPhase, v.showEndTurn, v.currentUnitIsRunningCommand);
		}).AddTo(this);
		m_PauseBind = SettingsRoot.Controls.Keybindings.General.Pause;
		m_PauseBind.OnValueChanged += SetButtonsBindTexts;
		SetButtonsBindTexts();
		m_ForceShowActionBarButton.OnLeftClickAsObservable().Subscribe(ForceShowActionBar).AddTo(this);
		base.ViewModel.IsForcedMinimized.Subscribe(UpdateForceShowButtonLayer).AddTo(this);
	}

	protected override void OnUnbind()
	{
		if (m_EndTurnHoldCoroutine != null)
		{
			StopCoroutine(m_EndTurnHoldCoroutine);
			m_EndTurnHoldCoroutine = null;
		}
		if (m_PauseBind != null)
		{
			m_PauseBind.OnValueChanged -= SetButtonsBindTexts;
			m_PauseBind = null;
		}
		SetExplorationKeybinding(needKeybinding: false);
		SetEndTurnKeybinding(needKeybinding: false);
		SetSpeedUpKeybinding(needKeybinding: false);
		m_ConsumablesView.Unbind();
		m_WeaponsView.Unbind();
		m_AbilitiesView.Unbind();
		m_VeilThicknessView.Unbind();
		m_ActionBarMoraleView.Unbind();
	}

	private void OnVisibleChanged(bool visible)
	{
		if (visible)
		{
			DoAppear();
		}
		else
		{
			DoDisappear();
		}
	}

	private void UpdateAnimatorDisappearPosition()
	{
		RectTransform rectTransform = (RectTransform)m_Animator.gameObject.transform;
		float num = ((RectTransform)m_Animator.gameObject.transform.parent).rect.width - rectTransform.rect.width;
		m_Animator.SetDisappearPosition(new Vector2(m_InitialAnimatorDissapearPosX - num / 2f, 0f));
	}

	private void DoAppear()
	{
		m_Animator.AppearAnimation();
		m_AdditionalAnimator.AppearAnimation();
		m_ContainerForMarkers.SetActive(value: true);
		CombatSounds.Instance.ActionBar.Show.Play();
	}

	private void DoDisappear()
	{
		UpdateAnimatorDisappearPosition();
		m_Animator.DisappearAnimation();
		m_AdditionalAnimator.DisappearAnimation();
		m_ContainerForMarkers.SetActive(value: false);
		m_ClearMPAlertGroup.gameObject.SetActive(value: false);
		m_AttackAbilityGroupCooldownAlertGroup.gameObject.SetActive(value: false);
		CombatSounds.Instance.ActionBar.Hide.Play();
	}

	private void SetTurnComponentsVisible(bool playerTurnVisible, bool enemyTurnVisible)
	{
		m_PlayerTurnComponents.alpha = (playerTurnVisible ? 1f : 0f);
		m_PlayerTurnComponents.interactable = playerTurnVisible;
		m_EnemyTurnComponents.alpha = (enemyTurnVisible ? 1f : 0f);
		m_EnemyTurnComponents.interactable = enemyTurnVisible;
	}

	private void ShowButtons(bool isTurnBased, bool deploymentPhase, bool showEndTurn, bool currentUnitIsRunningCommand)
	{
		bool flag = !isTurnBased;
		bool flag2 = isTurnBased && showEndTurn && !currentUnitIsRunningCommand;
		bool flag3 = isTurnBased && ((!showEndTurn && !deploymentPhase) || currentUnitIsRunningCommand);
		ShowButton(m_ExplorationButtonsFadeAnimator, flag);
		ShowButton(m_EndTurnButtonFadeAnimator, flag2);
		ShowButton(m_SpeedUpButtonFadeAnimator, flag3);
		SetExplorationKeybinding(flag);
		SetEndTurnKeybinding(flag2);
		SetSpeedUpKeybinding(flag3);
	}

	private static void ShowButton(FadeAnimator animator, bool show)
	{
		if (!(animator == null))
		{
			if (show)
			{
				animator.AppearAnimation();
			}
			else
			{
				animator.DisappearAnimation();
			}
		}
	}

	private void SetEndTurnKeybinding(bool needKeybinding)
	{
		if (!needKeybinding)
		{
			ApplyEndTurnBindAction(Game.Instance.Keyboard.Unbind);
			return;
		}
		ApplyEndTurnBindAction(delegate(string s, Action action)
		{
			Game.Instance.Keyboard.Bind(s, action);
		});
	}

	private void ApplyEndTurnBindAction(Action<string, Action> bindAction)
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		bindAction(uIKeybindGeneralSettings.EndTurn.name + UIConsts.SuffixOn, delegate
		{
			EndTurn(isHold: true);
		});
		bindAction(uIKeybindGeneralSettings.EndTurn.name + UIConsts.SuffixOff, delegate
		{
			EndTurn(isHold: false);
		});
	}

	private void SetSpeedUpKeybinding(bool needKeybinding)
	{
		if (!needKeybinding)
		{
			ApplySpeedUpBindAction(Game.Instance.Keyboard.Unbind);
			return;
		}
		ApplySpeedUpBindAction(delegate(string s, Action action)
		{
			Game.Instance.Keyboard.Bind(s, action);
		});
	}

	private void ApplySpeedUpBindAction(Action<string, Action> bindAction)
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		bindAction(uIKeybindGeneralSettings.SpeedUpEnemiesTurn.name + UIConsts.SuffixOn, delegate
		{
			SpeedUp(state: true);
		});
		bindAction(uIKeybindGeneralSettings.SpeedUpEnemiesTurn.name + UIConsts.SuffixOff, delegate
		{
			SpeedUp(state: false);
		});
	}

	private void SetExplorationKeybinding(bool needKeybinding)
	{
		if (!needKeybinding)
		{
			ApplyExplorationBindAction(Game.Instance.Keyboard.Unbind);
			return;
		}
		ApplyExplorationBindAction(delegate(string s, Action action)
		{
			Game.Instance.Keyboard.Bind(s, action);
		});
	}

	private void ApplyExplorationBindAction(Action<string, Action> bindAction)
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		bindAction(uIKeybindGeneralSettings.CameraRotateToPointNorth.name, CameraRig.Instance.ResetCameraRotate);
	}

	private void SetButtonsBindTexts(KeyBindingPair keyBindingPair = default(KeyBindingPair))
	{
		m_EndTurnButton.SetHint(UIStrings.Instance.Tooltips.EndTurn, "EndTurnOn").AddTo(this);
		m_EndTurnBindText.text = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("EndTurnOn"));
		m_SpeedUpButton.SetHint(UIStrings.Instance.Tooltips.SpeedUpEnemies, "SpeedUpEnemiesOn").AddTo(this);
		m_SpeedUpBindText.text = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("SpeedUpEnemiesOn"));
	}

	private void SpeedUp(bool state)
	{
		if (!base.ViewModel.CanSpeedUp)
		{
			ResetSpeedUp();
			return;
		}
		if (state)
		{
			m_SpeedUpButtonDefaultGroup.alpha = 0f;
			m_SpeedUpButtonSpeedingUpGroup.DOFade(1f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			ResetSpeedUp();
		}
		Game.Instance.SpeedUp(state);
	}

	private void ResetSpeedUp()
	{
		m_SpeedUpButtonDefaultGroup.alpha = 1f;
		DOTween.Kill(m_SpeedUpButtonSpeedingUpGroup);
		m_SpeedUpButtonSpeedingUpGroup.alpha = 0f;
	}

	private void ForceShowActionBar()
	{
		EventBus.RaiseEvent(delegate(IForceShowActionBarUIHandler h)
		{
			h.HandleForceShowActionBar(!base.ViewModel.IsVisible.CurrentValue);
		});
	}

	private void EndTurn(bool isHold)
	{
		if (base.ViewModel.CanEndTurn)
		{
			if (isHold && m_EndTurnHoldCoroutine == null)
			{
				m_EndTurnButton.SetActiveLayer("Hold");
				m_EndTurnHoldCoroutine = StartCoroutine(EndTurnHoldCoroutine());
			}
			else if (m_EndTurnHoldCoroutine != null)
			{
				StopCoroutine(m_EndTurnHoldCoroutine);
				m_EndTurnHoldCoroutine = null;
				m_EndTurnButtonFillImage.fillAmount = 0f;
				m_EndTurnButton.SetActiveLayer("Interactable");
			}
		}
	}

	private IEnumerator EndTurnHoldCoroutine()
	{
		float timer = 0f;
		while (timer < m_EndTurnButtonHoldDuration)
		{
			m_EndTurnButtonFillImage.fillAmount = Mathf.Clamp01(timer / m_EndTurnButtonHoldDuration);
			timer += Time.deltaTime;
			yield return null;
		}
		DoEndTurn();
		m_EndTurnButtonFillImage.fillAmount = 0f;
		m_EndTurnButton.SetActiveLayer("Interactable");
	}

	private void DoEndTurn()
	{
		base.ViewModel.HUDContext.EndTurn();
		CombatSounds.Instance.Combat.EndTurn.Play();
	}

	private void UpdateForceShowButtonLayer(bool isForceHidden)
	{
		m_ForceShowActionBarButton.SetActiveLayer(isForceHidden ? 2 : 0);
	}
}
