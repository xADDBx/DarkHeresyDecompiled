using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuffDOTView : TooltipBaseBrickView<TooltipBrickBuffDOTVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private TextMeshProUGUI m_DamageText;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.text = UIStrings.Instance.Tooltips.Damage.Text;
		m_Description.text = UIStrings.Instance.Tooltips.EveryRound.Text;
		m_DamageText.text = base.ViewModel.Damage;
	}
}
