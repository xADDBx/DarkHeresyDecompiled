using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using Kingmaker.Stores;
using Owlcat.Analytics;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Code.Middleware.Metrics;

public static class Metrics
{
	public static readonly AbilityUsageMetricsEvent Ability = new AbilityUsageMetricsEvent();

	public static readonly ToggleAbilityMetricsEvent ToggleAbility = new ToggleAbilityMetricsEvent();

	public static readonly AlignmentMetricsEvent Alignment = new AlignmentMetricsEvent();

	public static readonly ChapterMetricsEvent Chapter = new ChapterMetricsEvent();

	public static readonly ChargenMetricsEvent Chargen = new ChargenMetricsEvent();

	public static readonly CutsceneMetricsEvent Cutscene = new CutsceneMetricsEvent();

	public static readonly DetectiveCaseMetricsEvent DetectiveCase = new DetectiveCaseMetricsEvent();

	public static readonly DialogMetricsEvent Dialog = new DialogMetricsEvent();

	public static readonly EpilogueMetricsEvent Epilogue = new EpilogueMetricsEvent();

	public static readonly DetectivePieceMetricsEvent DetectivePiece = new DetectivePieceMetricsEvent();

	public static readonly EncounterStartMetricsEvent EncounterStart = new EncounterStartMetricsEvent();

	public static readonly EncounterCompanionStartMetricsEvent EncounterCompanionStart = new EncounterCompanionStartMetricsEvent();

	public static readonly EncounterCompanionFinishMetricsEvent EncounterCompanionFinish = new EncounterCompanionFinishMetricsEvent();

	public static readonly EncounterFinishMetricsEvent EncounterFinish = new EncounterFinishMetricsEvent();

	public static readonly EquipmentMetricsEvent Equipment = new EquipmentMetricsEvent();

	public static readonly EtudeMetricsEvent Etude = new EtudeMetricsEvent();

	public static readonly FormationMetricsEvent Formation = new FormationMetricsEvent();

	public static readonly InterfaceMetricsEvent Interface = new InterfaceMetricsEvent();

	public static readonly TabMetricsEvent Tab = new TabMetricsEvent();

	public static readonly LevelUpMetricsEvent LevelUp = new LevelUpMetricsEvent();

	public static readonly LocationMetricsEvent Location = new LocationMetricsEvent();

	public static readonly LocationDataMetricsEvent LocationData = new LocationDataMetricsEvent();

	public static readonly LocationCompanionMetricsEvent LocationCompanion = new LocationCompanionMetricsEvent();

	public static readonly MoralePhaseMetricsEvent Morale = new MoralePhaseMetricsEvent();

	public static readonly PlayerMetricsEvent Player = new PlayerMetricsEvent();

	public static readonly QuestMetricsEvent Quest = new QuestMetricsEvent();

	public static readonly QuestObjectiveMetricsEvent QuestObjective = new QuestObjectiveMetricsEvent();

	public static readonly RecruitMetricsEvent Recruitment = new RecruitMetricsEvent();

	public static readonly ReputationMetricsEvent Reputation = new ReputationMetricsEvent();

	public static readonly NewGameMetricsEvent NewGame = new NewGameMetricsEvent();

	public static readonly GameOverMetricsEvent GameOver = new GameOverMetricsEvent();

	public static readonly SaveMetricsEvent Save = new SaveMetricsEvent();

	public static readonly LoadMetricsEvent Load = new LoadMetricsEvent();

	public static readonly SettingsMetricsEvent Settings = new SettingsMetricsEvent();

	public static readonly SkillCheckMetricsEvent SkillCheck = new SkillCheckMetricsEvent();

	public static readonly VendorDealMetricsEvent VendorDeal = new VendorDealMetricsEvent();

	public static readonly PerformanceMetricsEvent Performance = new PerformanceMetricsEvent();

	private static bool _initialized;

	public static bool Enabled { get; private set; }

	public static void StartDataCollection()
	{
		if (Application.platform == RuntimePlatform.WindowsPlayer && Init())
		{
			AbolethService.Instance.StartDataCollection();
			Enabled = true;
			MetricsEventBusListener.Init();
		}
	}

	public static void StopDataCollection()
	{
		if (_initialized)
		{
			AbolethService.Instance.StopDataCollection();
			Enabled = false;
		}
	}

	private static bool Init()
	{
		if (_initialized)
		{
			return true;
		}
		try
		{
			string metricsProjectId = MiddlewareConfig.Get.MetricsProjectId;
			if (string.IsNullOrEmpty(metricsProjectId))
			{
				PFLog.Metrics.Warning("Empty project ID. Metrics disabled!");
				return false;
			}
			AbolethConfig abolethConfig = new AbolethConfig(metricsProjectId);
			if (StoreManager.Store == StoreType.Steam && SteamManager.Initialized)
			{
				try
				{
					CSteamID steamID = SteamUser.GetSteamID();
					EAccountType eAccountType = steamID.GetEAccountType();
					if (eAccountType != EAccountType.k_EAccountTypePending && eAccountType != 0)
					{
						int value = new System.Random((int)steamID.GetAccountID().m_AccountID).Next();
						byte[] array = new byte[12];
						Span<byte> span;
						Span<byte> span2 = (span = array);
						BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(0, 8), steamID.m_SteamID);
						span = span2;
						BinaryPrimitives.WriteInt32LittleEndian(span.Slice(8, 4), value);
						abolethConfig.UserId = new Guid(MD5.Create().ComputeHash(array)).ToString("N");
					}
				}
				catch (Exception ex)
				{
					PFLog.Metrics.Exception(ex);
				}
			}
			AbolethService.Initialize(abolethConfig);
			_initialized = true;
			return true;
		}
		catch (Exception ex2)
		{
			PFLog.Metrics.Exception(ex2);
			return false;
		}
	}
}
