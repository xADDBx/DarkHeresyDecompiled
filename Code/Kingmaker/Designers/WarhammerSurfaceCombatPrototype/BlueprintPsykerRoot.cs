using System;
using Kingmaker.Blueprints;
using Kingmaker.Framework.GlobalEffectSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Visual.Sound;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

[TypeId("d9dca1c6bd538034ba12f51cee56d95e")]
public class BlueprintPsykerRoot : BlueprintScriptableObject
{
	[Serializable]
	public class PhenomenaData
	{
		public Bark Bark;

		public BpRef<BlueprintAbility> Ability;

		public PrefabLink OptionalMinorFX;
	}

	public int MaxVeilDamage = 20;

	public int PhenomenaBaseChance;

	public int PhenomenaChancePerVeilDamage = 5;

	public int PerilsBaseChance = 5;

	public int PerilsChancePerConsecutivePhenomena = 20;

	public int DefaultVeilDamageFromPower = 2;

	public int EveryRoundVeilHealing = 3;

	public int FetterVeilHealing = 5;

	public int PushVeilDamage = 10;

	public PhenomenaData[] PsychicPhenomena = new PhenomenaData[0];

	public PhenomenaData[] PerilsOfTheWarp = new PhenomenaData[0];

	[ValidateNotNull]
	public BpRef<BlueprintGlobalEffect> VeilDamageGlobalEffect = new BpRef<BlueprintGlobalEffect>();
}
