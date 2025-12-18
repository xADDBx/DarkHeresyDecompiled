using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public abstract class BaseEntityInfoWidget : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_Root;

	[SerializeField]
	private Vector2 m_PositionOffset;

	private GameObject m_RootGameObject;

	private RectTransform m_ParentRectTransform;

	private GameObject RootGameObject
	{
		get
		{
			if (!(m_RootGameObject != null))
			{
				return m_RootGameObject = m_Root.gameObject;
			}
			return m_RootGameObject;
		}
	}

	private RectTransform ParentRectTransform
	{
		get
		{
			if (!(m_ParentRectTransform != null))
			{
				return m_ParentRectTransform = m_Root.parent.transform as RectTransform;
			}
			return m_ParentRectTransform;
		}
	}

	public abstract bool TryShow(IEntityInfo entityInfo);

	public void Hide()
	{
		ShowInternal(show: false);
	}

	protected void ShowInternal(bool show)
	{
		RootGameObject.SetActive(show);
	}

	protected void SetPosition(Vector3 entityWorldPosition)
	{
		Vector2 anchoredPosition = WorldToAnchoredPosition(entityWorldPosition);
		m_Root.anchoredPosition = anchoredPosition;
	}

	private Vector2 WorldToAnchoredPosition(Vector3 worldPosition)
	{
		Vector3 normalizedPositionInCamera = UIUtilityRect.GetNormalizedPositionInCamera(worldPosition);
		Rect rect = ParentRectTransform.rect;
		return UIUtilityRect.NormalizedToPixelPosition(rect, normalizedPositionInCamera) - rect.size * ParentRectTransform.pivot + (m_Root.rect.size * m_Root.pivot + m_PositionOffset);
	}
}
public abstract class BaseEntityInfoWidget<TInfoElement> : BaseEntityInfoWidget where TInfoElement : MonoBehaviour
{
	[SerializeField]
	private Transform m_ElementsRoot;

	private readonly List<TInfoElement> m_PooledElements = new List<TInfoElement>();

	protected TInfoElement GetElementView(int index, TInfoElement elementPrefab)
	{
		if (m_PooledElements.Count > index)
		{
			return m_PooledElements[index];
		}
		TInfoElement widget = WidgetFactory.GetWidget(elementPrefab, activate: false, strictMatching: true);
		widget.transform.SetParent(m_ElementsRoot, worldPositionStays: false);
		m_PooledElements.Add(widget);
		return widget;
	}

	protected void HideAllElements()
	{
		foreach (TInfoElement pooledElement in m_PooledElements)
		{
			pooledElement.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		foreach (TInfoElement pooledElement in m_PooledElements)
		{
			WidgetFactory.DisposeWidget(pooledElement);
		}
		m_PooledElements.Clear();
	}
}
