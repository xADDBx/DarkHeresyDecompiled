using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityWeaponDamageView : BrickBaseView<BrickAbilityWeaponDamageVM>
{
	private const char EnDash = '–';

	[SerializeField]
	private TMP_Text m_WeaponFamilyText;

	[SerializeField]
	private TMP_Text m_DamageText;

	[SerializeField]
	private TMP_Text m_DamageValueText;

	[SerializeField]
	private Image m_WeaponIcon;

	[SerializeField]
	private WidgetList m_TagsWidgetList;

	[SerializeField]
	private SpecialWeaponTagWidget m_TagPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_WeaponFamilyText, m_DamageText);
		m_WeaponFamilyText.SetText(base.ViewModel.WeaponFamily);
		m_WeaponIcon.sprite = base.ViewModel.WeaponIcon;
		m_DamageText.SetText(base.ViewModel.DamageDescriptionText);
		m_DamageValueText.SetText($"{base.ViewModel.BaseDamageMin}{'–'}{base.ViewModel.BaseDamageMax}");
		m_TagsWidgetList.DrawEntries(base.ViewModel.WeaponTags, m_TagPrefab).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}
}
