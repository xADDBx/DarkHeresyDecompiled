using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI;

[OwlPackable(OwlPackableMode.Generate)]
public class PlayerUISettings : IHashable, IOwlPackable, IOwlPackable<PlayerUISettings>
{
	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Vector2 m_LogSize = new Vector2(500f, 200f);

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Vector2 m_LogSizeConsole = new Vector2(415f, 200f);

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private bool m_LogIsPinned;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private int m_LogSelectedFilterIndex;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_GlobalMapPartyHide;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_GlobalMapShowLocationName;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IronManMode = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_OnlyActiveCompanionExpMode = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_TermsOfUseSeen;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private bool m_JournalShowCompletedQuest = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_GlobalMapLocalMapPin;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ShowReviewHint;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private bool m_ShowInspect;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private bool m_ChargenProgressionLockedExpanded;

	[JsonProperty]
	[OwlPackInclude]
	public bool ShowFailedPerceptionChecks;

	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<string, string> SettingsList = new Dictionary<string, string>();

	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<SaleOptions, bool> OptionsDictionaryMap = new Dictionary<SaleOptions, bool>();

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public ItemsFilterType InventoryFilter;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public ItemsSorterType InventorySorter;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool ShowUnavailableItems = true;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool ShowUnavailableFeatures = true;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>> UnitToFavoritesMap = new Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>>();

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool LootExtendedView = true;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public SavedUnitProgressionWindowData SavedUnitProgressionWindowData;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public Quest CurrentQuest;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public EncyclopediaData EncyclopediaData = new EncyclopediaData();

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public CharGenSaveData ChargenData = new CharGenSaveData();

	private IPage m_CurrentEncyclopediaPage;

	public UISettingsManager.SettingsScreen LastSettingsMenuType;

	[JsonProperty]
	[OwlPackInclude]
	public bool DelayNotificatioinSeen;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool HoldCombatLog;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public int CombatLogSizeIndex;

	[JsonProperty]
	[OwlPackInclude]
	public bool AutoEnTurnOptionSeen;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public DetectiveSystemData DetectiveSystemData = new DetectiveSystemData();

	public bool IsTBMSpeedUp;

	private const float LocalMapFastMoveSpeedScale = 3f;

	public float LocalMapZoomScale;

