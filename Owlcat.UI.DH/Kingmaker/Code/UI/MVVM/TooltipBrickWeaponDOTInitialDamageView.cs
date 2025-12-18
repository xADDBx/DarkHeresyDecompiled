using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWeaponDOTInitialDamageView : TooltipBaseBrickView<TooltipBrickWeaponDOTInitialDamageVM>, IWidgetView
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private TextMeshProUGUI m_DamageTitle;

	[SerializeField]
	private TextMeshProUGUI m_DamageValue;

	protected override void OnBind()
	{
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Name;
		m_DamageValue.text = base.ViewModel.Damage;
		m_DamageTitle.text = UIStrings.Instance.Tooltips.Damage.Text;
		m_Description.text = UIStrings.Instance.Tooltips.InitialDamage.Text;
	}
}
