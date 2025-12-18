using System;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("7421aebdb7c790c4bad0366a79eb2db1")]
public class WarhammerBonusDamageToStarship : BlueprintComponent
{
	private enum SidePolicy
	{
		ChoosenSide,
		RandomSide,
		RandomExceptOppisite
	}

	private enum ApplyModificationTo
	{
		AllIncoming,
		Hull
	}

	[SerializeField]
	private SidePolicy m_sidePolicy;

	[SerializeField]
	[ShowIf("ChooseSide")]
	private StarshipHitLocation m_StarshipSide;

	[SerializeField]
	private ApplyModificationTo applyModificationTo;

	[SerializeField]
	private int m_BonusDamage;

	[SerializeField]
	private float m_ExtraDamageMod;

	[SerializeField]
	private GameObject m_BonusDamageMarker;

	private bool ChooseSide => m_sidePolicy == SidePolicy.ChoosenSide;
}
