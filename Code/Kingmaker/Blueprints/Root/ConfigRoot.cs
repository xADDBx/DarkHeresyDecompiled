using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.Gameplay.Blueprints.Root;
using Kingmaker.Console.PS5.PSNObjects;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Gameplay.Blueprints.Root;
using Kingmaker.Gameplay.Features.AoEPatterns.Blueprints;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Interaction;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Visual.HitSystem;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Blueprints.Root;

public class ConfigRoot
{
	public static readonly ConfigRoot Instance = new ConfigRoot();

	private static readonly BpRef<CheatRoot> CheatRootRef = new BpRef<CheatRoot>("3d126fd28a6c4b319e756557506134de");

	private static readonly BpRef<SORoot> SORootRef = new BpRef<SORoot>("03e1768f23a74522aa50495c5d122237");

	private static readonly BpRef<BlueprintCombatRoot> CombatRootRef = new BpRef<BlueprintCombatRoot>("3a92562d9427448cb873cae94b9611eb");

	private static readonly BpRef<BlueprintPsykerRoot> PsykerRootRef = new BpRef<BlueprintPsykerRoot>("0cabe30f540244f99f3eedd1f4e83932");

	private static readonly BpRef<UnitConditionBuffsRoot> UnitConditionBuffsRef = new BpRef<UnitConditionBuffsRoot>("abab157e71ab4d5bba491ef291dbc005");

	private static readonly BpRef<BlueprintDestructibleObjectsRoot> DestructibleObjectsRootRef = new BpRef<BlueprintDestructibleObjectsRoot>("df64b94c2c74433db75440176be96ec4");

	private static readonly BpRef<SkillCheckRoot> SkillCheckRootRef = new BpRef<SkillCheckRoot>("15a9c59c1d504d2d832691adb5d9acea");

	private static readonly BpRef<BlueprintDifficultyRoot> DifficultyRootRef = new BpRef<BlueprintDifficultyRoot>("ea0fc7492a3b459f9874c5576580efa9");

	private static readonly BpRef<CutscenesRoot> CutsceneRootRef = new BpRef<CutscenesRoot>("abe8b95bc0164356a0a47c3aee0d6b1c");

	private static readonly BpRef<BlueprintTraumaRoot> TraumaRootRef = new BpRef<BlueprintTraumaRoot>("c36f337f95724a49a2ea61f9246239e2");

	private static readonly BpRef<BlueprintDismembermentRoot> DismembermentRootRef = new BpRef<BlueprintDismembermentRoot>("120aabb9ba384fdba461c353676652d1");

	private static readonly BpRef<BlueprintAlignmentMarksRoot> AlignmentMarksRootRef = new BpRef<BlueprintAlignmentMarksRoot>("3f5c629bd2e344208b29ae81be861a89");

	private static readonly BpRef<LevelUpFxLibrary> LevelUpFxLibraryRef = new BpRef<LevelUpFxLibrary>("6a75c53cf1cb409c87a946ff3379ace5");

	private static readonly BpRef<DialogRoot> DialogRootRef = new BpRef<DialogRoot>("612767edb09b46b28bf136f0d07fc176");

	private static readonly BpRef<PreciseAttackRoot> PreciseAttackRootRef = new BpRef<PreciseAttackRoot>("35b2e845e2a2465090681f7195efd2a8");

	private static readonly BpRef<ProgressionRoot> ProgressionRootRef = new BpRef<ProgressionRoot>("5cada16f91ec485797fd7f0e2098206a");

	private static readonly BpRef<Prefabs> PrefabsRootRef = new BpRef<Prefabs>("cde653b0844f425798f65eb0fdc0143b");

	private static readonly BpRef<UIConfig> UIConfigRef = new BpRef<UIConfig>("b757732e714447ef867266c15d36db54");

	private static readonly BpRef<QuestsRoot> QuestsRootRef = new BpRef<QuestsRoot>("63def64115274bb8b65ff4b78405cf7a");

	private static readonly BpRef<SystemMechanicsRoot> SystemMechanicsRootRef = new BpRef<SystemMechanicsRoot>("e4f0a16970414f779cc990df79680d62");

	private static readonly BpRef<ConsumablesRoot> ConsumablesRootRef = new BpRef<ConsumablesRoot>("40b4e4ae734649b7943b8e2742e4d925");

	private static readonly BpRef<DlcRoot> DlcRootRef = new BpRef<DlcRoot>("77c72ce84fd44bfeb280f1aadff67910");

