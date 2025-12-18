using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common;

public sealed class ProgressBarSegment : BaseProgressBarSegment
{
	[SerializeField]
	private Image m_FillImage;

	[SerializeField]
	private Color m_FilledColor;

	[SerializeField]
	private Color m_EmptyColor;

	[SerializeField]
	private Color m_MaxColor;

	public override void SetFill(bool isFilled, bool isMaxValue)
	{
		m_FillImage.color = (isMaxValue ? m_MaxColor : (isFilled ? m_FilledColor : m_EmptyColor));
	}
}
