using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpAbilityUpgradeDescriptionView : TooltipBaseBrickView<TooltipBrickLevelUpAbilityUpgradeDescriptionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_BeforeLabel;

	[SerializeField]
	private TextMeshProUGUI m_AfterLabel;

	[SerializeField]
	private TextMeshProUGUI m_BaseAbilityDescription;

	[SerializeField]
	private TextMeshProUGUI m_UpgradeAbilityDescription;

	protected override void OnBind()
	{
		base.OnBind();
		m_BeforeLabel.text = UIStrings.Instance.Tooltips.Before;
		m_AfterLabel.text = UIStrings.Instance.Tooltips.After;
		m_BaseAbilityDescription.text = base.ViewModel.BaseAbilityDescription;
		m_UpgradeAbilityDescription.text = base.ViewModel.UpgradeAbilityDescription;
		m_BaseAbilityDescription.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		m_UpgradeAbilityDescription.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
	}
}