	private static readonly BpRef<NewGameRoot> NewGameSettingsRootRef = new BpRef<NewGameRoot>("f161f091b07f49b4bc26809a7ee514b1");

	private static readonly BpRef<CameraRoot> CameraRootRef = new BpRef<CameraRoot>("0896f77b2f2e4f97b3714240bd0363bf");

	private static readonly BpRef<LocalizedTexts> LocalizedTextsRef = new BpRef<LocalizedTexts>("8d2643b082254a859a00bd14a0aa6e3c");

	private static readonly BpRef<SoundRoot> SoundRootRef = new BpRef<SoundRoot>("6bc1744a503249208367364b901a92ac");

	private static readonly BpRef<SoundRagdollSettings> SoundRagdollSettingsRef = new BpRef<SoundRagdollSettings>("ffa86a206a2e4567af8467a7da2dd488");

	private static readonly BpRef<WarhammerDate> InitialDateRef = new BpRef<WarhammerDate>("904dc9b9bef74b3c8ea3c443b8a11fee");

	private static readonly BpRef<FormationsRoot> FormationRootRef = new BpRef<FormationsRoot>("e7e7b821199c41f3b5a6be7e8d2e24ef");

	private static readonly BpRef<FxRoot> FxRootRef = new BpRef<FxRoot>("61cc3b5e3dfd47909ceb4d7330c1b18a");

	private static readonly BpRef<BlueprintCharGenRoot> CharGenRootRef = new BpRef<BlueprintCharGenRoot>("d57c305491af4a27a35b4ef6595029bd");

	private static readonly BpRef<HitSystemRoot> HitSystemRootRef = new BpRef<HitSystemRoot>("91107a245fdb4e2294b7096b8b7a6fe9");

	private static readonly BpRef<PlayerUpgradeActionsRoot> PlayerUpgradeActionsRootRef = new BpRef<PlayerUpgradeActionsRoot>("c8263eb3b9284242bc5915a755a730ec");

	private static readonly BpRef<BlueprintAchievementsRoot> AchievementsRootRef = new BpRef<BlueprintAchievementsRoot>("4b64d9446fba4ea383a3050e538d125e");

	private static readonly BpRef<ConsoleRoot> ConsoleRootRef = new BpRef<ConsoleRoot>("06d53912d9df44c2901af7c8ea0e7df1");

	private static readonly BpRef<BlueprintTrapSettingsRoot> TrapSettingsRootRef = new BpRef<BlueprintTrapSettingsRoot>("c3e258ef78e043279f6d452c00c5cb6e");

	private static readonly BpRef<BlueprintInteractionRoot> InteractionRootRef = new BpRef<BlueprintInteractionRoot>("8224e8f316b749f1bbfbb58d025e94ba");

	private static readonly BpRef<FamiliarsRoot> FamiliarsRootRef = new BpRef<FamiliarsRoot>("d83b190e9aaf49a4b0f248563e5a9fd7");

	private static readonly BpRef<DetectiveSystemRoot> DetectiveSystemRootRef = new BpRef<DetectiveSystemRoot>("f81be79adacf4159b5c7bfb4384e9a59");

	private static readonly BpRef<BlueprintPSNObjectsRoot> PSNObjectsRef = new BpRef<BlueprintPSNObjectsRoot>("d0b1fc5e2a004b15976372ff6e8e28f6");

	private static readonly BpRef<DetectiveServoskullRoot> DetectiveServoskullRef = new BpRef<DetectiveServoskullRoot>("2c4c9717465a4ada8337f58eb319b2a9");

	private static readonly BpRef<BlueprintCoverRoot> CoverRef = new BpRef<BlueprintCoverRoot>("ced5b49181044172a8b327d6e7efa44d");

	private static readonly BpRef<AbilityRoot> AbilityRootRef = new BpRef<AbilityRoot>("70e363e085f14771b63dc5d4c5d30061");

	private static readonly BpRef<MoraleRoot> MoraleRootRef = new BpRef<MoraleRoot>("99e47f9cc2b84a9ca8fd2b6879fdbdcb");

	private static readonly BpRef<AoEPatternRoot> AoEPatternRootRef = new BpRef<AoEPatternRoot>("412d08b78e6e4feba729105234aee32e");

	private static readonly BpRef<BlueprintUnitStatsRoot> UnitStatsRootRef = new BpRef<BlueprintUnitStatsRoot>("7143f0eb69c848e0a8ef5fbc1a0f44d6");

