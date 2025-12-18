using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickTagDescription : ITooltipBrick
{
	private readonly Sprite m_Icon;

	private readonly Color m_BgrColor;

	private readonly string m_TagName;

	private readonly string m_TagDescription;

	public TooltipBrickTagDescription(Sprite icon, Color bgrColor, string tagName, string tagDescription)
	{
		m_Icon = icon;
		m_BgrColor = bgrColor;
		m_TagName = tagName;
		m_TagDescription = tagDescription;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTagDescriptionVM(m_Icon, m_BgrColor, m_TagName, m_TagDescription);
	}
}
