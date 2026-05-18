using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDamageBlockVM : ViewModel
{
	public enum IconType
	{
		None,
		Vital,
		Armor,
		Health
	}

	public struct PredictionData
	{
		public int Min;

		public int Max;

		public bool IsHeal;

		public IconType IconType;
	}

	private readonly OvertipDamageVisibilityVM m_VisibilityVM;

	private readonly ReactiveProperty<bool> m_HasHit = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<PredictionData> m_Prediction = new ReactiveProperty<PredictionData>();

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	public readonly MechanicEntityUIState EntityUIState;

	public ReadOnlyReactiveProperty<bool> HasHit => m_HasHit;

	public ReadOnlyReactiveProperty<PredictionData> Prediction => m_Prediction;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public OvertipDamageBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		m_VisibilityVM = new OvertipDamageVisibilityVM(mechanicEntityUIState).AddTo(this);
		EntityUIState = mechanicEntityUIState;
		EntityUIState.IsInCombat.CombineLatest(EntityUIState.IsVisibleForPlayer, EntityUIState.IsDeadOrUnconsciousIsDead, EntityUIState.Ability, EntityUIState.AbilityTargetUIData, EntityUIState.IsMouseOverUnit, EntityUIState.IsTarget, (bool _, bool _, bool _, AbilityData _, AbilityTargetUIData _, bool _, bool _) => new { }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			UpdateDamageVisibility();
		})
			.AddTo(this);
	}

	private void UpdateDamageVisibility()
	{
		bool state = m_VisibilityVM.IsVisible() && !EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.Damage);
		OnVisibilityChanged(state);
	}

	private void OnVisibilityChanged(bool state)
	{
		if (!state)
		{
			ClearProperties();
			m_IsVisible.Value = false;
		}
		else
		{
			UpdateProperties();
			m_IsVisible.Value = true;
		}
	}

	private void UpdateProperties()
	{
		ClearProperties();
		m_HasHit.Value = m_VisibilityVM.CanTargetByCurrentAbility();
		if (m_HasHit.CurrentValue)
		{
			AbilityTargetUIData currentValue = EntityUIState.AbilityTargetUIData.CurrentValue;
			UIDamagePredictionData damage = currentValue.Damage;
			UIHealPredictionData heal = currentValue.Heal;
			if (!damage.Equals(default(UIDamagePredictionData)))
			{
				m_Prediction.Value = new PredictionData
				{
					Min = damage.MinDamagePerAttack,
					Max = damage.MaxDamagePerAttack * currentValue.AttacksCount,
					IconType = GetDamageIconType(damage)
				};
			}
			else if (!heal.Equals(default(UIHealPredictionData)))
			{
				m_Prediction.Value = new PredictionData
				{
					Min = heal.MinHeal,
					Max = heal.MaxHeal,
					IsHeal = true,
					IconType = GetHealIconType(heal)
				};
			}
		}
	}

	private IconType GetDamageIconType(UIDamagePredictionData prediction)
	{
		if (prediction.VitalDamage > 0)
		{
			return IconType.Vital;
		}
		if (prediction.HPDamageBonus > 0 && prediction.HealthMaxDamage > 0)
		{
			return IconType.Health;
		}
		if (prediction.ArmorDamageBonus > 0 && prediction.ArmorMaxDamage > 0)
		{
			return IconType.Armor;
		}
		return IconType.None;
	}

	private IconType GetHealIconType(UIHealPredictionData prediction)
	{
		return prediction.HealStrategy switch
		{
			DamageStrategy.HealthOnly => IconType.Health, 
			DamageStrategy.ArmorOnly => IconType.Armor, 
			_ => IconType.None, 
		};
	}

	private void ClearProperties()
	{
		m_HasHit.Value = false;
		m_Prediction.Value = default(PredictionData);
	}
}
