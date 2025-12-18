using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Owlcat.UI;

internal class DisplayOrderTracker : UIBehaviour
{
	private bool m_HasChanges;

	private int m_LastSiblingIndex;

	private Canvas m_LastCanvas;

	private int m_LastCanvasSortingOrder;

	private event Action Changed;

	public static void Attach(Transform target, Action handler)
	{
		if (!target.TryGetComponent<DisplayOrderTracker>(out var component))
		{
			component = target.gameObject.AddComponent<DisplayOrderTracker>();
		}
		component.Changed += handler;
	}

	public static void Detach(Transform target, Action handler)
	{
		if (target.TryGetComponent<DisplayOrderTracker>(out var component))
		{
			component.Changed -= handler;
		}
	}

	protected override void Awake()
	{
		base.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
		Validate();
	}

	protected override void OnTransformParentChanged()
	{
		m_HasChanges = true;
	}

	protected override void OnCanvasHierarchyChanged()
	{
		m_LastCanvas = DisplayOrderComparer.GetCanvas(base.transform);
		m_LastCanvasSortingOrder = ((m_LastCanvas != null) ? m_LastCanvas.sortingOrder : 0);
		m_HasChanges = true;
	}

	private void Update()
	{
		if (IsActive() && this.Changed != null)
		{
			Validate();
		}
	}

	private void Validate()
	{
		m_HasChanges |= SetProperty(ref m_LastSiblingIndex, base.transform.GetSiblingIndex());
		m_HasChanges |= SetProperty(ref m_LastCanvasSortingOrder, (m_LastCanvas != null) ? m_LastCanvas.sortingOrder : 0);
		if (m_HasChanges)
		{
			m_HasChanges = false;
			this.Changed?.Invoke();
		}
	}

	private bool SetProperty<T>(ref T field, T value)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		return true;
	}
}
