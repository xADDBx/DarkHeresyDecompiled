using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpAbilityUpgradeDescriptionView : BrickBaseView<BrickLevelUpAbilityUpgradeDescriptionVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_BeforeLabel;

	[SerializeField]
	private TMP_Text m_AfterLabel;

	[SerializeField]
	private TMP_Text m_BaseAbilityDescription;

	[SerializeField]
	private TMP_Text m_UpgradeAbilityDescription;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_BeforeLabel, m_AfterLabel, m_BaseAbilityDescription, m_UpgradeAbilityDescription).AddTo(this);
		}
		base.OnBind();
		m_BeforeLabel.text = UIStrings.Instance.Tooltips.Before;
		m_AfterLabel.text = UIStrings.Instance.Tooltips.After;
		m_BaseAbilityDescription.text = base.ViewModel.BaseAbilityDescription;
		m_UpgradeAbilityDescription.text = base.ViewModel.UpgradeAbilityDescription;
		m_BaseAbilityDescription.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		m_UpgradeAbilityDescription.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
