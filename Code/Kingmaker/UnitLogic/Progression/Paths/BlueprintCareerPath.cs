using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("f70bef04068c49c58baf98907530f89d")]
public class BlueprintCareerPath : BlueprintPath
{
	[Serializable]
	[OwlPackable(OwlPackableMode.NoGenerate)]
	public new class Reference : BlueprintReference<BlueprintCareerPath>
	{
	}

	public CareerPathTier Tier;

	[Header("State UI Icons")]
	public Sprite InProgressIcon;

	public Sprite FinishedIcon;

	public Sprite NotAvailableIcon;

	public bool IsHunter;

	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	public bool IsAvailable
	{
		get
		{
			if (m_DlcReward != null && !m_DlcReward.IsEmpty())
			{
				return m_DlcReward.Get().IsActive;
			}
			return true;
		}
	}
}