	private bool m_IsLocalMapOpen;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PlayerUISettings",
		OldNames = null,
		Fields = new FieldInfo[32]
		{
			new FieldInfo("m_LogSize", typeof(Vector2)),
			new FieldInfo("m_LogSizeConsole", typeof(Vector2)),
			new FieldInfo("m_LogIsPinned", typeof(bool)),
			new FieldInfo("m_LogSelectedFilterIndex", typeof(int)),
			new FieldInfo("m_GlobalMapPartyHide", typeof(bool)),
			new FieldInfo("m_GlobalMapShowLocationName", typeof(bool)),
			new FieldInfo("m_IronManMode", typeof(bool)),
			new FieldInfo("m_OnlyActiveCompanionExpMode", typeof(bool)),
			new FieldInfo("m_TermsOfUseSeen", typeof(bool)),
			new FieldInfo("m_JournalShowCompletedQuest", typeof(bool)),
			new FieldInfo("m_GlobalMapLocalMapPin", typeof(bool)),
			new FieldInfo("m_ShowReviewHint", typeof(bool)),
			new FieldInfo("m_ShowInspect", typeof(bool)),
			new FieldInfo("m_ChargenProgressionLockedExpanded", typeof(bool)),
			new FieldInfo("ShowFailedPerceptionChecks", typeof(bool)),
			new FieldInfo("SettingsList", typeof(Dictionary<string, string>)),
			new FieldInfo("OptionsDictionaryMap", typeof(Dictionary<SaleOptions, bool>)),
			new FieldInfo("InventoryFilter", typeof(ItemsFilterType)),
			new FieldInfo("InventorySorter", typeof(ItemsSorterType)),
			new FieldInfo("ShowUnavailableItems", typeof(bool)),
			new FieldInfo("ShowUnavailableFeatures", typeof(bool)),
			new FieldInfo("UnitToFavoritesMap", typeof(Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>>)),
			new FieldInfo("LootExtendedView", typeof(bool)),
			new FieldInfo("SavedUnitProgressionWindowData", typeof(SavedUnitProgressionWindowData)),
			new FieldInfo("CurrentQuest", typeof(Quest)),
			new FieldInfo("EncyclopediaData", typeof(EncyclopediaData)),
			new FieldInfo("ChargenData", typeof(CharGenSaveData)),
			new FieldInfo("DelayNotificatioinSeen", typeof(bool)),
			new FieldInfo("HoldCombatLog", typeof(bool)),
			new FieldInfo("CombatLogSizeIndex", typeof(int)),
			new FieldInfo("AutoEnTurnOptionSeen", typeof(bool)),
			new FieldInfo("DetectiveSystemData", typeof(DetectiveSystemData))
		}
	};

	public IPage CurrentEncyclopediaPage
	{
		get
		{
			if (m_CurrentEncyclopediaPage != null && !(m_CurrentEncyclopediaPage is BlueprintEncyclopediaChapter) && !(m_CurrentEncyclopediaPage is BlueprintEncyclopediaGlossaryEntry))
			{
				return m_CurrentEncyclopediaPage;
			}
			BlueprintEncyclopediaChapter blueprintEncyclopediaChapter = UIConfig.Instance?.EncyclopediaDefaultPage?.Get();
			bool num = UIConfig.Instance?.EncyclopediaDefaultPage?.Get() != null;
			BlueprintEncyclopediaGlossaryChapter isGlossaryChapter = blueprintEncyclopediaChapter as BlueprintEncyclopediaGlossaryChapter;
			BlueprintEncyclopediaPage blueprintEncyclopediaPage = ((!num) ? blueprintEncyclopediaChapter : ((isGlossaryChapter != null) ? ((BlueprintEncyclopediaPage)(blueprintEncyclopediaChapter.ChildPages?.FirstOrDefault((BlueprintEncyclopediaPageReference cp) => cp?.Get() == isGlossaryChapter.GetChilds()?.FirstOrDefault()?.GetChilds()?.FirstOrDefault()))) : blueprintEncyclopediaChapter?.ChildPages?.FirstOrDefault()?.Get()));
			return m_CurrentEncyclopediaPage = blueprintEncyclopediaPage ?? blueprintEncyclopediaChapter;
		}
		set
		{
			m_CurrentEncyclopediaPage = value;
		}
	}

	public bool GlobalMapPartyHide
	{
		get
		{
			return m_GlobalMapPartyHide;
		}
		set
		{
			m_GlobalMapPartyHide = value;
		}
	}

	public Vector2 LogSize
	{
		get
		{
			return m_LogSize;
		}
		set
		{
			m_LogSize = value;
		}
	}

	public Vector2 LogSizeConsole
	{
		get
		{
			return m_LogSizeConsole;
		}
		set
		{
			m_LogSizeConsole = value;
		}
	}

	public bool LogIsPinned
	{
		get
		{
			return m_LogIsPinned;
		}
		set
		{
			m_LogIsPinned = value;
		}
	}

	public int LogSelectedFilterIndex
	{
		get
		{
			return m_LogSelectedFilterIndex;
		}
		set
		{
			m_LogSelectedFilterIndex = value;
		}
	}

	public bool GlobalMapShowLocationName
	{
		get
		{
			return m_GlobalMapShowLocationName;
		}
		set
		{
			m_GlobalMapShowLocationName = value;
		}
	}

	public bool JournalShowCompletedQuest
	{
		get
		{
			return m_JournalShowCompletedQuest;
		}
		set
		{
			m_JournalShowCompletedQuest = value;
		}
	}

	public bool GlobalMapLocalMapPin
	{
		get
		{
			return m_GlobalMapLocalMapPin;
		}
		set
		{
			m_GlobalMapLocalMapPin = value;
		}
	}

	public bool ShowInspect
	{
		get
		{
			return m_ShowInspect;
		}
		set
		{
			if (m_ShowInspect != value)
			{
				m_ShowInspect = value;
				EventBus.RaiseEvent(delegate(IShowInspectChanged h)
				{
					h.HandleShowInspect(m_ShowInspect);
				});
			}
		}
	}

	public bool ShowInspectJustToggle
	{
		set
		{
			m_ShowInspect = value;
		}
	}

	public bool ChargenProgressionLockedExpanded
	{
		get
		{
			return m_ChargenProgressionLockedExpanded;
		}
		set
		{
			m_ChargenProgressionLockedExpanded = value;
		}
	}

	public bool ShowReviewHint
	{
		get
		{
			return m_ShowReviewHint;
		}
		set
		{
			m_ShowReviewHint = value;
		}
	}

	public bool IronManMode
	{
		get
		{
			return m_IronManMode;
		}
		set
		{
			m_IronManMode = value;
		}
	}

	public bool OnlyActiveCompanionsReceiveExperience
	{
		get
		{
			return m_OnlyActiveCompanionExpMode;
		}
		set
		{
			m_OnlyActiveCompanionExpMode = value;
		}
	}

	public bool TermsOfUseSeen
	{
		get
		{
			return m_TermsOfUseSeen;
		}
		set
		{
			m_TermsOfUseSeen = value;
		}
	}

	private bool EnableCombatSpeedUp
	{
		get
		{
			if ((SpeedUpMode)SettingsRoot.Game.TurnBased.SpeedUpMode != SpeedUpMode.On)
			{
				if ((SpeedUpMode)SettingsRoot.Game.TurnBased.SpeedUpMode == SpeedUpMode.OnDemand)
				{
					return IsTBMSpeedUp;
				}
				return false;
			}
			return true;
		}
	}

	public bool FastMovement
	{
		get
		{
			if (EnableCombatSpeedUp)
			{
				return SettingsRoot.Game.TurnBased.FastMovement;
			}
			return false;
		}
	}

	public bool FastPartyCast
	{
		get
		{
			if (EnableCombatSpeedUp)
			{
				return SettingsRoot.Game.TurnBased.FastPartyCast;
			}
			return false;
		}
	}

	public float AnimSpeedUpPlayer
	{
		get
		{
			if (!EnableCombatSpeedUp)
			{
				return 1f;
			}
			return SettingsRoot.Game.TurnBased.TimeScaleInPlayerTurn;
		}
	}

	public float AnimSpeedUpNPC
	{
		get
		{
			if (!EnableCombatSpeedUp)
			{
				return 1f;
			}
			return SettingsRoot.Game.TurnBased.TimeScaleInNonPlayerTurn;
		}
	}

	public float TimeScaleAverage => (AnimSpeedUpPlayer + AnimSpeedUpNPC) / 2f;

	public bool IsLocalMapOpen
	{
		get
		{
			return m_IsLocalMapOpen;
		}
		set
		{
			m_IsLocalMapOpen = value;
			Game.Instance.Controllers.TimeController.SetLocalMapFastMove(value, 3f);
		}
	}

	public void DoSpeedUp()
	{
		IsTBMSpeedUp = true;
	}

	public void StopSpeedUp()
	{
		IsTBMSpeedUp = false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_GlobalMapPartyHide);
		result.Append(ref m_GlobalMapShowLocationName);
		result.Append(ref m_IronManMode);
		result.Append(ref m_OnlyActiveCompanionExpMode);
		result.Append(ref m_TermsOfUseSeen);
		result.Append(ref m_GlobalMapLocalMapPin);
		result.Append(ref m_ShowReviewHint);
		result.Append(ref ShowFailedPerceptionChecks);
		Dictionary<string, string> settingsList = SettingsList;
		if (settingsList != null)
		{
			int val = 0;
			foreach (KeyValuePair<string, string> item in settingsList)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val2);
				Hash128 val3 = StringHasher.GetHash128(item.Value);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		Dictionary<SaleOptions, bool> optionsDictionaryMap = OptionsDictionaryMap;
		if (optionsDictionaryMap != null)
		{
			int val4 = 0;
			foreach (KeyValuePair<SaleOptions, bool> item2 in optionsDictionaryMap)
			{
				Hash128 hash2 = default(Hash128);
				SaleOptions obj = item2.Key;
				Hash128 val5 = UnmanagedHasher<SaleOptions>.GetHash128(ref obj);
				hash2.Append(ref val5);
				bool obj2 = item2.Value;
				Hash128 val6 = UnmanagedHasher<bool>.GetHash128(ref obj2);
				hash2.Append(ref val6);
				val4 ^= hash2.GetHashCode();
			}
			result.Append(ref val4);
		}
		result.Append(ref DelayNotificatioinSeen);
		result.Append(ref AutoEnTurnOptionSeen);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PlayerUISettings source = new PlayerUISettings();
		result = Unsafe.As<PlayerUISettings, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PlayerUISettings>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_LogSize", ref m_LogSize, state);
		formatter.Field(1, "m_LogSizeConsole", ref m_LogSizeConsole, state);
		formatter.UnmanagedField(2, "m_LogIsPinned", ref m_LogIsPinned, state);
		formatter.UnmanagedField(3, "m_LogSelectedFilterIndex", ref m_LogSelectedFilterIndex, state);
		formatter.UnmanagedField(4, "m_GlobalMapPartyHide", ref m_GlobalMapPartyHide, state);
		formatter.UnmanagedField(5, "m_GlobalMapShowLocationName", ref m_GlobalMapShowLocationName, state);
		formatter.UnmanagedField(6, "m_IronManMode", ref m_IronManMode, state);
		formatter.UnmanagedField(7, "m_OnlyActiveCompanionExpMode", ref m_OnlyActiveCompanionExpMode, state);
		formatter.UnmanagedField(8, "m_TermsOfUseSeen", ref m_TermsOfUseSeen, state);
		formatter.UnmanagedField(9, "m_JournalShowCompletedQuest", ref m_JournalShowCompletedQuest, state);
		formatter.UnmanagedField(10, "m_GlobalMapLocalMapPin", ref m_GlobalMapLocalMapPin, state);
		formatter.UnmanagedField(11, "m_ShowReviewHint", ref m_ShowReviewHint, state);
		formatter.UnmanagedField(12, "m_ShowInspect", ref m_ShowInspect, state);
		formatter.UnmanagedField(13, "m_ChargenProgressionLockedExpanded", ref m_ChargenProgressionLockedExpanded, state);
		formatter.UnmanagedField(14, "ShowFailedPerceptionChecks", ref ShowFailedPerceptionChecks, state);
		formatter.Field(15, "SettingsList", ref SettingsList, state);
		formatter.Field(16, "OptionsDictionaryMap", ref OptionsDictionaryMap, state);
		formatter.EnumField(17, "InventoryFilter", ref InventoryFilter, state);
		formatter.EnumField(18, "InventorySorter", ref InventorySorter, state);
		formatter.UnmanagedField(19, "ShowUnavailableItems", ref ShowUnavailableItems, state);
		formatter.UnmanagedField(20, "ShowUnavailableFeatures", ref ShowUnavailableFeatures, state);
		formatter.Field(21, "UnitToFavoritesMap", ref UnitToFavoritesMap, state);
		formatter.UnmanagedField(22, "LootExtendedView", ref LootExtendedView, state);
		formatter.Field(23, "SavedUnitProgressionWindowData", ref SavedUnitProgressionWindowData, state);
		formatter.Field(24, "CurrentQuest", ref CurrentQuest, state);
		formatter.Field(25, "EncyclopediaData", ref EncyclopediaData, state);
		formatter.Field(26, "ChargenData", ref ChargenData, state);
		formatter.UnmanagedField(27, "DelayNotificatioinSeen", ref DelayNotificatioinSeen, state);
		formatter.UnmanagedField(28, "HoldCombatLog", ref HoldCombatLog, state);
		formatter.UnmanagedField(29, "CombatLogSizeIndex", ref CombatLogSizeIndex, state);
		formatter.UnmanagedField(30, "AutoEnTurnOptionSeen", ref AutoEnTurnOptionSeen, state);
		formatter.Field(31, "DetectiveSystemData", ref DetectiveSystemData, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PlayerUISettings>();
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
				m_LogSize = formatter.ReadPackable<Vector2>(state);
				break;
			case 1:
				m_LogSizeConsole = formatter.ReadPackable<Vector2>(state);
				break;
			case 2:
				m_LogIsPinned = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LogSelectedFilterIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_GlobalMapPartyHide = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				m_GlobalMapShowLocationName = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				m_IronManMode = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				m_OnlyActiveCompanionExpMode = formatter.ReadUnmanaged<bool>(state);
				break;
			case 8:
				m_TermsOfUseSeen = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_JournalShowCompletedQuest = formatter.ReadUnmanaged<bool>(state);
				break;
			case 10:
				m_GlobalMapLocalMapPin = formatter.ReadUnmanaged<bool>(state);
				break;
			case 11:
				m_ShowReviewHint = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_ShowInspect = formatter.ReadUnmanaged<bool>(state);
				break;
			case 13:
				m_ChargenProgressionLockedExpanded = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				ShowFailedPerceptionChecks = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				SettingsList = formatter.ReadPackable<Dictionary<string, string>>(state);
				break;
			case 16:
				OptionsDictionaryMap = formatter.ReadPackable<Dictionary<SaleOptions, bool>>(state);
				break;
			case 17:
				InventoryFilter = formatter.ReadEnum<ItemsFilterType>(state);
				break;
			case 18:
				InventorySorter = formatter.ReadEnum<ItemsSorterType>(state);
				break;
			case 19:
				ShowUnavailableItems = formatter.ReadUnmanaged<bool>(state);
				break;
			case 20:
				ShowUnavailableFeatures = formatter.ReadUnmanaged<bool>(state);
				break;
			case 21:
				UnitToFavoritesMap = formatter.ReadPackable<Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>>>(state);
				break;
			case 22:
				LootExtendedView = formatter.ReadUnmanaged<bool>(state);
				break;
			case 23:
				SavedUnitProgressionWindowData = formatter.ReadPackable<SavedUnitProgressionWindowData>(state);
				break;
			case 24:
				CurrentQuest = formatter.ReadPackable<Quest>(state);
				break;
			case 25:
				EncyclopediaData = formatter.ReadPackable<EncyclopediaData>(state);
				break;
			case 26:
				ChargenData = formatter.ReadPackable<CharGenSaveData>(state);
				break;
			case 27:
				DelayNotificatioinSeen = formatter.ReadUnmanaged<bool>(state);
				break;
			case 28:
				HoldCombatLog = formatter.ReadUnmanaged<bool>(state);
				break;
			case 29:
				CombatLogSizeIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 30:
				AutoEnTurnOptionSeen = formatter.ReadUnmanaged<bool>(state);
				break;
			case 31:
				DetectiveSystemData = formatter.ReadPackable<DetectiveSystemData>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
