using System.Collections.Generic;
using Kingmaker.UI;
using Kingmaker.UI.Pointer;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionView : View<ChargenProgressionVM>
{
	[SerializeField]
	private ChargenProgressionHeaderView m_MainHeaderView;

	[SerializeField]
	private ChargenProgressionHeaderView m_SmallHeaderView;

	[SerializeField]
	private List<ChargenProgressionFeatureView> m_FeatureViews;

	[SerializeField]
	private RectTransform m_FeatureHover;

	[SerializeField]
	private RectTransform m_Selector;

	private Vector2Int m_PrevHoveredCell;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		base.OnBind();
		m_MainHeaderView.Bind(base.ViewModel.HeaderVM);
		m_SmallHeaderView.Bind(base.ViewModel.HeaderVM);
		base.ViewModel.IsMinimized.Subscribe(Minimize).AddTo(this);
		for (int i = 0; i < m_FeatureViews.Count; i++)
		{
			if (i < base.ViewModel.FeatureRowVMs.Count)
			{
				m_FeatureViews[i].Bind(base.ViewModel.FeatureRowVMs[i]);
			}
			else
			{
				m_FeatureViews[i].gameObject.SetActive(value: false);
			}
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!m_MainHeaderView.gameObject.activeInHierarchy)
		{
			return;
		}
		Vector2 cursorPosition = CursorController.CursorPosition;
		int mAX_LEVELS = ChargenProgressionVM.MAX_LEVELS;
		int count = base.ViewModel.FeatureRowVMs.Count;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_FeatureHover, cursorPosition, UICamera.MainUICamera, out var localPoint))
		{
			float num = localPoint.x + m_FeatureHover.pivot.x * m_FeatureHover.rect.width;
			float num2 = localPoint.y + m_FeatureHover.pivot.y * m_FeatureHover.rect.height;
			float num3 = m_FeatureHover.rect.width / (float)mAX_LEVELS;
			float num4 = m_FeatureHover.rect.height / (float)count;
			int num5 = Mathf.Clamp(Mathf.FloorToInt(num / num3), 0, mAX_LEVELS - 1);
			int num6 = Mathf.Clamp(Mathf.FloorToInt(num2 / num4), 0, count - 1);
			if (m_PrevHoveredCell.x != num5 || m_PrevHoveredCell.y != num6)
			{
				m_PrevHoveredCell.x = num5;
				m_PrevHoveredCell.y = num6;
				base.ViewModel.SetCurrentHover(num5, count - num6);
				float num7 = (float)num5 * num3 + num3 / 2f;
				float num8 = (float)num6 * num4 + num4 / 2f;
				num7 -= m_FeatureHover.pivot.x * m_FeatureHover.rect.width;
				num8 -= m_FeatureHover.pivot.y * m_FeatureHover.rect.height;
				m_Selector.position = m_FeatureHover.TransformPoint(new Vector3(num7, num8, 0f));
			}
		}
	}

	private void Minimize(bool value)
	{
		m_MainHeaderView.gameObject.SetActive(!value);
	}
}
