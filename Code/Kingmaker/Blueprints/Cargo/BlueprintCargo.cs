using System;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Cargo;

[Obsolete]
[TypeId("33863faf406f5324f855756f396ea49e")]
public class BlueprintCargo : BlueprintMechanicEntityFact
{
	[SerializeField]
	private ItemsItemOrigin m_OriginType;

	[SerializeField]
	private bool m_CanRemoveItems = true;

	[SerializeField]
	private bool m_Integral;

	[SerializeField]
	private bool m_OverrideReputationCost;

	[ShowIf("OverrideReputationCost")]
	[SerializeField]
	private int m_OverridenReputationCost;

	public ItemsItemOrigin OriginType => m_OriginType;

	public bool OverrideReputationCost => m_OverrideReputationCost;

	public int? OverridenReputationCost
	{
		get
		{
			if (!m_OverrideReputationCost)
			{
				return null;
			}
			return m_OverridenReputationCost;
		}
	}
}
