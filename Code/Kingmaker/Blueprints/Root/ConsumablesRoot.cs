using System;
using Kingmaker.Blueprints.Items;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("7ae113baf1d347868b894b326cdf25f5")]
public class ConsumablesRoot : BlueprintScriptableObject
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_MeltaChargeItem;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_MultikeyItem;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_RitualSetItem;

	public BlueprintItem MeltaChargeItem => m_MeltaChargeItem.Get();

	public BlueprintItem MultikeyItem => m_MultikeyItem.Get();

	public BlueprintItem RitualSetItem => m_RitualSetItem.Get();
}
