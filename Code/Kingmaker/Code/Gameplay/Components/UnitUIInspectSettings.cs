using System;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
public class UnitUIInspectSettings
{
	[SerializeField]
	private UnitInspectUIFlags m_Flags;

	public Vector3 UnitInfoOffset;

	public bool HasFlags(UnitInspectUIFlags flags)
	{
		return m_Flags.HasFlag(flags);
	}
}
