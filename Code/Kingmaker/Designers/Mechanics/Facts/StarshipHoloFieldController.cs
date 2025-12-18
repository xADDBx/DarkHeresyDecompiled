using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Visual.FX;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fb8a35db69c8bee4eb229e8802ffbeff")]
public class StarshipHoloFieldController : BlueprintComponent
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public GameObject[] fxObjects;

		public int buffRank;

		public bool wasAttackedLastRound;
	}

	[SerializeField]
	private BlueprintBuffReference m_HoloFieldBuff;

	[SerializeField]
	private int restoreChargesPerTurn;

	[SerializeField]
	private bool dontRestoreChargesIfAttackedLastTurn;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	public BlueprintBuff HoloFieldBuff => m_HoloFieldBuff?.Get();

	private BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	private VisualFXSettings[] PrefabList => FXSettings.VisualFXSettings.MechanicalEvents[0].Settings.FXs;

	private int MaxFXRank => PrefabList.Length / 2 - 1 - 1;
}
