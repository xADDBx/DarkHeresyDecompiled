using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWeaponSetView : BrickBaseView<BrickWeaponSetVM>
{
	[Header("Elements")]
	[SerializeField]
	protected HandSlotView m_HandSlotView;

	[SerializeField]
	private GameObject m_RateOfFireBlock;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[Header("Labels")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_DamageLabel;

	[SerializeField]
	private TMP_Text m_DistanceLabel;

	[SerializeField]
	private TMP_Text m_RateOfFireLabel;

	[Header("Values")]
	[SerializeField]
	private Color m_TemplateTextColor;

	[Header("Views")]
	[SerializeField]
	private CharInfoWeaponSetAbilityPCView m_WeaponSetAbilityViewPrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title);
		}
		m_HandSlotView.Bind(base.ViewModel.EquipSlot);
		_ = base.ViewModel.Weapon.Blueprint;
		m_Title.text = base.ViewModel.Weapon.Name;
		string format = UIConfig.Instance.TextFormats.WeaponSetTextFormat.Replace("{color}", ColorUtility.ToHtmlStringRGBA(m_TemplateTextColor));
		IntermediateDamage resultDamage = base.ViewModel.Weapon.GetWeaponStats().ResultDamage;
		m_DamageLabel.text = string.Format(format, resultDamage.MinValueBase.ToString(), resultDamage.MaxValueBase.ToString());
		m_DistanceLabel.text = string.Format(format, base.ViewModel.Weapon.AttackOptimalRange.ToString(), base.ViewModel.Weapon.AttackRange.ToString());
		m_RateOfFireLabel.text = base.ViewModel.Weapon.RateOfFire.ToString();
		m_RateOfFireBlock.SetActive(base.ViewModel.Weapon.RateOfFire > 0);
		m_WidgetList.DrawEntries(base.ViewModel.Abilities.ToArray(), m_WeaponSetAbilityViewPrefab)?.AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
