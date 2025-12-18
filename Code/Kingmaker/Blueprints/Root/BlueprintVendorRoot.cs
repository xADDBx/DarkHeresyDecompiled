using System;
using System.Collections.Generic;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("e86225224a394ea8a20cfd197baaf46a")]
public class BlueprintVendorRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintVendorRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private List<BpRef<BlueprintVendorFaction>> m_VendorFactions;

	[Range(0f, 100f)]
	[Tooltip("За сколько % от указанной в блюпринте стоимости игрок продает предметы вендору")]
	public int SellToVendorCostFactor = 50;

	public IReadOnlyList<BpRef<BlueprintVendorFaction>> VendorFactions => m_VendorFactions;
}
