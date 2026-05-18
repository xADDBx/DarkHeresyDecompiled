using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponSetPCView : View<CharInfoWeaponSetViewData>
{
	[Header("Common Block")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Type;

	[SerializeField]
	private TextMeshProUGUI m_AbilityShortName;

	[SerializeField]
	private HandSlotView m_PrimaryHandView;

	[Header("Stats Block")]
	[SerializeField]
	private TextMeshProUGUI m_DamageLabel;

	[SerializeField]
	private TextMeshProUGUI m_VitalDamageValue;

	[SerializeField]
	private TextMeshProUGUI m_DistanceLabel;

	[SerializeField]
	private TextMeshProUGUI m_PenetrationLabel;

	[SerializeField]
	private GameObject m_VitalDamageObj;

	[Header("Ability Block")]
	[SerializeField]
	private CharInfoWeaponAbilityPCView m_WeaponSetAbilityViewPrefab;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[Header("Damage Template")]
	[SerializeField]
	private Color m_templateTextColor;

	[Header("Localization")]
	[SerializeField]
	private TextMeshProUGUI m_Damage;

	[SerializeField]
	private TextMeshProUGUI m_NotEquipped;

	protected override void OnBind()
	{
		m_Damage.text = UIStrings.Instance.Tooltips.Damage.Text;
		m_NotEquipped.text = UIStrings.Instance.Tooltips.NotEquipped.Text;
		EquipSlotVM equipSlotVM = ((!base.ViewModel.IsPrimary) ? base.ViewModel.WeaponSetVM?.Secondary : base.ViewModel.WeaponSetVM?.Primary);
		if (equipSlotVM?.ItemWeapon == null)
		{
			m_Button.SetActiveLayer("EmptyHand");
			return;
		}
		if (equipSlotVM.ItemWeapon.Blueprint.IsTwoHanded)
		{
			m_Button.SetActiveLayer("TwoHandedWeapon");
		}
		else
		{
			m_Button.SetActiveLayer("Normal");
		}
		BindHandSlot(m_PrimaryHandView, equipSlotVM);
		string templateText = GetTemplateText();
		m_Title.text = equipSlotVM.DisplayName.CurrentValue;
		m_Type.text = equipSlotVM.TypeName.CurrentValue;
		if (equipSlotVM.ItemEntity is ItemEntityWeapon { Blueprint: var blueprint } itemEntityWeapon)
		{
			IntermediateDamage resultDamage = GetWeaponStats(itemEntityWeapon).ResultDamage;
			m_DamageLabel.text = string.Format(templateText, resultDamage.MinValueBase, resultDamage.MaxValueBase);
			m_VitalDamageValue.text = $"+{itemEntityWeapon.DamageVital}";
			m_DistanceLabel.text = string.Format(templateText, itemEntityWeapon.AttackOptimalRange, itemEntityWeapon.AttackRange);
			m_PenetrationLabel.text = $"{blueprint.WarhammerPenetration}";
			m_AbilityShortName.text = (blueprint.IsMelee ? UIUtilityText.GetStatShortName(StatType.WeaponSkill) : UIUtilityText.GetStatShortName(StatType.BallisticSkill));
			m_VitalDamageObj.SetActive(itemEntityWeapon.DamageVital > 0);
			DrawAbilities(itemEntityWeapon);
		}
	}

	protected override void OnUnbind()
	{
		m_PrimaryHandView.Unbind();
		m_WidgetList.Clear();
	}

	private void BindHandSlot(HandSlotView slot, EquipSlotVM slotVm)
	{
		slot.Bind(slotVm);
		base.ViewModel.WeaponSetVM.SelectedHand.Subscribe(delegate(EquipSlotVM selectedVm)
		{
			if (selectedVm != null)
			{
				bool flag = selectedVm == slotVm;
				slot.Slot.SetActiveLayer(flag ? "Selected" : "Normal");
			}
		}).AddTo(this);
		bool canConfirmClick = base.ViewModel.WeaponSetVM.Primary.HasItem && base.ViewModel.WeaponSetVM.Secondary.HasItem;
		slot.SetClickAction(delegate
		{
			base.ViewModel.WeaponSetVM.SelectHand(slotVm);
		});
		slot.SetCanConfirmClick(canConfirmClick);
	}

	private void DrawAbilities(ItemEntityWeapon weapon)
	{
		List<CharInfoWeaponSetAbilityVM> list = new List<CharInfoWeaponSetAbilityVM>();
		foreach (WeaponAbility weaponAbility in weapon.Blueprint.WeaponAbilities)
		{
			CharInfoWeaponSetAbilityVM item = new CharInfoWeaponSetAbilityVM(weaponAbility.Ability, weapon, weapon.Owner).AddTo(this);
			list.Add(item);
		}
		m_WidgetList.DrawEntries(list, m_WeaponSetAbilityViewPrefab);
	}

	private RuleCalculateStatsWeapon GetWeaponStats(ItemEntityWeapon weapon)
	{
		RuleCalculateStatsWeapon weaponStats = weapon.GetWeaponStats();
		BaseUnitEntity currentSelectedUnit = UtilityParty.GetCurrentSelectedUnit();
		if (currentSelectedUnit == null)
		{
			return weaponStats;
		}
		return weapon.GetWeaponStats(currentSelectedUnit);
	}

	private string GetTemplateText()
	{
		return "<b>{0}<voffset=0.1em><size=74%>|</size></voffset></b><size=50%><color=#" + ColorUtility.ToHtmlStringRGBA(m_templateTextColor) + "> max</size> <size=66%><b>{1}</b></size></color>";
	}
}
