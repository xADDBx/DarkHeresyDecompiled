using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickGroupOneColumnView : BrickGroupView<BricksGroupOneColumnVM>
{
	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private Transform m_Column;

	private Color m_InitBgrColor;

	protected override void OnBind()
	{
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
		childTransform.SetParent(m_Column, worldPositionStays: false);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
