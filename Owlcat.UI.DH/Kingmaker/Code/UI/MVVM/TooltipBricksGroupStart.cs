using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBricksGroupStart : ITooltipBrick
{
	private readonly TooltipBricksGroupLayoutParams m_LayoutParams;

	private readonly bool m_HasBackground;

	private readonly Color? m_BackgroundColor;

	private readonly string m_Title;

	public TooltipBricksGroupStart(bool hasBackground = true, TooltipBricksGroupLayoutParams layoutParams = null, Color? backgroundColor = null, string title = null)
	{
		m_HasBackground = hasBackground;
		m_LayoutParams = layoutParams;
		m_BackgroundColor = backgroundColor;
		m_Title = title;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBricksGroupVM(TooltipBricksGroupType.Start, m_HasBackground, m_LayoutParams, m_BackgroundColor, m_Title);
	}
}
