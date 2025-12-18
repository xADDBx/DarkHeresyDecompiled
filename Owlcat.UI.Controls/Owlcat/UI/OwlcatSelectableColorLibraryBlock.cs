using System;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct OwlcatSelectableColorLibraryBlock : IEquatable<OwlcatSelectableColorLibraryBlock>
{
	[Serializable]
	public struct Item : IEquatable<Item>
	{
		[SerializeField]
		private Color PalleteStub;

		[SerializeField]
		[ColorPalleteItemPicker]
		private ColorPalleteItem PalleteItem;

		public readonly Color Color
		{
			get
			{
				if (!(PalleteItem == null))
				{
					return PalleteItem.Color;
				}
				return PalleteStub;
			}
		}

		public Item(Color color, ColorPalleteItem item)
		{
			PalleteStub = color;
			PalleteItem = item;
		}

		public readonly bool Equals(Item other)
		{
			if (PalleteStub == other.PalleteStub)
			{
				return PalleteItem == other.PalleteItem;
			}
			return false;
		}
	}

	[SerializeField]
	private Item m_Normal;

	[SerializeField]
	private Item m_Highlighted;

	[SerializeField]
	private Item m_Pressed;

	[SerializeField]
	private Item m_Focused;

	[SerializeField]
	private Item m_Disabled;

	[SerializeField]
	private float m_FadeDuration;

	public Item Normal
	{
		get
		{
			return m_Normal;
		}
		set
		{
			m_Normal = value;
		}
	}

	public Item Highlighted
	{
		get
		{
			return m_Highlighted;
		}
		set
		{
			m_Highlighted = value;
		}
	}

	public Item Pressed
	{
		get
		{
			return m_Pressed;
		}
		set
		{
			m_Pressed = value;
		}
	}

	public Item Focused
	{
		get
		{
			return m_Focused;
		}
		set
		{
			m_Focused = value;
		}
	}

	public Item Disabled
	{
		get
		{
			return m_Disabled;
		}
		set
		{
			m_Disabled = value;
		}
	}

	public float FadeDuration
	{
		get
		{
			return m_FadeDuration;
		}
		set
		{
			m_FadeDuration = value;
		}
	}

	public static OwlcatSelectableColorLibraryBlock DefaultActiveBlock
	{
		get
		{
			OwlcatSelectableColorLibraryBlock result = default(OwlcatSelectableColorLibraryBlock);
			result.FadeDuration = 0.1f;
			result.m_Normal = new Item(Color.white, null);
			result.m_Highlighted = new Item(new Color(49f / 51f, 49f / 51f, 49f / 51f, 1f), null);
			result.m_Pressed = new Item(new Color(0.7843137f, 0.7843137f, 0.7843137f, 1f), null);
			result.m_Focused = new Item(new Color(49f / 51f, 49f / 51f, 49f / 51f, 1f), null);
			result.m_Disabled = new Item(new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f), null);
			return result;
		}
	}

	public bool Equals(OwlcatSelectableColorLibraryBlock other)
	{
		if (Normal.Equals(other.Normal) && Highlighted.Equals(other.Highlighted) && Pressed.Equals(other.Pressed) && Focused.Equals(other.Focused) && Disabled.Equals(other.Disabled))
		{
			return Mathf.Approximately(m_FadeDuration, other.FadeDuration);
		}
		return false;
	}
}
