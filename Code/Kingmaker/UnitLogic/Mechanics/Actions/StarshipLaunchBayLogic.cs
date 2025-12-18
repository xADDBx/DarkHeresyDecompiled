using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("ebc58ddfe9f3df7468c3c2c3e38d18b8")]
public class StarshipLaunchBayLogic : BlueprintComponent
{
	public enum BayLocation
	{
		port,
		starboard
	}

	[Serializable]
	public class SingleBayInfo
	{
		[SerializeField]
		private BlueprintStarship.Reference m_WingBlueprint;

		public BayLocation bayLocation;

		public float launchTime;

		public int launchCD;

		public int boardOffset;

		public BlueprintStarship WingBlueprint => m_WingBlueprint?.Get();
	}

	public int wingsPerTurn = 1;

	public int wingsTotal;

	public int wingsReloadRoundsAfterExpiration;

	public SingleBayInfo[] baysInfo;
}
