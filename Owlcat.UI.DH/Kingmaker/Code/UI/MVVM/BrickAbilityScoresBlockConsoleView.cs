using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityScoresBlockConsoleView : BrickAbilityScoresBlockView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_TitleFrame;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_TitleFrame;
	}
}
