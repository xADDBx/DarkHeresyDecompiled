using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Achievements;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Console.PS5.PSNObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Formations;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Settings.Difficulty;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.ModsInfo;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public sealed class Player : Entity, IDisposable, IHashable, IOwlPackable<Player>
{
	private static class VisitedAreasDataHasher
	{
		public static Hash128 GetHash128(Dictionary<BlueprintArea, List<string>> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (KeyValuePair<BlueprintArea, List<string>> item in obj)
			{
				Hash128 val = SimpleBlueprintHasher.GetHash128(item.Key);
				result.Append(ref val);
				for (int i = 0; i < item.Value.Count; i++)
				{
					Hash128 val2 = StringHasher.GetHash128(item.Value[i]);
					result.Append(ref val2);
				}
			}
			return result;
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class CampaignImportSettings : IHashable, IOwlPackable, IOwlPackable<CampaignImportSettings>
	{
		[JsonProperty]
		[OwlPackInclude]
		public bool LetPlayerChooseSave;

		[JsonProperty]
		[OwlPackInclude]
		public bool AutoImportIfOnlyOneSave;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "CampaignImportSettings",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("LetPlayerChooseSave", typeof(bool)),
				new FieldInfo("AutoImportIfOnlyOneSave", typeof(bool))
			}
		};

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref LetPlayerChooseSave);
			result.Append(ref AutoImportIfOnlyOneSave);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			CampaignImportSettings source = new CampaignImportSettings();
			result = Unsafe.As<CampaignImportSettings, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<CampaignImportSettings>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.UnmanagedField(0, "LetPlayerChooseSave", ref LetPlayerChooseSave, state);
			formatter.UnmanagedField(1, "AutoImportIfOnlyOneSave", ref AutoImportIfOnlyOneSave, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CampaignImportSettings>();
			List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
			formatter.EnterObject();
			for (int i = 0; i < typeInfo.Fields.Length; i++)
			{
				formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
				switch (mappingForType[fieldID])
				{
				case byte.MaxValue:
					formatter.SkipField(size);
					break;
				case 0:
					LetPlayerChooseSave = formatter.ReadUnmanaged<bool>(state);
					break;
				case 1:
					AutoImportIfOnlyOneSave = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public enum CharactersList
	{
		ActiveUnits = 0,
		Everyone = 1,
		AllDetachedUnits = 3,
		DetachedPartyCharacters = 4,
		PartyCharacters = 5,
		PartyExceptMainCharacter = 6
	}

	public class SavedSceneEntry
	{
		[JsonProperty]
		public BlueprintArea Area;

		[JsonProperty]
		public string SceneName;
	}

	public enum GameOverReasonType
	{
		PartyIsDefeated = 0,
		EssentialUnitIsDead = 2,
		KingdomIsDestroyed = 3,
		Won = 4,
		QuestFailed = 5
	}

	public const string ID = "player-id";

	public new static readonly EntityRef<Player> Ref = new EntityRef<Player>("player-id");

	[JsonProperty]
	[OwlPackInclude]
	public DateTime? StartDate;

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAreaPreset StartPreset;

	private UnlockableFlagsManager m_UnlockableFlagsBackingField = new UnlockableFlagsManager();

	[GameStateIgnore]
	[JsonProperty("CurrentArea")]
	[OwlPackInclude]
	public BlueprintArea SavedInArea;

	[GameStateIgnore]
	[JsonProperty("CurrentAreaPart")]
	[OwlPackInclude]
	public BlueprintAreaPart SavedInAreaPart;

	[GameStateIgnore]
	[JsonProperty]
	[OwlPackInclude]
	private Vector3 m_CameraPos;

	[GameStateIgnore]
	[JsonProperty]
	[OwlPackInclude]
	private float m_CameraRot;

	[GameStateIgnore]
	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<StatType, Dictionary<string, int>> SkillChecks = new Dictionary<StatType, Dictionary<string, int>>();

	[JsonProperty]
	[OwlPackInclude]
	public int Chapter;

	[JsonProperty]
	[OwlPackInclude]
	public ItemsCollection SharedStash;

	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<BlueprintItemsStashReference, ItemsCollection> VirtualStashes = new Dictionary<BlueprintItemsStashReference, ItemsCollection>();

	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<EntityRef<MechanicEntity>, int> RespecUsedByChar = new Dictionary<EntityRef<MechanicEntity>, int>();

	[JsonProperty]
	[OwlPackInclude]
	public HashSet<BlueprintBarkBanter> PlayedBanters = new HashSet<BlueprintBarkBanter>();

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintArea PreviousVisitedArea;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool IsShowBlockedColonyProjects = true;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool IsShowFinishedColonyProjects = true;

	[JsonProperty]
	[OwlPackInclude]
	public Vector3? LastPositionOnPreviousVisitedArea;

	[JsonProperty]
	[OwlPackInclude]
	public List<EntityReference> ActivatedSpawners = new List<EntityReference>();

	[JsonProperty]
	[OwlPackInclude]
	public bool IsShowConsoleTooltip = true;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool IsCameraRotateMode = true;

	[Obsolete]
	[JsonProperty]
	[OwlPackInclude]
	public TraumasModification TraumasModification = new TraumasModification();

	[JsonProperty]
	[OwlPackInclude]
	public bool CanAccessStarshipInventory = true;

	[JsonProperty]
	[OwlPackInclude]
	public CountableFlag CannotAccessContracts = new CountableFlag();

	[JsonProperty]
	[OwlPackInclude]
	public List<string> BrokenEntities = new List<string>();

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool ShowFullUnitInfo;

	[JsonProperty]
	[OwlPackInclude]
	public bool ForcedWalk;

	[JsonProperty]
	[OwlPackInclude]
	public readonly CountableFlag CameraScrollLocked = new CountableFlag();

	[OwlPackInclude]
	public readonly HashSet<BlueprintEncounter> CompletedEncounters = new HashSet<BlueprintEncounter>();

	private bool m_CharacterListsValid;

	private readonly List<BaseUnitEntity> m_Party = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_PartyAndPets = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_PartyAndPetsDetached = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_ActiveCompanions = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_RemoteCompanions = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_AllCharacters = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_AllStarships = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_AllCharactersAndStarships = new List<BaseUnitEntity>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Player",
		OldNames = null,
		Fields = new FieldInfo[64]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("GameTime", typeof(TimeSpan)),
			new FieldInfo("RealTime", typeof(TimeSpan)),
			new FieldInfo("StartDate", typeof(DateTime?)),
			new FieldInfo("StartPreset", typeof(BlueprintAreaPreset)),
			new FieldInfo("m_UnlockableFlags", typeof(UnlockableFlagsManager)),
			new FieldInfo("CompanionStories", typeof(CompanionStoriesManager)),
			new FieldInfo("VisitedAreas", typeof(HashSet<BlueprintArea>)),
			new FieldInfo("SavedInArea", typeof(BlueprintArea)),
			new FieldInfo("SavedInAreaPart", typeof(BlueprintAreaPart)),
			new FieldInfo("m_CameraPos", typeof(Vector3)),
			new FieldInfo("m_CameraRot", typeof(float)),
			new FieldInfo("SkillChecks", typeof(Dictionary<StatType, Dictionary<string, int>>)),
			new FieldInfo("ExperienceRatePercent", typeof(int)),
			new FieldInfo("PartyCharacters", typeof(List<UnitReference>)),
			new FieldInfo("UISettings", typeof(PlayerUISettings)),
			new FieldInfo("Money", typeof(long)),
			new FieldInfo("MinDifficultyController", typeof(MinDifficultyController)),
			new FieldInfo("Stalker", typeof(BlueprintUnit)),
			new FieldInfo("Chapter", typeof(int)),
			new FieldInfo("SharedStash", typeof(ItemsCollection)),
			new FieldInfo("VirtualStashes", typeof(Dictionary<BlueprintItemsStashReference, ItemsCollection>)),
			new FieldInfo("Achievements", typeof(AchievementsManager)),
			new FieldInfo("PSNObjects", typeof(PSNObjectsManager)),
			new FieldInfo("InspectUnitsManager", typeof(InspectUnitsManager)),
			new FieldInfo("UpgradeActions", typeof(List<PlayerUpgradeAction>)),
			new FieldInfo("AppliedPlayerUpgraders", typeof(List<BlueprintPlayerUpgrader>)),
			new FieldInfo("IgnoredAppliedPlayerUpgraders", typeof(List<BlueprintPlayerUpgrader>)),
			new FieldInfo("IgnoredNotAppliedPlayerUpgraders", typeof(List<BlueprintPlayerUpgrader>)),
			new FieldInfo("MainCharacter", typeof(UnitReference)),
			new FieldInfo("MainCharacterOriginal", typeof(UnitReference)),
			new FieldInfo("RespecUsedByChar", typeof(Dictionary<EntityRef<MechanicEntity>, int>)),
			new FieldInfo("PlayedBanters", typeof(HashSet<BlueprintBarkBanter>)),
			new FieldInfo("PreviousVisitedArea", typeof(BlueprintArea)),
			new FieldInfo("IsShowBlockedColonyProjects", typeof(bool)),
			new FieldInfo("IsShowFinishedColonyProjects", typeof(bool)),
			new FieldInfo("LastPositionOnPreviousVisitedArea", typeof(Vector3?)),
			new FieldInfo("ActivatedSpawners", typeof(List<EntityReference>)),
			new FieldInfo("IsShowConsoleTooltip", typeof(bool)),
			new FieldInfo("IsCameraRotateMode", typeof(bool)),
			new FieldInfo("TraumasModification", typeof(TraumasModification)),
			new FieldInfo("CanAccessStarshipInventory", typeof(bool)),
			new FieldInfo("CannotAccessContracts", typeof(CountableFlag)),
			new FieldInfo("ItemsToCargo", typeof(HashSet<BlueprintItem>)),
			new FieldInfo("NextEnterPoint", typeof(BlueprintAreaEnterPoint)),
			new FieldInfo("BrokenEntities", typeof(List<string>)),
			new FieldInfo("m_StartNewGameAdditionalContentDlcStatus", typeof(Dictionary<BlueprintDlc, bool>)),
			new FieldInfo("UsedDlcRewards", typeof(List<BlueprintDlcReward>)),
			new FieldInfo("ClaimedDlcRewards", typeof(List<BlueprintDlcReward>)),
			new FieldInfo("ImportedCampaigns", typeof(HashSet<BlueprintCampaign>)),
			new FieldInfo("CampaignsToOfferImport", typeof(Dictionary<BlueprintCampaign, CampaignImportSettings>)),
			new FieldInfo("ShowFullUnitInfo", typeof(bool)),
			new FieldInfo("ForcedWalk", typeof(bool)),
			new FieldInfo("CameraScrollLocked", typeof(CountableFlag)),
			new FieldInfo("CompletedEncounters", typeof(HashSet<BlueprintEncounter>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan GameTime { get; set; } = TimeSpan.Zero;


	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan RealTime { get; set; } = TimeSpan.Zero;


	public SceneEntitiesState CrossSceneState => Game.Instance.State.CrossSceneState;

	[JsonProperty]
	[OwlPackInclude]
	private UnlockableFlagsManager m_UnlockableFlags
	{
		get
		{
			return m_UnlockableFlagsBackingField;
		}
		set
		{
			m_UnlockableFlagsBackingField = value;
			UnlockableFlagsManagerWrapper.Instance.Setup(m_UnlockableFlags);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	public CompanionStoriesManager CompanionStories { get; private set; } = new CompanionStoriesManager();


	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	public HashSet<BlueprintArea> VisitedAreas { get; private set; } = new HashSet<BlueprintArea>();


	[JsonProperty]
	[OwlPackInclude]
	public int ExperienceRatePercent { get; set; } = 100;


	[JsonProperty]
	[OwlPackInclude]
	public List<UnitReference> PartyCharacters { get; private set; } = new List<UnitReference>();


	[JsonProperty]
	[OwlPackInclude]
	public PlayerUISettings UISettings { get; private set; } = new PlayerUISettings();


	[JsonProperty]
	[OwlPackInclude]
	public long Money { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public MinDifficultyController MinDifficultyController { get; private set; } = new MinDifficultyController();


	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintUnit Stalker { get; set; }

	public Encumbrance Encumbrance => Encumbrance.Light;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public AchievementsManager Achievements { get; private set; } = new AchievementsManager();


	[JsonProperty]
	[OwlPackInclude]
	public PSNObjectsManager PSNObjects { get; private set; } = new PSNObjectsManager();


	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public InspectUnitsManager InspectUnitsManager { get; private set; } = new InspectUnitsManager();


	[JsonProperty]
	[OwlPackInclude]
	public List<PlayerUpgradeAction> UpgradeActions { get; private set; } = new List<PlayerUpgradeAction>();


	[JsonProperty]
	[OwlPackInclude]
	public List<BlueprintPlayerUpgrader> AppliedPlayerUpgraders { get; private set; } = new List<BlueprintPlayerUpgrader>();


	[JsonProperty]
	[OwlPackInclude]
	public List<BlueprintPlayerUpgrader> IgnoredAppliedPlayerUpgraders { get; private set; } = new List<BlueprintPlayerUpgrader>();


	[JsonProperty]
	[OwlPackInclude]
	public List<BlueprintPlayerUpgrader> IgnoredNotAppliedPlayerUpgraders { get; private set; } = new List<BlueprintPlayerUpgrader>();


	[JsonProperty]
	[OwlPackInclude]
	public UnitReference MainCharacter { get; private set; }

	public BaseUnitEntity MainCharacterEntity => MainCharacter.Entity.ToBaseUnitEntity();

	[JsonProperty]
	[OwlPackInclude]
	public UnitReference MainCharacterOriginal { get; private set; }

	public BaseUnitEntity MainCharacterOriginalEntity => MainCharacterOriginal.Entity?.ToBaseUnitEntity() ?? MainCharacter.Entity.ToBaseUnitEntity();

	[JsonProperty]
	[OwlPackInclude]
	public HashSet<BlueprintItem> ItemsToCargo { get; private set; } = new HashSet<BlueprintItem>();


	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAreaEnterPoint NextEnterPoint { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<BlueprintDlc, bool> m_StartNewGameAdditionalContentDlcStatus { get; set; } = new Dictionary<BlueprintDlc, bool>();


	[JsonProperty]
	[OwlPackInclude]
	public List<BlueprintDlcReward> UsedDlcRewards { get; private set; } = new List<BlueprintDlcReward>();


	[JsonProperty]
	[OwlPackInclude]
	public List<BlueprintDlcReward> ClaimedDlcRewards { get; private set; } = new List<BlueprintDlcReward>();


	[JsonProperty]
	[OwlPackInclude]
	public HashSet<BlueprintCampaign> ImportedCampaigns { get; private set; } = new HashSet<BlueprintCampaign>();


	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<BlueprintCampaign, CampaignImportSettings> CampaignsToOfferImport { get; private set; } = new Dictionary<BlueprintCampaign, CampaignImportSettings>();


	[GameStateIgnore]
	public IEnumerable<BlueprintDlcReward> DlcRewardsToSave => from reward in UsedDlcRewards.Concat(ClaimedDlcRewards)
		where reward.IsRequiredInSaves
		select reward;

	public GameOverReasonType? GameOverReason { get; private set; }

	public InGameSettings Settings => Game.Instance.State.InGameSettings;

	public VendorsManager VendorsManager => Game.Instance.VendorsManager;

	public PartyFormationManager FormationManager => GetOrCreate<PartyFormationManager>();

	public PartyStrategistManager StrategistManager => GetOrCreate<PartyStrategistManager>();

	public MoraleDataPart MoraleData => GetOrCreate<MoraleDataPart>();

	public UnitPartFlashlight Flashlight => MainCharacter.ToAbstractUnitEntity()?.GetOrCreate<UnitPartFlashlight>();

	public UnitGroup Group => ((BaseUnitEntity)PartyCharacters[0].Entity).CombatGroup.Group;

	public bool CapitalPartyMode => Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode ?? false;

	public bool ModsUser => UserModsData.Instance.PlayingWithMods;

	public IEnumerable<BaseUnitEntity> RemoteCompanions
	{
		get
		{
			UpdateCharacterLists();
			return m_RemoteCompanions;
		}
	}

	public IEnumerable<BaseUnitEntity> AllCrossSceneUnits => CrossSceneState.AllEntityData.OfType<BaseUnitEntity>();

	public List<BaseUnitEntity> Party
	{
		get
		{
			UpdateCharacterLists();
			return m_Party;
		}
	}

	public List<BaseUnitEntity> PartyAndPets
	{
		get
		{
			UpdateCharacterLists();
			return m_PartyAndPets;
		}
	}

	public List<BaseUnitEntity> PartyAndPetsDetached
	{
		get
		{
			UpdateCharacterLists();
			return m_PartyAndPetsDetached;
		}
	}

	public List<BaseUnitEntity> ActiveCompanions
	{
		get
		{
			UpdateCharacterLists();
			return m_ActiveCompanions;
		}
	}

	public List<BaseUnitEntity> AllCharacters
	{
		get
		{
			UpdateCharacterLists();
			return m_AllCharacters;
		}
	}

	public List<BaseUnitEntity> AllStarships
	{
		get
		{
			UpdateCharacterLists();
			return m_AllStarships;
		}
	}

	public List<BaseUnitEntity> AllCharactersAndStarships
	{
		get
		{
			UpdateCharacterLists();
			return m_AllCharactersAndStarships;
		}
	}

	public int PartyLevel
	{
		get
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < PartyCharacters.Count; i++)
			{
				IAbstractUnitEntity entity = PartyCharacters[i].Entity;
				if (entity != null && entity.ToAbstractUnitEntity().IsInGame)
				{
					num += (float)entity.ToBaseUnitEntity().Progression.CharacterLevel;
					num2++;
				}
			}
			return (int)Math.Round(num / (float)num2);
		}
	}

	public BlueprintCampaign Campaign => SimpleBlueprintExtendAsObject.Or(StartPreset, null)?.Campaign;

	public UnlockableFlagsManager UnlockableFlags => m_UnlockableFlags;

	public bool IsInCombat
	{
		get
		{
			ActiveEncounter current = ActiveEncounter.Current;
			if (current != null)
			{
				return !current.IsCompleted;
			}
			return false;
		}
	}

	public bool PlayerIsKing
	{
		get
		{
			BlueprintUnlockableFlag kingFlag = ConfigRoot.Instance.SystemMechanics.KingFlag;
			if (kingFlag != null)
			{
				return UnlockableFlags.IsUnlocked(kingFlag);
			}
			return false;
		}
	}

	public string GameId { get; set; }

	private Player(OwlPackConstructorParameter _)
		: base(_)
	{
		UnlockableFlagsManagerWrapper.Instance.Setup(m_UnlockableFlags);
	}

	public Player()
		: base("player-id", isInGame: true)
	{
		UnlockableFlagsManagerWrapper.Instance.Setup(m_UnlockableFlags);
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		SharedStash = new ItemsCollection(this);
	}

	public void InitializeHack()
	{
		GameTime = ConfigRoot.Instance.InitialDate.GetTime();
		Achievements.Activate();
		PSNObjects.Activate();
		AppliedPlayerUpgraders.AddRange(ConfigRoot.Instance.PlayerUpgradeActions.Upgraders);
		IgnoredNotAppliedPlayerUpgraders.AddRange(ConfigRoot.Instance.PlayerUpgradeActions.IgnoreUpgraders);
	}

	protected override void OnDispose()
	{
		CrossSceneState.Dispose();
		SharedStash.Dispose();
		Achievements.Deactivate();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.Dispose();
		});
		VirtualStashes.Clear();
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		GetOrCreate<TurnDataPart>();
		GetOrCreate<MoraleDataPart>();
		GetOrCreate<PartyFormationManager>();
		GetOrCreate<PartyStrategistManager>();
		MainCharacter.Entity.ToAbstractUnitEntity().GetOrCreate<UnitPartFlashlight>();
		SharedStash.PostLoad();
		MinDifficultyController.PostLoad();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.PrePostLoad();
		});
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.PostLoad();
		});
		CrossSceneState.AllEntityData.OfType<DroppedLoot.EntityData>().ForEach(Game.Instance.Controllers.EntityDestroyer.Destroy);
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			instance.SavedPosition = m_CameraPos;
			instance.SavedRotation = m_CameraRot;
		}
		Achievements.Activate();
		PSNObjects.Activate();
		if (StartPreset == null)
		{
			StartPreset = ConfigRoot.Instance.NewGameSettings.NewGamePreset;
		}
	}

	protected override void OnSubscribe()
	{
		base.OnSubscribe();
		SharedStash?.Subscribe();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.Subscribe();
		});
	}

	protected override void OnUnsubscribe()
	{
		base.OnUnsubscribe();
		SharedStash.Unsubscribe();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.Unsubscribe();
		});
	}

	public void ApplyUpgrades()
	{
		foreach (PlayerUpgradeAction upgradeAction in UpgradeActions)
		{
			try
			{
				upgradeAction.Apply();
			}
			catch (Exception ex)
			{
				PFLog.Default.Error($"Exception while applying upgrade action: {upgradeAction.Type} ({upgradeAction.Blueprint})");
				PFLog.Default.Exception(ex);
			}
		}
		UpgradeActions.Clear();
		if ((bool)ConfigRoot.Instance.PlayerUpgradeActions)
		{
			ConfigRoot.Instance.PlayerUpgradeActions.ApplyUpgrades();
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		CrossSceneState.PreSave();
		SharedStash.PreSave();
		MinDifficultyController.PreSave();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.PreSave();
		});
		CameraRig instance = CameraRig.Instance;
		m_CameraPos = (instance ? instance.transform.position : Vector3.zero);
		m_CameraRot = (instance ? instance.transform.eulerAngles.y : 0f);
	}

	public void SetMainCharacter(BaseUnitEntity unit)
	{
		if (MainCharacter != null)
		{
			RemovePartyCharacter(MainCharacter);
			if (MainCharacterEntity != null)
			{
				MainCharacterEntity.Remove<UnitPartMainCharacter>();
				MainCharacterEntity.Remove<UnitPartFlashlight>();
				EventBus.RaiseEvent(MainCharacter.Entity.ToIBaseUnitEntity(), delegate(IPartyHandler h)
				{
					h.HandleCompanionRemoved(stayInGame: false);
				});
			}
		}
		if (unit != null)
		{
			AddPartyCharacter(unit.FromAbstractUnitEntity());
			unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IPartyHandler>)delegate(IPartyHandler h)
			{
				h.HandleAddCompanion();
			}, isCheckRuntime: true);
		}
		MainCharacter = unit.FromAbstractUnitEntity();
		m_CharacterListsValid = false;
		MainCharacter.Entity.ToAbstractUnitEntity().GetOrCreate<UnitPartMainCharacter>();
		MainCharacter.Entity.ToAbstractUnitEntity().GetOrCreate<UnitPartFlashlight>();
		MainCharacterOriginal = MainCharacter;
	}

	public void SetCompanionToMainCharacter(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		if (MainCharacter.Entity == null)
		{
			throw new InvalidOperationException();
		}
		if (MainCharacterOriginal == null)
		{
			MainCharacterOriginal = MainCharacter;
		}
		MainCharacterEntity.Parts.RemoveAll((UnitPartMainCharacter i) => i.Temporary);
		MainCharacter = unit.FromAbstractUnitEntity();
		unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
		unit.GetOrCreate<UnitPartMainCharacter>().Temporary = MainCharacterOriginalEntity != unit;
		MainCharacter.Entity.ToAbstractUnitEntity().GetOrCreate<UnitPartFlashlight>();
		m_CharacterListsValid = false;
	}

	public void MoveCharacters([NotNull] AreaEnterPoint areaEnterPoint, bool moveFollowers, bool moveCamera)
	{
		areaEnterPoint.PositionCharacters();
		if (moveFollowers)
		{
			foreach (BaseUnitEntity item in Party)
			{
				UnitPartFollowedByUnits optional = item.GetOptional<UnitPartFollowedByUnits>();
				if (optional == null)
				{
					continue;
				}
				foreach (var (abstractUnitEntity2, followerAction2) in Game.Instance.Controllers.FollowersFormationController.CalculateTeleportToLeaderDestinations(optional))
				{
					if ((bool)abstractUnitEntity2.View)
					{
						abstractUnitEntity2.View.StopMoving();
					}
					abstractUnitEntity2.Position = followerAction2.Position;
					abstractUnitEntity2.DesiredOrientation = followerAction2.Orientation;
				}
			}
		}
		if (moveCamera && Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			CameraRig.Instance.ScrollToImmediately(MainCharacter.Entity.Position);
		}
		EventBus.RaiseEvent(delegate(ITeleportHandler h)
		{
			h.HandlePartyTeleport(areaEnterPoint);
		});
	}

	public void GainPartyExperience(int gained, bool isExperienceForDeath = false)
	{
		if (gained < 0)
		{
			PFLog.LevelUp.ErrorWithReport($"Party received invalid amount of experience: {gained}");
			return;
		}
		foreach (BaseUnitEntity item in (Game.Instance.CurrentModeType != GameModeType.SpaceCombat) ? AllCharacters.Where((BaseUnitEntity u) => u.Master == null).Distinct() : AllStarships.Distinct())
		{
			item.Progression.GainExperience(gained, log: false);
		}
		EventBus.RaiseEvent(delegate(IPartyGainExperienceHandler h)
		{
			h.HandlePartyGainExperience(gained, isExperienceForDeath);
		});
	}

	public bool SpendMoney(long amount)
	{
		if (Money < amount)
		{
			return false;
		}
		Money -= amount;
		return true;
	}

	public void GainMoney(long amount)
	{
		Money += amount;
	}

	public void AddCompanion(BaseUnitEntity value, bool remote = false)
	{
		value.GetOrCreate<UnitPartCompanion>().SetState(remote ? CompanionState.Remote : CompanionState.InParty);
		value.Faction.Set(ConfigRoot.Instance.SystemMechanics.PlayerFaction);
		TryUpdateLevel(value);
		if (!remote)
		{
			AddPartyCharacter(value.FromBaseUnitEntity());
		}
		InvalidateCharacterLists();
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		if (currentlyLoadedArea != null && !currentlyLoadedArea.IsPartyArea)
		{
			value.IsInGame = false;
		}
		if (!remote)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)value, (Action<IPartyHandler>)delegate(IPartyHandler h)
			{
				h.HandleCompanionActivated();
			}, isCheckRuntime: true);
		}
	}

	private void TryUpdateLevel(BaseUnitEntity value)
	{
		int experience = MainCharacter.Entity.ToBaseUnitEntity().Progression.Experience;
		if (value.Progression.Experience != experience)
		{
			value.Progression.AdvanceExperienceTo(experience, log: false);
		}
	}

	public void RemoveCompanion(BaseUnitEntity value, bool stayInGame = false)
	{
		if (MainCharacter == value)
		{
			PFLog.Default.Error("Trying to remove Main Character from party");
		}
		else
		{
			RemoveCompanionInternal(value, stayInGame);
		}
	}

	private void RemoveCompanionInternal(BaseUnitEntity value, bool stayInGame = false)
	{
		RemovePartyCharacter(value.FromBaseUnitEntity());
		value.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.Remote);
		m_CharacterListsValid = false;
		if (!stayInGame)
		{
			value.IsInGame = false;
			SelectionManagerFacade.UpdateSelectedUnits();
		}
		EventBus.RaiseEvent((IBaseUnitEntity)value, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame);
		}, isCheckRuntime: true);
	}

	private static void ReplaceCompanionInList(BaseUnitEntity remove, BaseUnitEntity add, List<UnitReference> list)
	{
		int num = list.IndexOf(remove.FromAbstractUnitEntity());
		if (num >= 0)
		{
			list[num] = add.FromAbstractUnitEntity();
		}
	}

	public void DismissCompanion([NotNull] BaseUnitEntity value)
	{
		if ((!(value.GetCompanionOptional()?.CanRemoveFromParty)) ?? true)
		{
			PFLog.Default.Error("Trying to remove story companion from party");
			return;
		}
		RemovePartyCharacter(value.FromAbstractUnitEntity());
		value.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
		m_CharacterListsValid = false;
		value.IsInGame = false;
		foreach (ItemEntity item in Game.Instance.PartySharedInventory.Collection)
		{
			if (item.Owner == value && !item.HoldingSlot.RemoveItem())
			{
				PFLog.Default.Error("Unable to unequip item {0} while dismissing a custom companion: item will disappear!", item.Blueprint);
			}
		}
		Game.Instance.Controllers.EntityDestroyer.Destroy(value);
		EventBus.RaiseEvent((IBaseUnitEntity)value, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame: false);
		}, isCheckRuntime: true);
	}

	public void DetachPartyMember(BaseUnitEntity unit)
	{
		if (!PartyCharacters.Contains(unit.FromBaseUnitEntity()))
		{
			throw new Exception($"Unit {unit} is not in party or already detached");
		}
		if (PartyCharacters.Count < 2)
		{
			throw new Exception("Can't detach all party members");
		}
		RemovePartyCharacter(unit.FromBaseUnitEntity());
		unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InPartyDetached);
		m_CharacterListsValid = false;
		SelectionManagerFacade.UpdateSelectedUnits();
		EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame: false);
		}, isCheckRuntime: true);
	}

	public void AttachPartyMember(BaseUnitEntity unit)
	{
		if (!unit.IsDetached)
		{
			throw new Exception($"Unit {unit} is not detached");
		}
		unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
		AddPartyCharacter(unit.FromBaseUnitEntity());
		m_CharacterListsValid = false;
		EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleAddCompanion();
		}, isCheckRuntime: true);
	}

	public void SwapAttachedAndDetachedPartyMembers()
	{
		List<BaseUnitEntity> list = AllCrossSceneUnits.Where((BaseUnitEntity u) => !u.IsPet && u.IsDetached).ToTempList();
		if (list.Count < 1)
		{
			throw new Exception("Has no detached party members");
		}
		List<UnitReference> list2 = PartyCharacters.ToList();
		list.ForEach(delegate(BaseUnitEntity u)
		{
			AttachPartyMember(u);
		});
		list2.ForEach(delegate(UnitReference u)
		{
			DetachPartyMember(u.ToBaseUnitEntity());
		});
		InvalidateCharacterLists();
	}

	public void InvalidateCharacterLists()
	{
		m_CharacterListsValid = false;
	}

	public void UpdateCharacterLists()
	{
		if (m_CharacterListsValid)
		{
			return;
		}
		m_Party.Clear();
		m_ActiveCompanions.Clear();
		m_PartyAndPets.Clear();
		m_AllCharacters.Clear();
		m_RemoteCompanions.Clear();
		m_AllStarships.Clear();
		m_AllCharactersAndStarships.Clear();
		foreach (UnitReference item in PartyCharacters.ToTempList())
		{
			AddCharacterToLists(item.Entity.ToBaseUnitEntity());
		}
		foreach (Entity allEntityDatum in CrossSceneState.AllEntityData)
		{
			if (allEntityDatum is BaseUnitEntity baseUnitEntity && !m_AllCharactersAndStarships.Contains(baseUnitEntity))
			{
				m_AllCharactersAndStarships.Add(baseUnitEntity);
				AddCharacterToLists(baseUnitEntity);
			}
		}
		m_CharacterListsValid = true;
	}

	private void AddCharacterToLists(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			PFLog.Default.Error("Unit is null! (maybe you load old save)");
			return;
		}
		if (m_AllCharacters.Contains(unit))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = unit.Master ?? unit;
		UnitReference unitReference = baseUnitEntity.FromBaseUnitEntity();
		CompanionState? obj = baseUnitEntity.GetOptional<UnitPartCompanion>()?.State;
		bool flag = obj == CompanionState.InParty;
		bool num = obj == CompanionState.InPartyDetached;
		bool isPet = unit.IsPet;
		if (flag && !PartyCharacters.Contains(unitReference))
		{
			LogChannel.Default.Warning($"Unit {unitReference} in party, but not in party list. Fixing.");
			AddPartyCharacter(unitReference);
		}
		if (!flag && RemovePartyCharacter(unitReference))
		{
			LogChannel.Default.Warning($"Unit {unitReference} not in party, but in party list. Fixing.");
		}
		if (CapitalPartyMode)
		{
			flag = baseUnitEntity.IsMainCharacter;
		}
		if (!isPet && flag)
		{
			m_Party.Add(unit);
		}
		if (!isPet && flag && unitReference != MainCharacter)
		{
			m_ActiveCompanions.Add(unit);
		}
		UnitPartCompanion optional = baseUnitEntity.GetOptional<UnitPartCompanion>();
		if (optional == null || optional.State != CompanionState.Remote)
		{
			UnitPartCompanion optional2 = baseUnitEntity.GetOptional<UnitPartCompanion>();
			if (optional2 == null || optional2.State != CompanionState.InParty || flag)
			{
				goto IL_0163;
			}
		}
		m_RemoteCompanions.Add(unit);
		goto IL_0163;
		IL_0163:
		if (flag && unit.IsInGame)
		{
			m_PartyAndPets.Add(unit);
		}
		if (num && unit.IsInGame)
		{
			m_PartyAndPetsDetached.Add(unit);
		}
		m_AllCharacters.Add(unit);
	}

	private void AddStarshipToLists(BaseUnitEntity starship)
	{
		if (starship == null)
		{
			PFLog.Default.Error("Starship is null! (maybe you load old save)");
		}
		else if (!m_AllStarships.Contains(starship))
		{
			m_AllStarships.Add(starship);
		}
	}

	public IEnumerable<BaseUnitEntity> GetCharactersList(CharactersList type)
	{
		return type switch
		{
			CharactersList.ActiveUnits => PartyAndPets, 
			CharactersList.Everyone => AllCharacters, 
			CharactersList.AllDetachedUnits => AllCharacters.Where((BaseUnitEntity u) => u.IsDetached), 
			CharactersList.DetachedPartyCharacters => AllCharacters.Where((BaseUnitEntity u) => !u.IsPet && u.IsDetached), 
			CharactersList.PartyCharacters => Party, 
			CharactersList.PartyExceptMainCharacter => Party.Where((BaseUnitEntity u) => u != MainCharacterEntity), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public IEnumerable<BaseUnitEntity> GetPartyCharactersForGroupCommand(Vector3 approachPoint, bool checkArea)
	{
		uint area = ObstacleAnalyzer.GetArea(approachPoint);
		return from u in PartyAndPets
			where u.IsDirectlyControllable
			where u.IsMyNetRole()
			where u.CanMove
			where !checkArea || ObstacleAnalyzer.GetArea(u.Position) == area
			where u.Parts.GetOptional<UnitPartSaddled>() == null
			select u;
	}

	public void FixPartyAfterChange()
	{
		if (!Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			return;
		}
		foreach (UnitReference partyCharacter in PartyCharacters)
		{
			BaseUnitEntity baseUnitEntity = partyCharacter.Entity.ToBaseUnitEntity();
			if (baseUnitEntity == null)
			{
				break;
			}
			if (!baseUnitEntity.IsInGame || baseUnitEntity.Suppressed || !AreaService.IsInMechanicBounds(baseUnitEntity.Position))
			{
				baseUnitEntity.IsInGame = true;
				Vector3 vector = Game.Instance.Player.MainCharacter.Entity.Position;
				if ((bool)AstarPath.active)
				{
					FreePlaceSelector.PlaceSpawnPlaces(2, baseUnitEntity.Corpulence, vector);
					vector = FreePlaceSelector.GetRelaxedPosition(1, projectOnGround: true);
				}
				baseUnitEntity.Position = vector;
			}
			if (!baseUnitEntity.Faction.IsDirectlyControllable)
			{
				baseUnitEntity.Faction.Set(ConfigRoot.Instance.SystemMechanics.PlayerFaction);
			}
			if (baseUnitEntity.CombatGroup.Id != Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity().CombatGroup.Id)
			{
				baseUnitEntity.CombatGroup.Id = Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity().CombatGroup.Id;
			}
		}
	}

	public void CreateCustomCompanion(Action<BaseUnitEntity> successCallback = null, int? xp = null, CharGenCompanionType companionType = CharGenCompanionType.Common)
	{
		int characterLevel = MainCharacter.Entity.ToBaseUnitEntity().Progression.CharacterLevel;
		int targetExp = xp ?? ConfigRoot.Instance.Progression.ExperienceTable.GetBonus(characterLevel);
		BaseUnitEntity newCompanion = ConfigRoot.Instance.SystemMechanics.CustomCompanion.CreateEntity();
		newCompanion.Progression.AdvanceExperienceTo(targetExp, log: false);
		UpdateSoundState(MusicStateHandler.MusicState.Chargen);
		CharGenConfig.Create(newCompanion, CharGenMode.NewCompanion, companionType, isCustomCompanionChargen: true).SetOnComplete(OnComplete).SetOnClose(OnClose)
			.SetOnCloseSoundAction(delegate
			{
				UpdateSoundState(MusicStateHandler.MusicState.Setting);
			})
			.OpenUI();
		void OnClose()
		{
			newCompanion.Dispose();
		}
		void OnComplete(BaseUnitEntity newUnit)
		{
			successCallback?.Invoke(newUnit);
		}
		static void UpdateSoundState(MusicStateHandler.MusicState state)
		{
			SoundState.Instance.OnMusicStateChange(state);
		}
	}

	public int GetCustomCompanionCost()
	{
		return ConfigRoot.Instance.SystemMechanics.CustomCompanionBaseCost;
	}

	public int GetMinimumRespecCost()
	{
		return (from ch in AllCrossSceneUnits.Where(delegate(BaseUnitEntity u)
			{
				UnitPartCompanion optional = u.GetOptional<UnitPartCompanion>();
				if (optional == null || optional.State != CompanionState.InParty)
				{
					UnitPartCompanion optional2 = u.GetOptional<UnitPartCompanion>();
					if (optional2 == null)
					{
						return false;
					}
					return optional2.State == CompanionState.Remote;
				}
				return true;
			})
			where ch.Progression != null && PartUnitProgression.CanRespec(ch)
			select ch).Min((BaseUnitEntity ch) => ch.Progression.GetRespecCost());
	}

	public static bool IsForcedToWalk(AbstractUnitEntity owner)
	{
		if (!Game.Instance.Player.ForcedWalk)
		{
			return false;
		}
		if (Game.Instance.Player.PartyAndPets.Contains(owner))
		{
			return true;
		}
		return false;
	}

	public void GameOver(GameOverReasonType reason)
	{
		if (reason == GameOverReasonType.Won)
		{
			Game.Instance.ResetToMainMenu();
		}
		else
		{
			GameOverReason = reason;
			Game.Instance.StartMode(GameModeType.GameOver);
		}
		EventBus.RaiseEvent(delegate(IGameOverHandler h)
		{
			h.HandleGameOver(reason);
		});
	}

	public void ReInitPartyCharacters(List<UnitReference> newCompanions)
	{
		foreach (UnitReference item in PartyCharacters.ToList())
		{
			if (!newCompanions.Contains(item))
			{
				RemoveCompanionInternal(item.Entity.ToBaseUnitEntity());
			}
		}
		foreach (UnitReference newCompanion in newCompanions)
		{
			if (!PartyCharacters.Contains(newCompanion))
			{
				AddCompanion(newCompanion.ToBaseUnitEntity());
			}
		}
		PartyCharacters.Clear();
		foreach (UnitReference newCompanion2 in newCompanions)
		{
			AddPartyCharacter(newCompanion2);
		}
	}

	private void AddPartyCharacter(UnitReference unitReference)
	{
		PartyCharacters.Add(unitReference);
	}

	private bool RemovePartyCharacter(UnitReference unitReference)
	{
		return PartyCharacters.Remove(unitReference);
	}

	public int GetMillennium()
	{
		return ConfigRoot.Instance.InitialDate.Millenniums;
	}

	public int GetAMRCYears()
	{
		return (ConfigRoot.Instance.InitialDate.GetAMRCDate() + GameTime).TotalWarhammerYears();
	}

	public int GetVVYears()
	{
		return (ConfigRoot.Instance.InitialDate.GetVVDate() + GameTime).TotalWarhammerYears();
	}

	public int GetSegments()
	{
		return (ConfigRoot.Instance.InitialDate.Segments.Segments() + GameTime).Segments();
	}

	public void ChangeChapter(int chapter)
	{
		int chapter2 = Chapter;
		Chapter = Math.Max(Chapter, chapter);
		if (chapter2 != Chapter)
		{
			if (chapter2 >= 0)
			{
				Metrics.Chapter.Number(chapter2.ToString()).ChapterState(ChapterMetricsEvent.ChapterStates.Finish).Send();
			}
			Metrics.Chapter.Number(chapter.ToString()).ChapterState(ChapterMetricsEvent.ChapterStates.Start).Send();
			EventBus.RaiseEvent(delegate(IChangeChapterHandler h)
			{
				h.HandleChangeChapter();
			});
		}
	}

	public bool CheckDlcAvailable()
	{
		using (ContextData<DlcExtension.LoadSaveDlcCheck>.Request())
		{
			return DlcRewardsToSave.All((BlueprintDlcReward dlcReward) => dlcReward.IsAvailable);
		}
	}

	public void UpdateAdditionalContentDlcStatus(BlueprintDlc dlc, bool status)
	{
		if (!m_StartNewGameAdditionalContentDlcStatus.TryAdd(dlc, status))
		{
			m_StartNewGameAdditionalContentDlcStatus[dlc] = status;
		}
	}

	public bool GetAdditionalContentDlcStatus(BlueprintDlc dlc)
	{
		return m_StartNewGameAdditionalContentDlcStatus.GetValueOrDefault(dlc, defaultValue: false);
	}

	public IEnumerable<IBlueprintDlc> GetAvailableAdditionalContentDlcForCurrentCampaign()
	{
		foreach (IBlueprintDlc item in StoreManager.GetAllAvailableAdditionalContentDlc())
		{
			if (Campaign != null && item != null && item.Rewards.Any((IBlueprintDlcReward dlcReward) => dlcReward is BlueprintDlcRewardCampaignAdditionalContent blueprintDlcRewardCampaignAdditionalContent && blueprintDlcRewardCampaignAdditionalContent.Campaign == Campaign))
			{
				yield return item;
			}
		}
	}

	public void ApplySwitchOnDlc(List<BlueprintDlc> dlcList)
	{
		try
		{
			foreach (BlueprintDlc dlc in dlcList)
			{
				UpdateAdditionalContentDlcStatus(dlc, status: true);
			}
			Game.Instance.MakeQuickSave(ApplySwitchOnDlcLoad);
		}
		catch (Exception ex)
		{
			PFLog.Default.Error("Exception while applying switch on dlc`s");
			PFLog.Default.Exception(ex);
		}
	}

	private void ApplySwitchOnDlcLoad()
	{
		Game.Instance.QuickLoadGame();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		TimeSpan val2 = GameTime;
		result.Append(ref val2);
		TimeSpan val3 = RealTime;
		result.Append(ref val3);
		if (StartDate.HasValue)
		{
			DateTime val4 = StartDate.Value;
			result.Append(ref val4);
		}
		Hash128 val5 = SimpleBlueprintHasher.GetHash128(StartPreset);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<UnlockableFlagsManager>.GetHash128(m_UnlockableFlags);
		result.Append(ref val6);
		Hash128 val7 = ClassHasher<CompanionStoriesManager>.GetHash128(CompanionStories);
		result.Append(ref val7);
		HashSet<BlueprintArea> visitedAreas = VisitedAreas;
		if (visitedAreas != null)
		{
			int num = 0;
			foreach (BlueprintArea item in visitedAreas)
			{
				num ^= SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		int val8 = ExperienceRatePercent;
		result.Append(ref val8);
		List<UnitReference> partyCharacters = PartyCharacters;
		if (partyCharacters != null)
		{
			for (int i = 0; i < partyCharacters.Count; i++)
			{
				UnitReference obj = partyCharacters[i];
				Hash128 val9 = UnitReferenceHasher.GetHash128(ref obj);
				result.Append(ref val9);
			}
		}
		Hash128 val10 = ClassHasher<PlayerUISettings>.GetHash128(UISettings);
		result.Append(ref val10);
		long val11 = Money;
		result.Append(ref val11);
		Hash128 val12 = ClassHasher<MinDifficultyController>.GetHash128(MinDifficultyController);
		result.Append(ref val12);
		Hash128 val13 = SimpleBlueprintHasher.GetHash128(Stalker);
		result.Append(ref val13);
		result.Append(ref Chapter);
		Hash128 val14 = ClassHasher<ItemsCollection>.GetHash128(SharedStash);
		result.Append(ref val14);
		Dictionary<BlueprintItemsStashReference, ItemsCollection> virtualStashes = VirtualStashes;
		if (virtualStashes != null)
		{
			int val15 = 0;
			foreach (KeyValuePair<BlueprintItemsStashReference, ItemsCollection> item2 in virtualStashes)
			{
				Hash128 hash = default(Hash128);
				Hash128 val16 = BlueprintReferenceHasher.GetHash128(item2.Key);
				hash.Append(ref val16);
				Hash128 val17 = ClassHasher<ItemsCollection>.GetHash128(item2.Value);
				hash.Append(ref val17);
				val15 ^= hash.GetHashCode();
			}
			result.Append(ref val15);
		}
		Hash128 val18 = ClassHasher<PSNObjectsManager>.GetHash128(PSNObjects);
		result.Append(ref val18);
		List<PlayerUpgradeAction> upgradeActions = UpgradeActions;
		if (upgradeActions != null)
		{
			for (int j = 0; j < upgradeActions.Count; j++)
			{
				Hash128 val19 = ClassHasher<PlayerUpgradeAction>.GetHash128(upgradeActions[j]);
				result.Append(ref val19);
			}
		}
		List<BlueprintPlayerUpgrader> appliedPlayerUpgraders = AppliedPlayerUpgraders;
		if (appliedPlayerUpgraders != null)
		{
			for (int k = 0; k < appliedPlayerUpgraders.Count; k++)
			{
				Hash128 val20 = SimpleBlueprintHasher.GetHash128(appliedPlayerUpgraders[k]);
				result.Append(ref val20);
			}
		}
		List<BlueprintPlayerUpgrader> ignoredAppliedPlayerUpgraders = IgnoredAppliedPlayerUpgraders;
		if (ignoredAppliedPlayerUpgraders != null)
		{
			for (int l = 0; l < ignoredAppliedPlayerUpgraders.Count; l++)
			{
				Hash128 val21 = SimpleBlueprintHasher.GetHash128(ignoredAppliedPlayerUpgraders[l]);
				result.Append(ref val21);
			}
		}
		List<BlueprintPlayerUpgrader> ignoredNotAppliedPlayerUpgraders = IgnoredNotAppliedPlayerUpgraders;
		if (ignoredNotAppliedPlayerUpgraders != null)
		{
			for (int m = 0; m < ignoredNotAppliedPlayerUpgraders.Count; m++)
			{
				Hash128 val22 = SimpleBlueprintHasher.GetHash128(ignoredNotAppliedPlayerUpgraders[m]);
				result.Append(ref val22);
			}
		}
		UnitReference obj2 = MainCharacter;
		Hash128 val23 = UnitReferenceHasher.GetHash128(ref obj2);
		result.Append(ref val23);
		UnitReference obj3 = MainCharacterOriginal;
		Hash128 val24 = UnitReferenceHasher.GetHash128(ref obj3);
		result.Append(ref val24);
		Dictionary<EntityRef<MechanicEntity>, int> respecUsedByChar = RespecUsedByChar;
		if (respecUsedByChar != null)
		{
			int val25 = 0;
			foreach (KeyValuePair<EntityRef<MechanicEntity>, int> item3 in respecUsedByChar)
			{
				Hash128 hash2 = default(Hash128);
				EntityRef<MechanicEntity> obj4 = item3.Key;
				Hash128 val26 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj4);
				hash2.Append(ref val26);
				int obj5 = item3.Value;
				Hash128 val27 = UnmanagedHasher<int>.GetHash128(ref obj5);
				hash2.Append(ref val27);
				val25 ^= hash2.GetHashCode();
			}
			result.Append(ref val25);
		}
		HashSet<BlueprintBarkBanter> playedBanters = PlayedBanters;
		if (playedBanters != null)
		{
			int num2 = 0;
			foreach (BlueprintBarkBanter item4 in playedBanters)
			{
				num2 ^= SimpleBlueprintHasher.GetHash128(item4).GetHashCode();
			}
			result.Append(num2);
		}
		Hash128 val28 = SimpleBlueprintHasher.GetHash128(PreviousVisitedArea);
		result.Append(ref val28);
		if (LastPositionOnPreviousVisitedArea.HasValue)
		{
			Vector3 val29 = LastPositionOnPreviousVisitedArea.Value;
			result.Append(ref val29);
		}
		List<EntityReference> activatedSpawners = ActivatedSpawners;
		if (activatedSpawners != null)
		{
			for (int n = 0; n < activatedSpawners.Count; n++)
			{
				Hash128 val30 = ClassHasher<EntityReference>.GetHash128(activatedSpawners[n]);
				result.Append(ref val30);
			}
		}
		result.Append(ref IsShowConsoleTooltip);
		Hash128 val31 = ClassHasher<TraumasModification>.GetHash128(TraumasModification);
		result.Append(ref val31);
		result.Append(ref CanAccessStarshipInventory);
		Hash128 val32 = ClassHasher<CountableFlag>.GetHash128(CannotAccessContracts);
		result.Append(ref val32);
		HashSet<BlueprintItem> itemsToCargo = ItemsToCargo;
		if (itemsToCargo != null)
		{
			int num3 = 0;
			foreach (BlueprintItem item5 in itemsToCargo)
			{
				num3 ^= SimpleBlueprintHasher.GetHash128(item5).GetHashCode();
			}
			result.Append(num3);
		}
		Hash128 val33 = SimpleBlueprintHasher.GetHash128(NextEnterPoint);
		result.Append(ref val33);
		List<string> brokenEntities = BrokenEntities;
		if (brokenEntities != null)
		{
			for (int num4 = 0; num4 < brokenEntities.Count; num4++)
			{
				Hash128 val34 = StringHasher.GetHash128(brokenEntities[num4]);
				result.Append(ref val34);
			}
		}
		Dictionary<BlueprintDlc, bool> startNewGameAdditionalContentDlcStatus = m_StartNewGameAdditionalContentDlcStatus;
		if (startNewGameAdditionalContentDlcStatus != null)
		{
			int val35 = 0;
			foreach (KeyValuePair<BlueprintDlc, bool> item6 in startNewGameAdditionalContentDlcStatus)
			{
				Hash128 hash3 = default(Hash128);
				Hash128 val36 = SimpleBlueprintHasher.GetHash128(item6.Key);
				hash3.Append(ref val36);
				bool obj6 = item6.Value;
				Hash128 val37 = UnmanagedHasher<bool>.GetHash128(ref obj6);
				hash3.Append(ref val37);
				val35 ^= hash3.GetHashCode();
			}
			result.Append(ref val35);
		}
		List<BlueprintDlcReward> usedDlcRewards = UsedDlcRewards;
		if (usedDlcRewards != null)
		{
			for (int num5 = 0; num5 < usedDlcRewards.Count; num5++)
			{
				Hash128 val38 = SimpleBlueprintHasher.GetHash128(usedDlcRewards[num5]);
				result.Append(ref val38);
			}
		}
		List<BlueprintDlcReward> claimedDlcRewards = ClaimedDlcRewards;
		if (claimedDlcRewards != null)
		{
			for (int num6 = 0; num6 < claimedDlcRewards.Count; num6++)
			{
				Hash128 val39 = SimpleBlueprintHasher.GetHash128(claimedDlcRewards[num6]);
				result.Append(ref val39);
			}
		}
		HashSet<BlueprintCampaign> importedCampaigns = ImportedCampaigns;
		if (importedCampaigns != null)
		{
			int num7 = 0;
			foreach (BlueprintCampaign item7 in importedCampaigns)
			{
				num7 ^= SimpleBlueprintHasher.GetHash128(item7).GetHashCode();
			}
			result.Append(num7);
		}
		Dictionary<BlueprintCampaign, CampaignImportSettings> campaignsToOfferImport = CampaignsToOfferImport;
		if (campaignsToOfferImport != null)
		{
			int val40 = 0;
			foreach (KeyValuePair<BlueprintCampaign, CampaignImportSettings> item8 in campaignsToOfferImport)
			{
				Hash128 hash4 = default(Hash128);
				Hash128 val41 = SimpleBlueprintHasher.GetHash128(item8.Key);
				hash4.Append(ref val41);
				Hash128 val42 = ClassHasher<CampaignImportSettings>.GetHash128(item8.Value);
				hash4.Append(ref val42);
				val40 ^= hash4.GetHashCode();
			}
			result.Append(ref val40);
		}
		result.Append(ref ForcedWalk);
		Hash128 val43 = ClassHasher<CountableFlag>.GetHash128(CameraScrollLocked);
		result.Append(ref val43);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Player source = new Player(default(OwlPackConstructorParameter));
		result = Unsafe.As<Player, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<Player>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		TimeSpan value2 = GameTime;
		formatter.Field(10, "GameTime", ref value2, state);
		TimeSpan value3 = RealTime;
		formatter.Field(11, "RealTime", ref value3, state);
		formatter.NullableField(12, "StartDate", ref StartDate, state);
		formatter.Field(13, "StartPreset", ref StartPreset, state);
		UnlockableFlagsManager value4 = m_UnlockableFlags;
		formatter.Field(14, "m_UnlockableFlags", ref value4, state);
		CompanionStoriesManager value5 = CompanionStories;
		formatter.Field(15, "CompanionStories", ref value5, state);
		HashSet<BlueprintArea> value6 = VisitedAreas;
		formatter.Field(16, "VisitedAreas", ref value6, state);
		formatter.Field(17, "SavedInArea", ref SavedInArea, state);
		formatter.Field(18, "SavedInAreaPart", ref SavedInAreaPart, state);
		formatter.Field(19, "m_CameraPos", ref m_CameraPos, state);
		formatter.UnmanagedField(20, "m_CameraRot", ref m_CameraRot, state);
		formatter.Field(21, "SkillChecks", ref SkillChecks, state);
		int value7 = ExperienceRatePercent;
		formatter.UnmanagedField(22, "ExperienceRatePercent", ref value7, state);
		List<UnitReference> value8 = PartyCharacters;
		formatter.Field(23, "PartyCharacters", ref value8, state);
		PlayerUISettings value9 = UISettings;
		formatter.Field(24, "UISettings", ref value9, state);
		long value10 = Money;
		formatter.UnmanagedField(25, "Money", ref value10, state);
		MinDifficultyController value11 = MinDifficultyController;
		formatter.Field(26, "MinDifficultyController", ref value11, state);
		BlueprintUnit value12 = Stalker;
		formatter.Field(27, "Stalker", ref value12, state);
		formatter.UnmanagedField(28, "Chapter", ref Chapter, state);
		formatter.Field(29, "SharedStash", ref SharedStash, state);
		formatter.Field(30, "VirtualStashes", ref VirtualStashes, state);
		AchievementsManager value13 = Achievements;
		formatter.Field(31, "Achievements", ref value13, state);
		PSNObjectsManager value14 = PSNObjects;
		formatter.Field(32, "PSNObjects", ref value14, state);
		InspectUnitsManager value15 = InspectUnitsManager;
		formatter.Field(33, "InspectUnitsManager", ref value15, state);
		List<PlayerUpgradeAction> value16 = UpgradeActions;
		formatter.Field(34, "UpgradeActions", ref value16, state);
		List<BlueprintPlayerUpgrader> value17 = AppliedPlayerUpgraders;
		formatter.Field(35, "AppliedPlayerUpgraders", ref value17, state);
		List<BlueprintPlayerUpgrader> value18 = IgnoredAppliedPlayerUpgraders;
		formatter.Field(36, "IgnoredAppliedPlayerUpgraders", ref value18, state);
		List<BlueprintPlayerUpgrader> value19 = IgnoredNotAppliedPlayerUpgraders;
		formatter.Field(37, "IgnoredNotAppliedPlayerUpgraders", ref value19, state);
		UnitReference value20 = MainCharacter;
		formatter.Field(38, "MainCharacter", ref value20, state);
		UnitReference value21 = MainCharacterOriginal;
		formatter.Field(39, "MainCharacterOriginal", ref value21, state);
		formatter.Field(40, "RespecUsedByChar", ref RespecUsedByChar, state);
		formatter.Field(41, "PlayedBanters", ref PlayedBanters, state);
		formatter.Field(42, "PreviousVisitedArea", ref PreviousVisitedArea, state);
		formatter.UnmanagedField(43, "IsShowBlockedColonyProjects", ref IsShowBlockedColonyProjects, state);
		formatter.UnmanagedField(44, "IsShowFinishedColonyProjects", ref IsShowFinishedColonyProjects, state);
		formatter.NullableField(45, "LastPositionOnPreviousVisitedArea", ref LastPositionOnPreviousVisitedArea, state);
		formatter.Field(46, "ActivatedSpawners", ref ActivatedSpawners, state);
		formatter.UnmanagedField(47, "IsShowConsoleTooltip", ref IsShowConsoleTooltip, state);
		formatter.UnmanagedField(48, "IsCameraRotateMode", ref IsCameraRotateMode, state);
		formatter.Field(49, "TraumasModification", ref TraumasModification, state);
		formatter.UnmanagedField(50, "CanAccessStarshipInventory", ref CanAccessStarshipInventory, state);
		formatter.Field(51, "CannotAccessContracts", ref CannotAccessContracts, state);
		HashSet<BlueprintItem> value22 = ItemsToCargo;
		formatter.Field(52, "ItemsToCargo", ref value22, state);
		BlueprintAreaEnterPoint value23 = NextEnterPoint;
		formatter.Field(53, "NextEnterPoint", ref value23, state);
		formatter.Field(54, "BrokenEntities", ref BrokenEntities, state);
		Dictionary<BlueprintDlc, bool> value24 = m_StartNewGameAdditionalContentDlcStatus;
		formatter.Field(55, "m_StartNewGameAdditionalContentDlcStatus", ref value24, state);
		List<BlueprintDlcReward> value25 = UsedDlcRewards;
		formatter.Field(56, "UsedDlcRewards", ref value25, state);
		List<BlueprintDlcReward> value26 = ClaimedDlcRewards;
		formatter.Field(57, "ClaimedDlcRewards", ref value26, state);
		HashSet<BlueprintCampaign> value27 = ImportedCampaigns;
		formatter.Field(58, "ImportedCampaigns", ref value27, state);
		Dictionary<BlueprintCampaign, CampaignImportSettings> value28 = CampaignsToOfferImport;
		formatter.Field(59, "CampaignsToOfferImport", ref value28, state);
		formatter.UnmanagedField(60, "ShowFullUnitInfo", ref ShowFullUnitInfo, state);
		formatter.UnmanagedField(61, "ForcedWalk", ref ForcedWalk, state);
		CountableFlag value29 = CameraScrollLocked;
		formatter.Field(62, "CameraScrollLocked", ref value29, state);
		HashSet<BlueprintEncounter> value30 = CompletedEncounters;
		formatter.Field(63, "CompletedEncounters", ref value30, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Player>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				GameTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 11:
				RealTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 12:
				StartDate = formatter.ReadNullablePackable<DateTime>(state);
				break;
			case 13:
				StartPreset = formatter.ReadPackable<BlueprintAreaPreset>(state);
				break;
			case 14:
				m_UnlockableFlags = formatter.ReadPackable<UnlockableFlagsManager>(state);
				break;
			case 15:
				CompanionStories = formatter.ReadPackable<CompanionStoriesManager>(state);
				break;
			case 16:
				VisitedAreas = formatter.ReadPackable<HashSet<BlueprintArea>>(state);
				break;
			case 17:
				SavedInArea = formatter.ReadPackable<BlueprintArea>(state);
				break;
			case 18:
				SavedInAreaPart = formatter.ReadPackable<BlueprintAreaPart>(state);
				break;
			case 19:
				m_CameraPos = formatter.ReadPackable<Vector3>(state);
				break;
			case 20:
				m_CameraRot = formatter.ReadUnmanaged<float>(state);
				break;
			case 21:
				SkillChecks = formatter.ReadPackable<Dictionary<StatType, Dictionary<string, int>>>(state);
				break;
			case 22:
				ExperienceRatePercent = formatter.ReadUnmanaged<int>(state);
				break;
			case 23:
				PartyCharacters = formatter.ReadPackable<List<UnitReference>>(state);
				break;
			case 24:
				UISettings = formatter.ReadPackable<PlayerUISettings>(state);
				break;
			case 25:
				Money = formatter.ReadUnmanaged<long>(state);
				break;
			case 26:
				MinDifficultyController = formatter.ReadPackable<MinDifficultyController>(state);
				break;
			case 27:
				Stalker = formatter.ReadPackable<BlueprintUnit>(state);
				break;
			case 28:
				Chapter = formatter.ReadUnmanaged<int>(state);
				break;
			case 29:
				SharedStash = formatter.ReadPackable<ItemsCollection>(state);
				break;
			case 30:
				VirtualStashes = formatter.ReadPackable<Dictionary<BlueprintItemsStashReference, ItemsCollection>>(state);
				break;
			case 31:
				Achievements = formatter.ReadPackable<AchievementsManager>(state);
				break;
			case 32:
				PSNObjects = formatter.ReadPackable<PSNObjectsManager>(state);
				break;
			case 33:
				InspectUnitsManager = formatter.ReadPackable<InspectUnitsManager>(state);
				break;
			case 34:
				UpgradeActions = formatter.ReadPackable<List<PlayerUpgradeAction>>(state);
				break;
			case 35:
				AppliedPlayerUpgraders = formatter.ReadPackable<List<BlueprintPlayerUpgrader>>(state);
				break;
			case 36:
				IgnoredAppliedPlayerUpgraders = formatter.ReadPackable<List<BlueprintPlayerUpgrader>>(state);
				break;
			case 37:
				IgnoredNotAppliedPlayerUpgraders = formatter.ReadPackable<List<BlueprintPlayerUpgrader>>(state);
				break;
			case 38:
				MainCharacter = formatter.ReadPackable<UnitReference>(state);
				break;
			case 39:
				MainCharacterOriginal = formatter.ReadPackable<UnitReference>(state);
				break;
			case 40:
				RespecUsedByChar = formatter.ReadPackable<Dictionary<EntityRef<MechanicEntity>, int>>(state);
				break;
			case 41:
				PlayedBanters = formatter.ReadPackable<HashSet<BlueprintBarkBanter>>(state);
				break;
			case 42:
				PreviousVisitedArea = formatter.ReadPackable<BlueprintArea>(state);
				break;
			case 43:
				IsShowBlockedColonyProjects = formatter.ReadUnmanaged<bool>(state);
				break;
			case 44:
				IsShowFinishedColonyProjects = formatter.ReadUnmanaged<bool>(state);
				break;
			case 45:
				LastPositionOnPreviousVisitedArea = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 46:
				ActivatedSpawners = formatter.ReadPackable<List<EntityReference>>(state);
				break;
			case 47:
				IsShowConsoleTooltip = formatter.ReadUnmanaged<bool>(state);
				break;
			case 48:
				IsCameraRotateMode = formatter.ReadUnmanaged<bool>(state);
				break;
			case 49:
				TraumasModification = formatter.ReadPackable<TraumasModification>(state);
				break;
			case 50:
				CanAccessStarshipInventory = formatter.ReadUnmanaged<bool>(state);
				break;
			case 51:
				CannotAccessContracts = formatter.ReadPackable<CountableFlag>(state);
				break;
			case 52:
				ItemsToCargo = formatter.ReadPackable<HashSet<BlueprintItem>>(state);
				break;
			case 53:
				NextEnterPoint = formatter.ReadPackable<BlueprintAreaEnterPoint>(state);
				break;
			case 54:
				BrokenEntities = formatter.ReadPackable<List<string>>(state);
				break;
			case 55:
				m_StartNewGameAdditionalContentDlcStatus = formatter.ReadPackable<Dictionary<BlueprintDlc, bool>>(state);
				break;
			case 56:
				UsedDlcRewards = formatter.ReadPackable<List<BlueprintDlcReward>>(state);
				break;
			case 57:
				ClaimedDlcRewards = formatter.ReadPackable<List<BlueprintDlcReward>>(state);
				break;
			case 58:
				ImportedCampaigns = formatter.ReadPackable<HashSet<BlueprintCampaign>>(state);
				break;
			case 59:
				CampaignsToOfferImport = formatter.ReadPackable<Dictionary<BlueprintCampaign, CampaignImportSettings>>(state);
				break;
			case 60:
				ShowFullUnitInfo = formatter.ReadUnmanaged<bool>(state);
				break;
			case 61:
				ForcedWalk = formatter.ReadUnmanaged<bool>(state);
				break;
			case 62:
				Unsafe.AsRef(in CameraScrollLocked) = formatter.ReadPackable<CountableFlag>(state);
				break;
			case 63:
				Unsafe.AsRef(in CompletedEncounters) = formatter.ReadPackable<HashSet<BlueprintEncounter>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
