using System;
using System.Collections.Generic;
using Kingmaker.Networking;
using Kingmaker.UI.Common.PageNavigation;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.PageNavigation;

public abstract class PageNavigationBase : MonoBehaviour, IDisposable
{
	[SerializeField]
	private bool m_HasPoints = true;

	[SerializeField]
	[ShowIf("m_HasPoints")]
	private PageNavigationPoint m_PointPrefab;

	[SerializeField]
	[ShowIf("m_HasPoints")]
	private Transform m_PointsContainer;

	private Action m_PrevCallback;

	private Action m_NextCallback;

	private readonly List<PageNavigationPoint> m_Points = new List<PageNavigationPoint>();

	private int m_PageCount;

	private ReadOnlyReactiveProperty<int> m_CurrentIndex;

	private Action<int> m_SetPageIndex;

	protected readonly List<IDisposable> Disposables = new List<IDisposable>();

	protected bool HasPrevious
	{
		get
		{
			ReadOnlyReactiveProperty<int> currentIndex = m_CurrentIndex;
			if (currentIndex == null)
			{
				return false;
			}
			return currentIndex.CurrentValue > 0;
		}
	}

	protected bool HasNext => m_CurrentIndex?.CurrentValue < m_PageCount - 1;

	public virtual void Initialize(int pageCount, ReadOnlyReactiveProperty<int> pageIndex, Action<int> setPageIndex, Action prevCallback = null, Action nextCallback = null)
	{
		m_SetPageIndex = setPageIndex;
		if (pageCount <= 1 || pageIndex == null)
		{
			OnCurrentIndexChanged(0);
			Hide();
			return;
		}
		Show(pageCount, pageIndex);
		m_PrevCallback = prevCallback;
		m_NextCallback = nextCallback;
		Disposables.Add(m_CurrentIndex.Subscribe(OnCurrentIndexChanged));
	}

	public void Show(int pageCount, ReadOnlyReactiveProperty<int> pageIndexProperty = null, int pageIndexInt = -1)
	{
		Clear();
		m_PageCount = pageCount;
		m_CurrentIndex = pageIndexProperty ?? new ReactiveProperty<int>(pageIndexInt);
		FillPoints(pageCount);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		Clear();
		base.gameObject.SetActive(value: false);
	}

	private void FillPoints(int count)
	{
		if (!m_HasPoints)
		{
			return;
		}
		for (int j = 0; j < count; j++)
		{
			PageNavigationPoint widget = WidgetFactory.GetWidget(m_PointPrefab);
			widget.transform.SetParent(m_PointsContainer, worldPositionStays: false);
			m_Points.Add(widget);
			int i1 = j;
			widget.Initialize(delegate
			{
				SetCurrentIndex(i1);
			});
		}
		UpdatePointsState();
	}

	private void ClearPoints()
	{
		if (m_HasPoints)
		{
			m_Points.ForEach(delegate(PageNavigationPoint widget)
			{
				widget.Dispose();
				WidgetFactory.DisposeWidget(widget);
			});
			m_Points.Clear();
		}
	}

	protected virtual void OnCurrentIndexChanged(int index)
	{
		UpdatePointsState();
	}

	public void OnPreviousClick()
	{
		if (HasPrevious && !PhotonManager.Lobby.PingPressed)
		{
			SetCurrentIndex(m_CurrentIndex.CurrentValue - 1);
			m_PrevCallback?.Invoke();
		}
	}

	public void OnNextClick()
	{
		if (HasNext && !PhotonManager.Lobby.PingPressed)
		{
			SetCurrentIndex(m_CurrentIndex.CurrentValue + 1);
			m_NextCallback?.Invoke();
		}
	}

	private void SetCurrentIndex(int index)
	{
		index %= m_PageCount;
		m_SetPageIndex?.Invoke(index);
	}

	private void UpdatePointsState()
	{
		if (m_HasPoints)
		{
			for (int i = 0; i < m_Points.Count; i++)
			{
				m_Points[i].SetSelected(i == m_CurrentIndex.CurrentValue);
			}
		}
	}

	private void Clear()
	{
		ClearPoints();
		Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		Disposables.Clear();
		m_CurrentIndex = null;
	}

	public virtual void Dispose()
	{
		Clear();
	}
}
