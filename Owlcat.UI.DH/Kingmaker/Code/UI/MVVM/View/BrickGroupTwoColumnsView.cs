using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickGroupTwoColumnsView : BrickGroupView<BricksGroupTwoColumnsVM>
{
	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private Transform m_LeftColumn;

	[SerializeField]
	private Transform m_RightColumn;

	private Color m_InitBgrColor;

	private int m_ColumnIndex;

	protected override void OnBind()
	{
		m_ColumnIndex = 0;
		m_InitBgrColor = m_BackgroundImage.color;
		m_Background.SetActive(base.ViewModel.HasBackground);
		if (base.ViewModel.BackgroundColor.HasValue && (bool)m_BackgroundImage)
		{
			m_BackgroundImage.color = base.ViewModel.BackgroundColor.Value;
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_BackgroundImage.color = m_InitBgrColor;
	}

	public override void AddChild(RectTransform childTransform)
	{
		Transform activeTransform = GetActiveTransform();
		childTransform.SetParent(activeTransform, worldPositionStays: false);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	private Transform GetActiveTransform()
	{
		if (m_LeftColumn == null || m_RightColumn == null)
		{
			return base.transform;
		}
		Transform result = ((m_ColumnIndex % 2 == 0) ? m_LeftColumn : m_RightColumn);
		m_ColumnIndex++;
		return result;
	}
}
