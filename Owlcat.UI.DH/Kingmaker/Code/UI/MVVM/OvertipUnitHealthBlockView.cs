using System;
using DG.Tweening;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipUnitHealthBlockView : View<OvertipHealthBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private float m_SmallFontSize;

	[SerializeField]
	private float m_BigFontSize;

	[Header("Health")]
	[SerializeField]
	private Image m_HPBackground;

	[SerializeField]
	private Slider m_HPMaxSlider;

	[SerializeField]
	private FilledDelayedSlider m_HPLeftSlider;

	[SerializeField]
	private FilledRangedSlider m_HPMaxDamageSlider;

	[SerializeField]
	private FilledRangedSlider m_HPMaxHealSlider;

	[SerializeField]
	private TextMeshProUGUI m_HPLabel;

	[SerializeField]
	private CanvasGroup m_HPLabelCanvasGroup;

	[Header("Armor")]
	[SerializeField]
	private CanvasGroup m_ArmorGroup;

	[SerializeField]
	private Image m_HPArmorBackground;

	[SerializeField]
	private Slider m_ArmorMaxSlider;

	[SerializeField]
	private FilledDelayedSlider m_ArmorLeftSlider;

	[SerializeField]
	private FilledRangedSlider m_ArmorMaxDamageSlider;

	[SerializeField]
	private FilledRangedSlider m_ArmorMaxHealSlider;

	[SerializeField]
	private TextMeshProUGUI m_ArmorLabel;

	[SerializeField]
	private CanvasGroup m_ArmorLabelCanvasGroup;

	[Header("Death")]
	[SerializeField]
	private CanvasGroup m_DeathMark;

	private Tweener m_FadeTween;

	private Tweener m_SizeTween;

	private bool m_IsBinding;

	private bool CheckDestructible
	{
		get
		{
			if (!base.ViewModel.MechanicEntityUIState.IsDestructible.CurrentValue)
			{
				return true;
			}
			if (!base.ViewModel.MechanicEntityUIState.IsTBM.CurrentValue && !base.ViewModel.VisibleInExploration)
			{
				return false;
			}
			if (base.ViewModel.MechanicEntityUIState.IsDestructibleNotCover.CurrentValue)
			{
				return true;
			}
			if (base.ViewModel.MechanicEntityUIState.Ability.CurrentValue != null && base.ViewModel.MechanicEntityUIState.IsTarget.CurrentValue)
			{
				return true;
			}
			return base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CurrentValue;
		}
	}

	private bool IsEnemyInPreparationTurn
	{
		get
		{
			if (base.ViewModel.MechanicEntityUIState.IsEnemy.CurrentValue)
			{
				return base.ViewModel.MechanicEntityUIState.IsPreparationTurn.CurrentValue;
			}
			return false;
		}
	}

	private bool IsVisible
	{
		get
		{
			if (!base.ViewModel.MechanicEntityUIState.HasHiddenCondition.CurrentValue && (base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue || IsEnemyInPreparationTurn || base.ViewModel.VisibleInExploration) && !base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
			{
				return CheckDestructible;
			}
			return false;
		}
	}

	public void HideInstant()
	{
		m_FadeTween?.Kill();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		m_IsBinding = true;
		base.ViewModel.MechanicEntityUIState.IsEnemy.CombineLatest(base.ViewModel.MechanicEntityUIState.IsPlayer, base.ViewModel.MechanicEntityUIState.IsDestructible, base.ViewModel.MechanicEntityUIState.IsCover, (bool enemy, bool player, bool destructible, bool cover) => new { enemy, player, destructible, cover }).Subscribe(value =>
		{
			int activeLayer = ((value.destructible || value.cover) ? 3 : ((!value.player) ? (value.enemy ? 1 : 2) : 0));
			m_MultiSelectable.SetActiveLayer(activeLayer);
		}).AddTo(this);
		base.ViewModel.HitPointMax.Subscribe(delegate(int value)
		{
			m_HPMaxSlider.maxValue = value;
		}).AddTo(this);
		base.ViewModel.HitPointLeft.Subscribe(delegate(int value)
		{
			m_HPLeftSlider.SetValue(value, !m_IsBinding);
			m_HPLabel.text = (base.ViewModel.HideRealHealthInUI ? "???" : value.ToString());
		}).AddTo(this);
		base.ViewModel.CanDie.Subscribe(delegate(bool value)
		{
			m_DeathMark.alpha = (value ? 1 : 0);
			m_DeathMark.blocksRaycasts = value;
		}).AddTo(this);
		base.ViewModel.ArmorMax.Subscribe(delegate(int value)
		{
			SetArmorVisuals(value > 0);
			m_ArmorMaxSlider.maxValue = value;
		}).AddTo(this);
		base.ViewModel.ArmorLeft.Subscribe(delegate(int value)
		{
			m_ArmorLeftSlider.SetValue(value, !m_IsBinding);
			m_ArmorLabel.text = (base.ViewModel.HideRealHealthInUI ? "???" : value.ToString());
		}).AddTo(this);
		base.ViewModel.MaxDamage.Subscribe(SetDamagePredict).AddTo(this);
		base.ViewModel.MaxHeal.CombineLatest(base.ViewModel.HealStrategy, (int heal, DamageStrategy strategy) => (heal: heal, strategy: strategy)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(SetHealPredict)
			.AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsEnemy.CombineLatest(base.ViewModel.MechanicEntityUIState.IsInCombat, base.ViewModel.MechanicEntityUIState.HasHiddenCondition, base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead, base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, (bool _, bool _, bool _, bool _, bool _, bool _) => true).Subscribe(delegate
		{
			DoVisibility();
		}).AddTo(this);
		base.ViewModel.HitChanceBlockVisible.Subscribe(delegate(bool hitChanceVisible)
		{
			(int, int) currentValue = base.ViewModel.MaxDamage.CurrentValue;
			bool flag = currentValue.Item1 != 0 || currentValue.Item2 != 0;
			m_ArmorLabelCanvasGroup.alpha = ((flag || hitChanceVisible) ? 0f : 1f);
		}).AddTo(this);
		m_IsBinding = false;
	}

	protected override void OnUnbind()
	{
		m_FadeTween?.Kill();
		m_SizeTween?.Kill();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
	}

	private void DoVisibility()
	{
		float endValue = (IsVisible ? 1f : 0f);
		m_FadeTween?.Kill();
		m_FadeTween = m_CanvasGroup.DOFade(endValue, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_CanvasGroup.blocksRaycasts = IsVisible;
	}

	private void SetArmorVisuals(bool hasArmor)
	{
		m_ArmorGroup.alpha = (hasArmor ? 1f : 0f);
		m_HPBackground.color = ((!hasArmor) ? Color.white : Color.clear);
		m_HPArmorBackground.color = (hasArmor ? Color.white : Color.clear);
		m_HPLabel.fontSize = (hasArmor ? m_SmallFontSize : m_BigFontSize);
	}

	private void SetDamagePredict((int health, int armor) damage)
	{
		if (damage.health == 0 && damage.armor == 0)
		{
			m_HPLabelCanvasGroup.alpha = 1f;
			m_ArmorLabelCanvasGroup.alpha = 1f;
			m_ArmorMaxDamageSlider.Clear();
			m_HPMaxDamageSlider.Clear();
			return;
		}
		m_HPLabelCanvasGroup.alpha = 0f;
		m_ArmorLabelCanvasGroup.alpha = 0f;
		int currentValue = base.ViewModel.ArmorLeft.CurrentValue;
		int currentValue2 = base.ViewModel.HitPointLeft.CurrentValue;
		int num = Mathf.Max(0, currentValue - damage.armor);
		int num2 = Mathf.Max(0, currentValue2 - damage.health);
		m_ArmorMaxDamageSlider.SetRange(num, currentValue, blink: true);
		m_HPMaxDamageSlider.SetRange(num2, currentValue2, blink: true);
	}

	private void SetHealPredict((int amount, DamageStrategy strategy) heal)
	{
		if (heal.amount == 0)
		{
			m_HPLabelCanvasGroup.alpha = 1f;
			m_ArmorLabelCanvasGroup.alpha = 1f;
			m_ArmorMaxHealSlider.Clear();
			m_HPMaxHealSlider.Clear();
			return;
		}
		m_HPLabelCanvasGroup.alpha = 0f;
		m_ArmorLabelCanvasGroup.alpha = 0f;
		int currentValue = base.ViewModel.ArmorLeft.CurrentValue;
		int currentValue2 = base.ViewModel.ArmorMax.CurrentValue;
		int currentValue3 = base.ViewModel.HitPointLeft.CurrentValue;
		int currentValue4 = base.ViewModel.HitPointMax.CurrentValue;
		switch (heal.strategy)
		{
		case DamageStrategy.ArmorOnly:
			m_ArmorMaxHealSlider.SetRange(currentValue, Mathf.Min(currentValue + heal.amount, currentValue2), blink: true);
			break;
		case DamageStrategy.HealthOnly:
			m_HPMaxHealSlider.SetRange(currentValue3, Mathf.Min(currentValue3 + heal.amount, currentValue4), blink: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("strategy", heal.strategy, null);
		case DamageStrategy.Default:
			break;
		}
	}

	private void Awake()
	{
		m_HPLeftSlider.Initialize();
		m_ArmorLeftSlider.Initialize();
		m_HPMaxDamageSlider.Initialize();
		m_HPMaxHealSlider.Initialize();
		m_ArmorMaxDamageSlider.Initialize();
		m_ArmorMaxHealSlider.Initialize();
	}
}
