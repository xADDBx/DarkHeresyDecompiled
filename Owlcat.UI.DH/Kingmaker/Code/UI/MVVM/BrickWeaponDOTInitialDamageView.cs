using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWeaponDOTInitialDamageView : BrickBaseView<BrickWeaponDOTInitialDamageVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private TMP_Text m_DamageTitle;

	[SerializeField]
	private TMP_Text m_DamageValue;

	[SerializeField]
	protected Image m_Icon;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title, m_Description, m_DamageTitle);
		}
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Name;
		m_DamageValue.text = base.ViewModel.Damage;
		m_DamageTitle.text = UIStrings.Instance.Tooltips.Damage.Text;
		m_Description.text = UIStrings.Instance.Tooltips.InitialDamage.Text;
	}
}
