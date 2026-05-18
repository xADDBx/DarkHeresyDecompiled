using System;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[Serializable]
public struct QuickInspectUISettings
{
	[SerializeField]
	private bool m_CustomNameColor;

	[ShowIf("m_CustomNameColor")]
	[SerializeField]
	private Color m_NameColor;

	[SerializeField]
	private bool m_CustomDescriptionColor;

	[ShowIf("m_CustomDescriptionColor")]
	[SerializeField]
	private Color m_DescriptionColor;

	[SerializeField]
	private bool m_CustomDescriptionIcon;

	[ShowIf("m_CustomDescriptionIcon")]
	[SerializeField]
	private Sprite m_DescriptionIcon;

	[SerializeField]
	private bool m_CustomPriority;

	[ShowIf("m_CustomPriority")]
	[SerializeField]
	private int m_Priority;

	public bool ForceShow;

	public Color? GetNameColor()
	{
		if (!m_CustomNameColor)
		{
			return null;
		}
		return m_NameColor;
	}

	public Color? GetDescriptionColor()
	{
		if (!m_CustomDescriptionColor)
		{
			return null;
		}
		return m_DescriptionColor;
	}

	public Sprite GetDescriptionIcon()
	{
		if (!m_CustomDescriptionIcon)
		{
			return null;
		}
		return m_DescriptionIcon;
	}

	public int GetPriority()
	{
		if (m_CustomPriority)
		{
			return m_Priority;
		}
		return int.MinValue;
	}
}
