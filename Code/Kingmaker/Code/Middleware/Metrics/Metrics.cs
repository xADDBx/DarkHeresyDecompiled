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
	public static readonly AbilityUsageMetricsEvent Ability = new AbilityUsageMetricsEvent(isGameEvent: true);

	public static readonly ToggleAbilityMetricsEvent ToggleAbility = new ToggleAbilityMetricsEvent(isGameEvent: true);

	public static readonly AlignmentMetricsEvent Alignment = new AlignmentMetricsEvent(isGameEvent: true);

	public static readonly ChapterMetricsEvent Chapter = new ChapterMetricsEvent(isGameEvent: true);

	public static readonly DetectiveCaseMetricsEvent DetectiveCase = new DetectiveCaseMetricsEvent(isGameEvent: true);

	public static readonly DetectivePieceMetricsEvent DetectivePiece = new DetectivePieceMetricsEvent(isGameEvent: true);

	public static readonly EncounterMetricsEvent Encounter = new EncounterMetricsEvent(isGameEvent: true);

	public static readonly EquipmentMetricsEvent Equipment = new EquipmentMetricsEvent(isGameEvent: true);

	public static readonly EtudeMetricsEvent Etude = new EtudeMetricsEvent(isGameEvent: true);

	public static readonly FormationMetricsEvent Formation = new FormationMetricsEvent(isGameEvent: true);

	public static readonly InterfaceMetricsEvent Interface = new InterfaceMetricsEvent(isGameEvent: true);

	public static readonly LevelUpMetricsEvent LevelUp = new LevelUpMetricsEvent(isGameEvent: true);

	public static readonly LocationMetricsEvent Location = new LocationMetricsEvent(isGameEvent: true);

	public static readonly PlayerMetricsEvent Player = new PlayerMetricsEvent(isGameEvent: true);

	public static readonly QuestMetricsEvent Quest = new QuestMetricsEvent(isGameEvent: true);

	public static readonly RecruitMetricsEvent Recruitment = new RecruitMetricsEvent(isGameEvent: true);

	public static readonly ReputationMetricsEvent Reputation = new ReputationMetricsEvent(isGameEvent: true);

	public static readonly NewGameEvent NewGame = new NewGameEvent(isGameEvent: false);

	public static readonly SaveMetricsEvent Save = new SaveMetricsEvent(isGameEvent: true);

	public static readonly LoadMetricsEvent Load = new LoadMetricsEvent(isGameEvent: false);

	public static readonly SettingsMetricsEvent Settings = new SettingsMetricsEvent(isGameEvent: false);

	public static readonly VendorDealMetricsEvent VendorDeal = new VendorDealMetricsEvent(isGameEvent: true);

	private static bool _initialized;

	public static bool Enabled { get; private set; }

	public static void StartDataCollection()
	{
		if (Application.platform == RuntimePlatform.WindowsPlayer && Init())
		{
			AbolethService.Instance.StartDataCollection();
			Enabled = true;
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
					int value = new System.Random((int)steamID.GetAccountID().m_AccountID).Next();
					byte[] array = new byte[12];
					Span<byte> span;
					Span<byte> span2 = (span = array);
					BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(0, 8), steamID.m_SteamID);
					span = span2;
					BinaryPrimitives.WriteInt32LittleEndian(span.Slice(8, 4), value);
					abolethConfig.UserId = new Guid(MD5.Create().ComputeHash(array)).ToString("N");
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
