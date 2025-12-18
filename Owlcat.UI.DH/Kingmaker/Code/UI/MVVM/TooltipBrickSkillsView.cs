using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSkillsView : TooltipBaseBrickView<TooltipBrickSkillsVM>
{
	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_AbilityScoresBlockView;

	protected override void OnBind()
	{
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
	}
}
