using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScoresView : TooltipBaseBrickView<TooltipBrickAbilityScoresVM>
{
	[SerializeField]
	protected CharInfoAbilityScoresBlockBaseView m_AbilityScoresBlockView;

	protected override void OnBind()
	{
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
	}
}
