using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Inspect;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarConsoleView : View<ActionBarVM>, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Parts")]
	[SerializeField]
	private ActionBarPartQuickAccessConsoleView m_QuickAccessConsoleView;

	[SerializeField]
	private ActionBarPartAbilitiesConsoleView m_AbilitiesConsoleView;

	[SerializeField]
	private VeilThicknessConsoleView m_VeilThicknessConsoleView;

	[Header("Alerts")]
	[SerializeField]
	private Image m_ClearMPAlertGroup;

	[SerializeField]
	private Image m_AttackAbilityGroupCooldownAlertGroup;

	private readonly ReactiveProperty<bool> m_IsInspectVisible = new ReactiveProperty<bool>();

	[Header("Hints")]
	[SerializeField]
	private HintView m_InspectHint;

	[SerializeField]
	private HintView m_HideActionBarHint;

	[SerializeField]
	private HintView m_ShowActionBarHint;

	[Header("ContainersForMarkers")]
	[SerializeField]
	private GameObject[] m_ContainersForMarkers;

	[Header("Other")]
	[SerializeField]
	private TextMeshProUGUI m_AnotherPlayerTurnLabel;

	private ReadOnlyReactiveProperty<bool> m_IsVisible;

	private ReactiveProperty<bool> m_VisibleTrigger;

	public void Initialize()
	{
		m_QuickAccessConsoleView.Initialize();
		m_AbilitiesConsoleView.Initialize();
		m_VeilThicknessConsoleView.Initialize();
	}

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		m_VisibleTrigger = new ReactiveProperty<bool>();
		m_IsVisible = base.ViewModel.IsVisible.And(m_VisibleTrigger).ToReadOnlyReactiveProperty(initialValue: false);
		m_QuickAccessConsoleView.Bind(base.ViewModel);
		m_AbilitiesConsoleView.Bind(base.ViewModel.Abilities);
		m_VeilThicknessConsoleView.Bind(base.ViewModel.VeilThickness);
		HandleTurnBasedModeSwitched(TurnController.IsInTurnBasedCombat());
		m_IsVisible.Subscribe(OnVisibleChanged).AddTo(this);
		base.ViewModel.CurrentCombatUnit.Subscribe(delegate
		{
			if (m_IsVisible.CurrentValue)
			{
				CombatSounds.Instance.ActionBar.ActionBarSwitch.Play();
			}
		}).AddTo(this);
		base.ViewModel.EndTurnText.Subscribe(delegate(string value)
		{
			m_ClearMPAlertGroup.gameObject.SetActive(!string.IsNullOrEmpty(value));
		}).AddTo(this);
		base.ViewModel.IsAttackAbilityGroupCooldownAlertActive.Subscribe(m_AttackAbilityGroupCooldownAlertGroup.gameObject.SetActive).AddTo(this);
		base.ViewModel.HighlightedUnit.Subscribe(OnHighlightedUnit).AddTo(this);
		base.ViewModel.IsNotControllableCharacter.CombineLatest(base.ViewModel.ControllablePlayerNickname, (bool notControllable, string playerNickName) => new { notControllable, playerNickName }).Subscribe(value =>
		{
			m_AnotherPlayerTurnLabel.transform.parent.parent.gameObject.SetActive(value.notControllable);
			m_AnotherPlayerTurnLabel.text = ((!string.IsNullOrWhiteSpace(value.playerNickName)) ? string.Format(UIStrings.Instance.ActionBar.AnotherPlayerTurnWithNickname, value.playerNickName) : ((string)UIStrings.Instance.ActionBar.AnotherPlayerTurn));
		}).AddTo(this);
	}

	public void AddInput()
	{
	}

	private void TriggerVisibility(bool trigger)
	{
		if (!(Game.Instance.CursorController.SelectedAbility != null))
		{
			m_VisibleTrigger.Value = trigger;
		}
	}

	private void OnInspectUnit()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.HighlightedUnit.CurrentValue.EntityData);
		});
	}

	private void OnVisibleChanged(bool visible)
	{
		if (visible)
		{
			m_Animator.AppearAnimation();
			CombatSounds.Instance.ActionBar.Show.Play();
			m_ContainersForMarkers.ForEach(delegate(GameObject c)
			{
				c.Or(null)?.SetActive(value: true);
			});
		}
		else
		{
			m_Animator.DisappearAnimation();
			CombatSounds.Instance.ActionBar.Hide.Play();
			m_ContainersForMarkers.ForEach(delegate(GameObject c)
			{
				c.Or(null)?.SetActive(value: false);
			});
		}
	}

	public void OnHighlightedUnit(AbstractUnitEntityView unit)
	{
		m_IsInspectVisible.Value = unit != null && InspectUnitsHelper.IsInspectAllow(unit.EntityData);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TriggerVisibility(isTurnBased);
	}

	public void HandleTurnBasedModeResumed()
	{
		HandleTurnBasedModeSwitched(isTurnBased: true);
	}
}
