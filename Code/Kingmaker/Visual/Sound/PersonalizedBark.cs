using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[Serializable]
[Obsolete]
public class PersonalizedBark : Bark
{
	[SerializeField]
	private BlueprintUnitReference[] m_UnitReferences = Array.Empty<BlueprintUnitReference>();

	public BlueprintUnitReference[] UnitReferences => m_UnitReferences;
}
