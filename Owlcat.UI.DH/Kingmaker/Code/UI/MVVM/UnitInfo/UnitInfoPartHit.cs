using System;
using System.Globalization;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartHit : UnitInfoPart
{
	private enum FormatType
	{
		Default,
		Percent,
		Multiplication
	}

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_HitChance;

	[SerializeField]
	private UnitInfoPartElementView m_InitialHitChance;

	[SerializeField]
	private UnitInfoPartElementView m_CriticalEffectsAvoidanceChance;

	[SerializeField]
	private UnitInfoPartElementView m_DefenceChance;

	[SerializeField]
	private UnitInfoPartElementView m_CoverChance;

	[SerializeField]
	private UnitInfoPartElementView m_OverpenetrationChance;

	[SerializeField]
	private UnitInfoPartCompareElementView m_CompareHitChance;

	private void Awake()
	{
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		m_HeaderLabel.text = tooltips.TotalHitChance.Text;
		m_CompareHitChance.SetName(tooltips.TotalHitChance.Text);
		m_InitialHitChance.SetName(tooltips.InitialHitChance.Text);
		m_CriticalEffectsAvoidanceChance.SetName(tooltips.CriticalEffectsAvoidanceChance.Text);
		m_DefenceChance.SetName(tooltips.DefencePenalty.Text);
		m_CoverChance.SetName(tooltips.Cover.Text);
		m_OverpenetrationChance.SetName(tooltips.Overpenetration.Text);
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.HitChance.Subscribe(delegate(float value)
		{
			m_HitChance.text = Mathf.Round(value).ToString();
		}).AddTo(this);
		base.ViewModel.Data.InitialHitChance.Subscribe(delegate(float value)
		{
			SetHitChanceBlockElement(m_InitialHitChance, value, show: true);
		}).AddTo(this);
		base.ViewModel.Data.AbilityIcon.Subscribe(delegate(Sprite value)
		{
			m_InitialHitChance.SetIcon(value);
		}).AddTo(this);
		base.ViewModel.Data.CriticalEffectsAvoidanceChance.Subscribe(delegate(float value)
		{
			SetHitChanceBlockElement(m_CriticalEffectsAvoidanceChance, value, value > 0f);
		}).AddTo(this);
		base.ViewModel.Data.DefenceChance.Subscribe(delegate(float value)
		{
			SetHitChanceBlockElement(m_DefenceChance, (100f - value) / 100f, value > 0f, FormatType.Multiplication);
		}).AddTo(this);
		base.ViewModel.Data.CoverChance.Subscribe(delegate(float value)
		{
			SetHitChanceBlockElement(m_CoverChance, (100f - value) / 100f, value > 0f, FormatType.Multiplication);
		}).AddTo(this);
		base.ViewModel.Data.OverpenetrationChance.Subscribe(delegate(float value)
		{
			SetHitChanceBlockElement(m_OverpenetrationChance, value / 100f, value > 0f, FormatType.Multiplication);
		}).AddTo(this);
		base.ViewModel.Data.PreciseAttackHasNoTarget.Subscribe(SetPreciseAttackHasNoTarget).AddTo(this);
		base.ViewModel.CompareData.HitChance.Subscribe(delegate(float value)
		{
			float currentValue = base.ViewModel.Data.HitChance.CurrentValue;
			bool valueChanged = Mathf.Abs(currentValue - value) > float.Epsilon;
			bool valueIncreased = value > currentValue;
			m_CompareHitChance.SetValue(UIUtilityText.GetPercentString(value));
			m_CompareHitChance.SetValueChangeMarker(valueChanged, valueIncreased);
		}).AddTo(this);
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		bool active = (state.HasHit && !state.HasHeal) || state.PreciseAttackHasNoTarget;
		base.gameObject.SetActive(active);
	}

	private void SetHitChanceBlockElement(UnitInfoPartElementView elementView, float value, bool show, FormatType type = FormatType.Percent)
	{
		if (!show)
		{
			elementView.SetActive(active: false);
			return;
		}
		elementView.SetValue(GetFormattedValue(value, type));
		elementView.SetActive(active: true);
	}

	private string GetFormattedValue(float value, FormatType formatType)
	{
		return formatType switch
		{
			FormatType.Default => value.ToString(CultureInfo.InvariantCulture), 
			FormatType.Percent => UIUtilityText.GetPercentString(value), 
			FormatType.Multiplication => "×" + value.ToString(CultureInfo.InvariantCulture), 
			_ => throw new ArgumentOutOfRangeException("formatType", formatType, null), 
		};
	}

	private void SetPreciseAttackHasNoTarget(bool preciseAttackHasNoTarget)
	{
		m_HeaderLabel.text = (preciseAttackHasNoTarget ? LocalizedTexts.Instance.PreciseAttack.PreciseAttackHeader.Text : UIStrings.Instance.Tooltips.TotalHitChance.Text);
	}
}
