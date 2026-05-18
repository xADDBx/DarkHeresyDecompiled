using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpSkillcheckBonusView : BrickBaseView<BrickLevelUpSkillcheckBonusVM>
{
	[SerializeField]
	private TMP_Text m_ParamBonusLabel;

	[SerializeField]
	private TMP_Text m_Value;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_ParamBonusLabel).AddTo(this);
		}
		base.OnBind();
		m_Value.text = base.ViewModel.BonusValue;
		m_ParamBonusLabel.text = UIStrings.Instance.Tooltips.BonusValue;
		m_TextHelper.UpdateTextSize();
	}
}
