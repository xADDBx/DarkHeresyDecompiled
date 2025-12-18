using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSpace : ITooltipBrick
{
	private readonly float? m_Height;

	public TooltipBrickSpace()
	{
	}

	public TooltipBrickSpace(float height)
	{
		m_Height = height;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSpaceVM(m_Height);
	}
}
