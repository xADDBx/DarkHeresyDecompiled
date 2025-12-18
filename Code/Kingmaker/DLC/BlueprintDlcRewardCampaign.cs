using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("59799e878ee1411a8784f9791fe060f3")]
public class BlueprintDlcRewardCampaign : BlueprintDlcReward
{
	public Texture2D ScreenshotForImportSave;

	[SerializeField]
	public BlueprintCampaignReference m_Campaign;

	public BlueprintCampaign Campaign => m_Campaign;

	public override void RecheckAvailability()
	{
		base.RecheckAvailability();
		Campaign.RecheckAvailability();
	}
}
