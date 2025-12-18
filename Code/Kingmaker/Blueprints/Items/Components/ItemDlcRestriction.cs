using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DLC;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("0dc814f3eeffd884f94a25193e35dc4f")]
public class ItemDlcRestriction : BlueprintComponent
{
	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	[SerializeField]
	[FormerlySerializedAs("ChangeTo")]
	private BlueprintItemReference m_ChangeTo;

	public bool HideInVendors = true;

	public BlueprintDlcReward DlcReward => m_DlcReward;

	public BlueprintItem ChangeTo => m_ChangeTo?.Get();

	public bool IsRestricted => !(DlcReward?.IsActive ?? true);
}
