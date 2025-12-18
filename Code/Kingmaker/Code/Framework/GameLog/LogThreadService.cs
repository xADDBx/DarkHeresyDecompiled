using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.Code.Framework.GameLog;

public class LogThreadService : IService, IDisposable
{
	private Dictionary<LogChannelType, List<LogThreadBase>> m_Logs = new Dictionary<LogChannelType, List<LogThreadBase>>
	{
		{
			LogChannelType.Common,
			new List<LogThreadBase>
			{
				new InteractionRestrictionLogThread(),
				new KnowledgeLogThread(),
				new PartyEncumbranceLogThread(),
				new UnitEncumbranceLogThread(),
				new AwarenessLogThread(),
				new GameTimeAdvancedLogThread(),
				new WarningNotificationLogThread(),
				new UnitFakeDeathMessageLogThread(),
				new FactionReputationLogThread(),
				new VeilThicknessLogThread(),
				new CriticalEffectStageChangedLogThread(),
				new BodyPartHitAdditionalEffectLogThread(),
				new QuickSlotsReplenishLogThread()
			}
		},
		{
			LogChannelType.AnyCombat,
			new List<LogThreadBase>
			{
				new RulebookDealDamageLogThread(),
				new UnitInitiativeLogThread(),
				new UnitMissedTurnLogThread(),
				new InterruptCurrentTurnLogThread(),
				new HealingLogThread(),
				new RollSkillCheckLogThread(),
				new RulebookCastSpellLogThread(),
				new PartyUseAbilityLogThread(),
				new RulebookSavingThrowLogThread(),
				new AddSeparatorLogThread(),
				new MergeRulePerformSavingThrowLogThread(),
				new MoraleLogThread(),
				new MergeRuleDealDamageLogThread()
			}
		},
		{
			LogChannelType.TacticalCombat,
			new List<LogThreadBase>
			{
				new WarningNotificationLogThread()
			}
		},
		{
			LogChannelType.InGameCombat,
			new List<LogThreadBase>
			{
				new UnitLifeStateChangedLogThread(),
				new PerformAttackLogThread(),
				new GrenadeDealDamageLogThread(),
				new PerformScatterAttackLogThread(),
				new PickLockLogThread(),
				new DisarmTrapLogThread(),
				new RulebookCanApplyBuffLogThread(),
				new MergeRuleCalculateCanApplyBuffLogThread(),
				new ContextActionKillLogThread()
			}
		},
		{
			LogChannelType.LifeEvents,
			new List<LogThreadBase>
			{
				new UnitEquipmentLogThread(),
				new ItemsCollectionLogThread(),
				new FactsCollectionLogThread(),
				new PartyGainExperienceLogThread(),
				new PartyCombatLogThread(),
				new IdentifyLogThread(),
				new UnitGainExperienceLogThread(),
				new TemporaryHitPointsLogThread()
			}
		},
		{
			LogChannelType.Dialog,
			new List<LogThreadBase>
			{
				new CombatLogBarkLogThread(),
				new DialogHistoryLogThread(),
				new DialogLogThread()
			}
		},
		{
			LogChannelType.DialogAndLife,
			new List<LogThreadBase>()
		}
	};

	private static ServiceProxy<LogThreadService> s_Proxy;

	public static LogThreadService Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<LogThreadService>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public IEnumerable<LogThreadBase> AllThreads => m_Logs.SelectMany((KeyValuePair<LogChannelType, List<LogThreadBase>> i) => i.Value);

	public LogThreadService()
	{
		foreach (KeyValuePair<LogChannelType, List<LogThreadBase>> log in m_Logs)
		{
			foreach (LogThreadBase item in log.Value)
			{
				item.StartThread();
			}
		}
	}

	public List<LogThreadBase> GetThreadsByChannelType(params LogChannelType[] channelType)
	{
		List<LogThreadBase> list = new List<LogThreadBase>();
		foreach (LogChannelType key in channelType)
		{
			list.AddRange(m_Logs[key]);
		}
		return list;
	}

	public void Dispose()
	{
		m_Logs.ForEach(delegate(KeyValuePair<LogChannelType, List<LogThreadBase>> z)
		{
			z.Value.ForEach(delegate(LogThreadBase x)
			{
				x.Dispose();
			});
		});
		m_Logs.Clear();
		m_Logs = null;
	}

	public void Cleanup()
	{
		m_Logs.ForEach(delegate(KeyValuePair<LogChannelType, List<LogThreadBase>> z)
		{
			z.Value.ForEach(delegate(LogThreadBase x)
			{
				x.Cleanup();
			});
		});
	}
}
