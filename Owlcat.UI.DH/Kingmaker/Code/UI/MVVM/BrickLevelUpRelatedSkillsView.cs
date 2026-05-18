using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpRelatedSkillsView : BrickBaseView<BrickLevelUpRelatedSkillsVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_RelatedSkillLabel;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private TooltipElementRelatedSkillView m_RelatedSkillPrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RelatedSkillLabel).AddTo(this);
		}
		base.OnBind();
		m_RelatedSkillLabel.text = UIStrings.Instance.Tooltips.RelatedSkills;
		m_WidgetList.DrawEntries(base.ViewModel.RelatedSkills, m_RelatedSkillPrefab).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
