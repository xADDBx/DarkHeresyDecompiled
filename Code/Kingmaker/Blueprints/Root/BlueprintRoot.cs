using System;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.Gameplay.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.Interaction;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Visual.Animation;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[Obsolete]
[TypeId("bdfe642a3da11b04a80bc782b3ddea00")]
public class BlueprintRoot : BlueprintScriptableObject
{
	[Header("Uncategorized")]
	[SerializeField]
	private BlueprintUnitReference m_DefaultPlayerCharacter;

	[SerializeField]
	private BlueprintUnitReference[] m_SelectablePlayerCharacters;

	[SerializeField]
	private BlueprintFactionReference m_PlayerFaction;

	[SerializeField]
	private BlueprintUnlockableFlagReference m_KingFlag;

	public AnimationSet HumanAnimationSet;

	[SerializeField]
	private bool m_UseLightweightUnit;

	public bool CompanionsAI;

	[SerializeField]
	private BlueprintUnitReference m_InvisibleKittenUnit;

	public GameObject StealthEffectPrefab;

	public GameObject ExitStealthEffectPrefab;

	[SerializeField]
	private BlueprintUnitReference m_CustomCompanion;

	[SerializeField]
	private BlueprintFeatureReference m_NavigatorOccupation;

	public int CustomCompanionBaseCost = 1;

	public int StandartPerceptionRadius = 5;

	public int AreaEffectAutoDestroySeconds = 30;

	public int MinSprintDistance = 10;

	public int MaxWalkDistance = 2;

	public int MinSprintDistanceInCombatCells = 10;

	public int MaxWalkDistanceInCombatCells = 2;

	public Texture2D DefaultDissolveTexture;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintEntityPropertyReference m_AssassinLethalityPropertyRef;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintFeatureReference m_AssassinCareerPathRef;

	[Header("Scriptable Objects")]
	public UISettingsRoot UISettingsRoot;

	public StatusBuffsRoot StatusBuffs;

	public CursorRoot Cursors;

	public SettingsValues SettingsValues;

	[SerializeField]
	private DifficultyPresetsList m_DifficultyList;

	public CalendarRoot Calendar;

	[Header("Roots")]
	[SerializeField]
	private BlueprintWarhammerRootReference m_WarhammerRoot;

	[SerializeField]
	private BlueprintAreaPresetReference m_NewGamePreset;

	public ActionList StartGameActions;

	public DialogRoot Dialog;

	public PreciseAttackRoot PreciseAttack;

	public CheatRoot Cheats;

	public ProgressionRoot Progression;

	public Prefabs Prefabs;

	[SerializeField]
	private UIConfigReference m_UIConfig;

	public QuestsRoot Quests;

	public SystemMechanicsRoot SystemMechanics;

	public DlcRoot DlcSettings;

	public NewGameRoot NewGameSettings;

	[SerializeField]
	private CameraRoot.Reference m_CameraRoot;

	public LocalizedTexts LocalizedTexts;

	public SoundRoot Sound;

	[SerializeField]
	public SoundRagdollSettings.Reference SoundRagdollSettings;

	[SerializeField]
	public WarhammerDate InitialDate;

	[SerializeField]
	private FormationsRootReference m_Formations;

	[SerializeField]
	private FxRootReference m_FxRoot;

	[SerializeField]
	private CharGenRootReference m_CharGenRoot;

	[SerializeField]
	private HitSystemRootReference m_HitSystemRoot;

	[SerializeField]
	private PlayerUpgradeActionsRoot.Reference m_PlayerUpgradeActions;

	[SerializeField]
	private BlueprintAchievementsRoot.Reference m_Achievements;

	[ValidateNotNull]
	[SerializeField]
	private ConsoleRootReference m_ConsoleRoot;

	[SerializeField]
	private BlueprintTrapSettingsRootReference m_BlueprintTrapSettingsRoot;

	[SerializeField]
	private BlueprintInteractionRoot.Referense m_InteractionRoot;

	[ValidateNotNull]
	[SerializeField]
	private FamiliarsRoot.Reference m_FamiliarsRoot;

	[Header("PlayStation")]
	[SerializeField]
	private BlueprintPSNObjectsRootReference m_PSNObjects;
}
