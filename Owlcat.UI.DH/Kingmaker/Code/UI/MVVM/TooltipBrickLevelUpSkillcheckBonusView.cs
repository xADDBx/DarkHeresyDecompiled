using Assets.Code.View.UI.MVVM;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpSkillcheckBonusView : TooltipBaseBrickView<TooltipBrickLevelUpSkillcheckBonusVM>
{
	[SerializeField]
	private TextMeshProUGUI m_RelatedSkillLabel;

	[SerializeField]
	private TextMeshProUGUI m_ParamBonusLabel;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private TooltipElementRelatedSkillView m_RelatedSkillPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_Value.text = base.ViewModel.BonusValue;
		m_ParamBonusLabel.text = UIStrings.Instance.Tooltips.BonusValue;
		m_RelatedSkillLabel.text = UIStrings.Instance.Tooltips.RelatedSkills;
		m_WidgetList.DrawEntries(base.ViewModel.RelatedSkills, m_RelatedSkillPrefab).AddTo(this);
	}
}
