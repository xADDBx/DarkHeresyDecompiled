using System;
using System.Collections.Generic;
using System.Globalization;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Localization;
using Kingmaker.Predictions;
using Kingmaker.RuleSystem.Rules.Modifiers;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartDamage : UnitInfoPart
{
	[SerializeField]
	private UnitInfoPartHeader m_Header;

	[Space]
	[SerializeField]
	private UnitInfoPartElementView m_Damage;

	[SerializeField]
	private UnitInfoPartElementView m_VitalDamage;

	[SerializeField]
	private UnitInfoPartElementView m_VitalDamageLockedByArmor;

	[SerializeField]
	private UnitInfoPartElementView m_VitalDamageLockedByStrategy;

	[SerializeField]
	private UnitInfoPartElementView m_Burst;

	[SerializeField]
	private UnitInfoPartElementView m_HPDamageBonus;

	[SerializeField]
	private UnitInfoPartElementView m_ArmorDamageBonus;

	[SerializeField]
	private UnitInfoPartElementView m_DamageReduction;

	[SerializeField]
	private UnitInfoPartElementView m_DamageModifiers;

	[SerializeField]
	private UnitInfoPartCompareElementView m_CompareTotalDamage;

	[Space]
	[SerializeField]
	private UnitInfoPartElementView m_ElementPrefab;

	[SerializeField]
	private Transform m_ElementsRoot;

	[SerializeField]
	private int m_PooledElementFirstIndex = 1;

	[SerializeField]
	private MonoBehaviour m_ProtectedByArmorHintSource;

	[SerializeField]
	private MonoBehaviour m_VitalDamageStrategyHintSource;

	[SerializeField]
	private Sprite m_DefaultBuffIcon;

	private List<UnitInfoPartElementView> m_PooledBuffElements = new List<UnitInfoPartElementView>();

	private List<UnitInfoPartElementView> m_PooledModifierElements = new List<UnitInfoPartElementView>();

	private IDisposable m_ProtectedByArmorHintDisposable;

	private IDisposable m_VitalDamageStrategyHintDisposable;

	protected override void OnBind()
	{
		base.OnBind();
		LocalizedString vitalDamageLockedByArmorHint = UIStrings.Instance.UnitInfo.VitalDamageLockedByArmorHint;
		LocalizedString vitalDamageLockedByStrategyHint = UIStrings.Instance.UnitInfo.VitalDamageLockedByStrategyHint;
		m_ProtectedByArmorHintDisposable = m_ProtectedByArmorHintSource.SetHint(vitalDamageLockedByArmorHint);
		m_VitalDamageStrategyHintDisposable = m_VitalDamageStrategyHintSource.SetHint(vitalDamageLockedByStrategyHint);
		base.ViewModel.Data.AbilityIcon.Subscribe(delegate(Sprite value)
		{
			m_Damage.SetIcon(value);
			m_DamageModifiers.SetIcon(value);
		}).AddTo(this);
		base.ViewModel.Data.BurstIndex.CombineLatest(base.ViewModel.IsPreciseAttack, (int burst, bool isPreciseAttack) => (burst: burst, isPreciseAttack: isPreciseAttack)).Subscribe(SetBurst).AddTo(this);
		base.ViewModel.Data.Damage.CombineLatest(base.ViewModel.Data.IsVitalBodyPart, (UnitInfoReactiveData.PredictedDamage damage, bool isVitalBodyPart) => new { damage, isVitalBodyPart }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(evt =>
		{
			int minDamagePerAttack = evt.damage.Prediction.MinDamagePerAttack;
			int overallMaxDamage = evt.damage.OverallMaxDamage;
			m_Header.SetValue($"{minDamagePerAttack}–{overallMaxDamage}");
			int baseMinDamage = evt.damage.Prediction.BaseMinDamage;
			int baseMaxDamage = evt.damage.Prediction.BaseMaxDamage;
			m_Damage.SetValue($"{baseMinDamage}–{baseMaxDamage}");
			int value2 = (evt.isVitalBodyPart ? evt.damage.Prediction.VitalDamage : 0);
			SetElementValue(m_VitalDamage, value2, UIUtilityText.GetModifierString);
			SetModifiers(evt.damage.Prediction.DamageModifiers);
			m_VitalDamageLockedByArmor.SetActive(evt.damage.Prediction.VitalDamageResult == VitalDamageResult.LockedByArmor);
			m_VitalDamageLockedByStrategy.SetActive(evt.damage.Prediction.VitalDamageResult == VitalDamageResult.LockedByStrategy);
		})
			.AddTo(this);
		base.ViewModel.Data.HPDamageBonus.CombineLatest(base.ViewModel.Data.ArmorDamageBonus, base.ViewModel.Data.ArmorMax, (int hpDamageBonus, int armorDamageBonus, int armorMax) => new { hpDamageBonus, armorDamageBonus, armorMax }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			m_HPDamageBonus.SetValue($"+{value.hpDamageBonus}");
			m_ArmorDamageBonus.SetValue($"+{value.armorDamageBonus}");
			m_HPDamageBonus.SetActive(value.hpDamageBonus > 0);
			m_ArmorDamageBonus.SetActive(value.armorDamageBonus > 0 && value.armorMax > 0);
		})
			.AddTo(this);
		m_HPDamageBonus.SetHint(UIStrings.Instance.Tooltips.HasHPDamageBonus.Text).AddTo(this);
		m_ArmorDamageBonus.SetHint(UIStrings.Instance.Tooltips.HasArmorDamageBonus.Text).AddTo(this);
		base.ViewModel.Data.DamageReduction.Subscribe(delegate(float value)
		{
			SetElementValue(m_DamageReduction, value, UIUtilityText.GetPercentString);
		}).AddTo(this);
		base.ViewModel.CompareData.Damage.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(UnitInfoReactiveData.PredictedDamage dmg)
		{
			UnitInfoReactiveData.PredictedDamage currentValue = base.ViewModel.Data.Damage.CurrentValue;
			bool valueChanged = currentValue.OverallMaxDamage != dmg.OverallMaxDamage;
			bool valueIncreased = dmg.OverallMaxDamage > currentValue.OverallMaxDamage;
			m_CompareTotalDamage.SetValue($"{dmg.Prediction.MinDamagePerAttack}–{dmg.OverallMaxDamage}");
			m_CompareTotalDamage.SetValueChangeMarker(valueChanged, valueIncreased);
		}).AddTo(this);
		base.ViewModel.DamageBuffOnCasterAdded.Subscribe(AddBuff).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.DamageBuffsOnCasterCleared, delegate
		{
			ClearBuffs();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_ProtectedByArmorHintDisposable?.Dispose();
		m_VitalDamageStrategyHintDisposable?.Dispose();
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		if (!(this == null))
		{
			base.gameObject.SetActive(state.HasHit && state.HasDamage && !state.HasHeal);
		}
	}

	private void SetElementValue(UnitInfoPartElementView element, int value, Func<int, string> format = null)
	{
		if (value == 0)
		{
			element.SetActive(active: false);
			return;
		}
		string value2 = ((format != null) ? format(value) : value.ToString());
		element.SetValue(value2);
		element.SetActive(active: true);
	}

	private void SetElementValue(UnitInfoPartElementView element, float value, Func<float, string> format = null)
	{
		if (Mathf.Abs(value) < float.Epsilon)
		{
			element.SetActive(active: false);
			return;
		}
		string value2 = ((format != null) ? format(value) : value.ToString(CultureInfo.InvariantCulture));
		element.SetValue(value2);
		element.SetActive(active: true);
	}

	private void SetBurst((int burst, bool isPreciseAttack) value)
	{
		m_Burst.SetValue($"x{value.burst}");
		m_Burst.SetActive(!value.isPreciseAttack);
	}

	private void SetModifiers(IReadOnlyList<Modifier> modifiers)
	{
	}

	private void AddBuff(CollectionAddEvent<UnitBuffUIInfo> evt)
	{
		if (evt.Value.Value > 0)
		{
			UnitInfoPartElementView widget = WidgetFactory.GetWidget(m_ElementPrefab, activate: false, strictMatching: true);
			Transform obj = widget.transform;
			obj.SetParent(m_ElementsRoot, worldPositionStays: false);
			obj.SetSiblingIndex(m_PooledElementFirstIndex + m_PooledBuffElements.Count);
			UnitBuffUIInfo value = evt.Value;
			widget.SetName(value.Name);
			Sprite icon = (value.Icon ? value.Icon : m_DefaultBuffIcon);
			widget.SetIcon(icon);
			string value2 = UIUtilityText.FormatModifier(value.Value, value.ModifierType);
			widget.SetValue(value2);
			widget.gameObject.SetActive(value: true);
			m_PooledBuffElements.Add(widget);
		}
	}

	private void ClearBuffs()
	{
		foreach (UnitInfoPartElementView pooledBuffElement in m_PooledBuffElements)
		{
			WidgetFactory.DisposeWidget(pooledBuffElement);
		}
		m_PooledBuffElements.Clear();
	}

	private void ClearModifiers()
	{
		foreach (UnitInfoPartElementView pooledModifierElement in m_PooledModifierElements)
		{
			WidgetFactory.DisposeWidget(pooledModifierElement);
		}
		m_PooledModifierElements.Clear();
	}

	protected override void OnDestroy()
	{
		ClearBuffs();
		ClearModifiers();
	}

	private void Awake()
	{
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		m_Header.SetTitle(tooltips.TotalDamage.Text);
		m_CompareTotalDamage.SetName(tooltips.TotalDamage.Text);
		m_Damage.SetName(tooltips.InitialDamage.Text);
		m_Burst.SetName(tooltips.PossibleHits.Text);
		m_DamageReduction.SetName(tooltips.DamageReduction.Text);
		TooltipBrickStrings tooltipBrickStrings = GameLogStrings.Instance.TooltipBrickStrings;
		m_VitalDamage.SetName(tooltipBrickStrings.VitalDamage.Text);
		m_HPDamageBonus.SetName(tooltipBrickStrings.AdditionalDamage.Text);
		m_ArmorDamageBonus.SetName(tooltipBrickStrings.BonusArmorDamage.Text);
		m_DamageModifiers.SetName(UIStrings.Instance.UnitInfo.DamageModifiers);
		m_VitalDamageLockedByArmor.SetName(UIStrings.Instance.UnitInfo.VitalDamageLockedByArmor);
		m_VitalDamageLockedByStrategy.SetName(UIStrings.Instance.UnitInfo.VitalDamageLockedByStrategy);
	}
}
