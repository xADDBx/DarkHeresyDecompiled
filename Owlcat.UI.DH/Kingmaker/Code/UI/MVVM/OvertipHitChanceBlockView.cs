using System.Globalization;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipHitChanceBlockView : View<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private GameObject m_HitChanceBlock;

	[SerializeField]
	private GameObject m_AbilityBlock;

	[SerializeField]
	private BuffsGroupWidget m_CriticalEffectMarker;

	[SerializeField]
	private Image m_Ability;

	[SerializeField]
	private TextMeshProUGUI m_HitChance;

	[ShowIf("m_HasInitialHitChance")]
	[SerializeField]
	private TextMeshProUGUI m_InitialChance;

	[SerializeField]
	private bool m_HasInitialHitChance = true;

	[ShowIf("m_HasGlitchEffect")]
	[SerializeField]
	private SpriteGlitchSurfaceOvertip m_Glitch;

	[SerializeField]
	private bool m_HasGlitchEffect = true;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Space]
	[SerializeField]
	private Color m_LowChanceColor;

	[SerializeField]
	private Color m_MediumChanceColor;

	[SerializeField]
	private Color m_HighChanceColor;

	private bool m_IsVisible;

	public void HideInstant()
	{
		m_FadeAnimator.DisappearInstant();
		m_HitChanceBlock.SetActive(value: false);
		m_AbilityBlock.SetActive(value: false);
		m_IsVisible = false;
	}

	protected override void OnBind()
	{
		HideInstant();
		base.ViewModel.IsVisible.Subscribe(HandleVisibilityChanged).AddTo(this);
		base.ViewModel.HitChance.Subscribe(UpdateHitChance).AddTo(this);
		base.ViewModel.InitialHitChance.Subscribe(UpdateInitialHitChance).AddTo(this);
	}

	private void HandleVisibilityChanged(bool state)
	{
		if (state != m_IsVisible || m_IsVisible)
		{
			UpdateVisuals(state, base.ViewModel.Ability);
			UpdateInitialHitChance(base.ViewModel.InitialHitChance.CurrentValue);
			UpdateHitChance(base.ViewModel.HitChance.CurrentValue);
		}
	}

	private void UpdateVisibility(bool isVisible)
	{
		m_IsVisible = isVisible;
		m_FadeAnimator.PlayAnimation(isVisible);
	}

	private void UpdateVisuals(bool isVisible, AbilityData ability)
	{
		isVisible = isVisible && ability != null;
		if (!isVisible)
		{
			UpdateVisibility(isVisible: false);
			return;
		}
		UpdateVisualState(ability);
		UpdateGlitch();
		UpdateVisibility(isVisible: true);
	}

	private void UpdateVisualState(AbilityData ability)
	{
		float currentValue = base.ViewModel.HitChance.CurrentValue;
		bool flag = currentValue >= 0f && currentValue <= 100f;
		bool currentValue2 = base.ViewModel.EntityUIState.HoverSelfTargetAbility.CurrentValue;
		bool currentValue3 = base.ViewModel.IsCaster.CurrentValue;
		bool isThrow = ability.IsThrow;
		if (!base.ViewModel.IsAttack || !flag || currentValue2 || currentValue3 || ability.IsPrecise || isThrow || base.ViewModel.HitAlways.CurrentValue)
		{
			SetAbilityState(ability.Icon);
		}
		else
		{
			SetHitChanceState();
		}
	}

	private void UpdateGlitch()
	{
		if (m_HasGlitchEffect)
		{
			if (base.ViewModel.HitChance.CurrentValue >= 0f)
			{
				m_Glitch.SetIntensivity(100f - base.ViewModel.HitChance.CurrentValue);
			}
			else
			{
				m_Glitch.SetLowGlitch();
			}
		}
	}

	private void SetAbilityState(Sprite abilityIcon)
	{
		m_HitChanceBlock.SetActive(value: false);
		m_Ability.sprite = abilityIcon;
		m_AbilityBlock.SetActive(value: true);
	}

	private void SetHitChanceState()
	{
		m_AbilityBlock.SetActive(value: false);
		m_HitChanceBlock.SetActive(value: true);
	}

	private void UpdateHitChance(float value)
	{
		if (m_IsVisible)
		{
			int value2 = Mathf.RoundToInt(value);
			m_HitChance.SetText(value2.ToString());
			m_HitChance.color = GetHitChanceTextColor(value2);
			UpdateCriticalEffect(base.ViewModel.AffectedByCriticalEffect);
		}
	}

	private Color GetHitChanceTextColor(int value)
	{
		if (value > 10)
		{
			if (value <= 50)
			{
				return m_MediumChanceColor;
			}
			return m_HighChanceColor;
		}
		return m_LowChanceColor;
	}

	private void UpdateInitialHitChance(float value)
	{
		if (m_IsVisible && m_HasInitialHitChance)
		{
			m_InitialChance.SetText(value.ToString(CultureInfo.InvariantCulture));
		}
	}

	private void UpdateCriticalEffect(bool hasCritical)
	{
		if (!hasCritical)
		{
			m_CriticalEffectMarker.SetActive(isActive: false);
			return;
		}
		CriticalEffectsUIData casterCriticalEffects = base.ViewModel.GetCasterCriticalEffects();
		if (casterCriticalEffects.Count < 1)
		{
			m_CriticalEffectMarker.SetActive(isActive: false);
			return;
		}
		string activeLayer = ((casterCriticalEffects.Count > 1) ? $"Multiple_{casterCriticalEffects.HighestRank}" : $"Single_{casterCriticalEffects.HighestRank}");
		m_CriticalEffectMarker.SetActiveLayer(activeLayer);
		m_CriticalEffectMarker.SetActive(isActive: true);
	}
}
