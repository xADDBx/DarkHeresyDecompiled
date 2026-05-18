using System;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
public class UnitUIOvertipSettings
{
	[SerializeField]
	private bool m_ShowForUntargetable;

	[SerializeField]
	private UnitOvertipUIPart m_HideOvertipParts;

	public bool ShowForUntargetable => m_ShowForUntargetable;

	public bool HidePart(UnitOvertipUIPart overtipPart)
	{
		return m_HideOvertipParts.HasFlag(overtipPart);
	}
}
