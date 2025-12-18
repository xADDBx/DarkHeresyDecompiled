using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.UI;

[AddComponentMenu("Layout/Auto Layout Group", 150)]
public class AutoLayoutGroup : HorizontalOrVerticalLayoutGroup
{
	private enum LayoutType
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	private LayoutType m_Layout;

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxis(0, m_Layout == LayoutType.Vertical);
	}

	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxis(1, m_Layout == LayoutType.Vertical);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxis(0, m_Layout == LayoutType.Vertical);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1, m_Layout == LayoutType.Vertical);
	}
}
