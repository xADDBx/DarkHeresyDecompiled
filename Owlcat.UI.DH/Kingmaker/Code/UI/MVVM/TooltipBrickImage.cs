using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickImage : ITooltipBrick
{
	private readonly Sprite m_Sprite;

	private readonly Vector2Int m_Size;

	public TooltipBrickImage(Sprite sprite, Vector2Int size = default(Vector2Int))
	{
		m_Sprite = sprite;
		m_Size = size;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickImageVM(m_Sprite, m_Size);
	}
}
