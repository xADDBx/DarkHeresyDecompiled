using System;
using UnityEngine;

namespace Owlcat.UI;

internal class VirtualListConsoleNavigation : IConsoleNavigationScroll
{
	private IVirtualListLayoutSettings m_LayoutSettings;

	private VirtualListScrollSettings m_ScrollSettings;

	private IInternalScrollController m_Scroll;

	private IDisposable m_FocusSubscription;

	internal VirtualListConsoleNavigation(IVirtualListLayoutSettings layoutSettings, VirtualListScrollSettings scrollSettings, IInternalScrollController scroll)
	{
		m_LayoutSettings = layoutSettings;
		m_ScrollSettings = scrollSettings;
		m_Scroll = scroll;
	}

	internal void AddElement(VirtualListElement element)
	{
	}

	internal void InsertElement(int index, VirtualListElement element)
	{
	}

	internal void RemoveElement(VirtualListElement element)
	{
	}

	internal void Clear()
	{
	}

	internal void ResetNavigation()
	{
		m_FocusSubscription?.Dispose();
		m_FocusSubscription = null;
	}

	public bool CanFocusEntity(IConsoleEntity entity)
	{
		if (entity is VirtualListElement element)
		{
			bool needScrollDown;
			return m_Scroll.ElementIsInScrollZone(element, out needScrollDown);
		}
		return false;
	}

	public void ForceScrollToEntity(IConsoleEntity entity)
	{
		m_Scroll.ForceScrollToElement(entity as VirtualListElement);
	}

	public void ScrollLeft()
	{
		m_Scroll.Scroll(0f - m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollRight()
	{
		m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollUp()
	{
		m_Scroll.Scroll(0f - m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollDown()
	{
		m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollInDirection(Vector2 direction)
	{
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed * Vector2.Dot(Vector2.right, direction));
		}
		else
		{
			m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed * Vector2.Dot(Vector2.down, direction));
		}
	}
}
