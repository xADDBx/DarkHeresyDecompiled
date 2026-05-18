using System;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.Code.UI.MVVM;
using R3;
using UnityEngine;

namespace Kingmaker.UI.Common.PageNavigation;

public class Paginator : MonoBehaviour, IDisposable
{
	[SerializeField]
	private RectTransform m_ViewPort;

	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private PageNavigationPC m_PageNavigation;

	private float m_ViewPortHeight;

	private IDisposable m_IndexSubscription;

	private readonly ReactiveProperty<int> m_PageNumber = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_PageIndex = new ReactiveProperty<int>();

	private readonly ReactiveCommand<Unit> m_UpdateViewTrigger = new ReactiveCommand<Unit>();

	public Observable<Unit> UpdateViewTrigger => m_UpdateViewTrigger;

	public void Dispose()
	{
		m_PageNavigation.Dispose();
		m_IndexSubscription?.Dispose();
	}

	public void UpdateView()
	{
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			m_ViewPortHeight = (m_ViewPort.gameObject.activeSelf ? m_ViewPort.rect.height : 0f);
			m_PageNumber.Value = ((m_ViewPortHeight > 0f) ? Mathf.CeilToInt(m_Content.rect.height / m_ViewPortHeight) : 0);
			m_PageIndex.Value = 0;
			m_PageNavigation.Initialize(m_PageNumber.Value, m_PageIndex, delegate(int idx)
			{
				m_PageNumber.Value = idx;
			});
			m_UpdateViewTrigger.Execute();
			OnIndexChanged(m_PageIndex.Value);
		});
	}

	public void SetPageIndex(int index)
	{
		m_PageIndex.Value = index;
		OnIndexChanged(index);
	}

	private void OnIndexChanged(int index)
	{
		m_Content.anchoredPosition = new Vector2(0f, (float)index * m_ViewPortHeight);
	}
}