	private static readonly BpRef<BlueprintEncounterRoot> EncounterRootRef = new BpRef<BlueprintEncounterRoot>("04387068d0954ae7968800584cea657a");

	private static readonly BpRef<BlueprintItemProgressionRoot> ItemProgressionRootRef = new BpRef<BlueprintItemProgressionRoot>("8faf66ee99404b3fbf50b126759dd61a");

	public CheatRoot Cheats => CheatRootRef;

	public BlueprintCombatRoot CombatRoot => CombatRootRef;

	public FormationsRoot Formations => FormationRootRef;

	public FxRoot FxRoot => FxRootRef;

	public BlueprintCharGenRoot CharGenRoot => CharGenRootRef;

	public HitSystemRoot HitSystemRoot => HitSystemRootRef;

	public PlayerUpgradeActionsRoot PlayerUpgradeActions => PlayerUpgradeActionsRootRef;

	public BlueprintAchievementsRoot Achievements => AchievementsRootRef;

	public FamiliarsRoot FamiliarsRoot => FamiliarsRootRef;

	public ConsoleRoot ConsoleRoot => ConsoleRootRef;

	public BlueprintTrapSettingsRoot BlueprintTrapSettingsRoot => TrapSettingsRootRef;

	public BlueprintInteractionRoot Interaction => InteractionRootRef;

	public BlueprintPsykerRoot PsykerRoot => PsykerRootRef;

	public UnitConditionBuffsRoot UnitConditionBuffs => UnitConditionBuffsRef;

	public BlueprintDestructibleObjectsRoot DestructibleObjectsRoot => DestructibleObjectsRootRef;

	public SkillCheckRoot SkillCheckRoot => SkillCheckRootRef;

	public BlueprintDifficultyRoot DifficultyRoot => DifficultyRootRef;

	public CutscenesRoot CutsceneRoot => CutsceneRootRef;

	public BlueprintTraumaRoot BlueprintTraumaRoot => TraumaRootRef;

	public BlueprintDismembermentRoot BlueprintDismembermentRoot => DismembermentRootRef;

	public BlueprintAlignmentMarksRoot AlignmentMarksRoot => AlignmentMarksRootRef;

	public LevelUpFxLibrary LevelUpFxLibrary => LevelUpFxLibraryRef;

	public DialogRoot Dialog => DialogRootRef;

	public PreciseAttackRoot PreciseAttack => PreciseAttackRootRef;

	public ProgressionRoot Progression => ProgressionRootRef;

	public Prefabs Prefabs => PrefabsRootRef;

	public UIConfig UIConfig => UIConfigRef;

	public QuestsRoot Quests => QuestsRootRef;

	public SystemMechanicsRoot SystemMechanics => SystemMechanicsRootRef;

	public ConsumablesRoot Consumables => ConsumablesRootRef;

	public DlcRoot DlcSettings => DlcRootRef;

	public NewGameRoot NewGameSettings => NewGameSettingsRootRef;

	public CameraRoot CameraRoot => CameraRootRef;

	public LocalizedTexts LocalizedTexts => LocalizedTextsRef;

	public SoundRoot Sound => SoundRootRef;

	public SoundRagdollSettings SoundRagdollSettings => SoundRagdollSettingsRef;

	public SORoot SO => SORootRef;

	public WarhammerDate InitialDate => InitialDateRef;

	public DetectiveSystemRoot DetectiveSystem => DetectiveSystemRootRef;

	public UISettingsRoot UISettingsRoot => SO.UISettingsRoot;

	public CursorRoot Cursors => SO.Cursors;

	public SettingsValues SettingsValues => SO.SettingsValues;

	public DifficultyPresetsList DifficultyList => SO.DifficultyList;

	public CalendarRoot Calendar => SO.Calendar;

	public BlueprintPSNObjectsRoot PSNObjects => PSNObjectsRef;

	public DetectiveServoskullRoot DetectiveServoskull => DetectiveServoskullRef;

	public BlueprintCoverRoot Cover => CoverRef;

	public AbilityRoot AbilityRoot => AbilityRootRef;

	public MoraleRoot MoraleRoot => MoraleRootRef;

	public AoEPatternRoot AoEPatternRoot => AoEPatternRootRef;

	public BlueprintUnitStatsRoot UnitStatsRoot => UnitStatsRootRef;

	public BlueprintEncounterRoot EncounterRoot => EncounterRootRef;

	public BlueprintItemProgressionRoot ItemProgressionRoot => ItemProgressionRootRef;
}
