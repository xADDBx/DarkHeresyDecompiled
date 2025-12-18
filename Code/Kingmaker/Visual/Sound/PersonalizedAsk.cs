using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[Serializable]
public class PersonalizedAsk : AsksSet
{
	[SerializeField]
	private BlueprintUnitReference[] m_UnitReferences = Array.Empty<BlueprintUnitReference>();

	public BlueprintUnitReference[] UnitReferences
	{
		get
		{
			return m_UnitReferences;
		}
		set
		{
			m_UnitReferences = value;
		}
	}
}
