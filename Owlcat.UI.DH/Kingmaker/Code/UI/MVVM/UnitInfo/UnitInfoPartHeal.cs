using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Mechanics.Damage;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartHeal : UnitInfoPart
{
	[SerializeField]
	private UnitInfoPartHeader m_Header;

	[SerializeField]
	private TextMeshProUGUI m_WarningMessage;

	[SerializeField]
	private UnitInfoPartElementView m_HPHeal;

	[SerializeField]
	private UnitInfoPartElementView m_ArmorHeal;

	private void Awake()
	{
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		m_Header.SetTitle(tooltips.TotalHeal.Text);
		m_WarningMessage.text = tooltips.HasNoArmorToRepair.Text;
		m_HPHeal.SetName(tooltips.HPHeal.Text);
		m_ArmorHeal.SetName(tooltips.ArmorHeal.Text);
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.AbilityIcon.Subscribe(delegate(Sprite value)
		{
			m_HPHeal.SetIcon(value);
			m_ArmorHeal.SetIcon(value);
		}).AddTo(this);
		base.ViewModel.Data.MinHeal.CombineLatest(base.ViewModel.Data.MaxHeal, base.ViewModel.Data.HealStrategy, base.ViewModel.Data.ArmorMax, (int min, int max, DamageStrategy strategy, int armorMax) => new { min, max, strategy, armorMax }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			string value2 = ((value.min == value.max) ? value.max.ToString() : $"{value.min}–{value.max}");
			m_Header.SetValue(value2);
			switch (value.strategy)
			{
			case DamageStrategy.Default:
				m_HPHeal.SetActive(active: false);
				m_ArmorHeal.SetActive(active: false);
				m_WarningMessage.gameObject.SetActive(value: false);
				break;
			case DamageStrategy.ArmorOnly:
				m_ArmorHeal.SetValue($"{value.max}");
				m_ArmorHeal.SetActive(active: true);
				m_HPHeal.SetActive(active: false);
				m_WarningMessage.gameObject.SetActive(value.armorMax <= 0);
				break;
			case DamageStrategy.HealthOnly:
				m_HPHeal.SetValue($"{value.max}");
				m_HPHeal.SetActive(active: true);
				m_ArmorHeal.SetActive(active: false);
				m_WarningMessage.gameObject.SetActive(value: false);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		})
			.AddTo(this);
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		base.gameObject.SetActive(state.HasHit && state.HasHeal);
	}
}
