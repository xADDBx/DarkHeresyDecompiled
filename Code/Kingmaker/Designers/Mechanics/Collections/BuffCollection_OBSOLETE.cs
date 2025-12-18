using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Collections;

[Obsolete]
[TypeId("f524a3e2559c8994cad6056295fc2343")]
public class BuffCollection_OBSOLETE : BlueprintScriptableObject, IBlueprintScanner
{
	public bool CheckHidden;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("BuffList")]
	private BlueprintBuffReference[] m_BuffList;

	public ReferenceArrayProxy<BlueprintBuff> BuffList
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffList = m_BuffList;
			return buffList;
		}
	}

	public void Scan()
	{
	}
}
