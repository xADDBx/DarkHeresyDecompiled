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

	public const int DevelopmentVectorMaxValue = 4;

	public CareerPathTier Tier;

	[Header("State UI Icons")]
	public Sprite InProgressIcon;

	public Sprite FinishedIcon;

	public Sprite NotAvailableIcon;

	public bool IsHunter;

	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	[Header("Development Vector")]
	[SerializeField]
	[Range(0f, 4f)]
	private int m_DevelopmentVectorMovement;

	[SerializeField]
	[Range(0f, 4f)]
	private int m_DevelopmentVectorPsykana;

	[SerializeField]
	[Range(0f, 4f)]
	private int m_DevelopmentVectorRange;

	[SerializeField]
	[Range(0f, 4f)]
	private int m_DevelopmentVectorBuff;

	[SerializeField]
	[Range(0f, 4f)]
	private int m_DevelopmentVectorDefence;

	[SerializeField]
	[Range(0f, 4f)]
	private int m_DevelopmentVectorMelee;

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

	public int GetDevelopmentVectorValue(CareerPathDevelopmentVector type)
	{
		return type switch
		{
			CareerPathDevelopmentVector.Movement => m_DevelopmentVectorMovement, 
			CareerPathDevelopmentVector.Psykana => m_DevelopmentVectorPsykana, 
			CareerPathDevelopmentVector.Range => m_DevelopmentVectorRange, 
			CareerPathDevelopmentVector.Defence => m_DevelopmentVectorDefence, 
			CareerPathDevelopmentVector.Melee => m_DevelopmentVectorMelee, 
			CareerPathDevelopmentVector.Buff => m_DevelopmentVectorBuff, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
