using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class CheatsStats : IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IGlobalRulebookSubscriber
{
	private static readonly CheatsStats s_Instance = new CheatsStats();

	public Dictionary<StatType, Dictionary<string, int>> SkillChecks => Game.Instance.Player.SkillChecks;

	public CheatsStats()
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			EventBus.Subscribe(this);
		}
	}

	public static void RegisterCommands()
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			SmartConsole.RegisterCommand("set_stat", "Setting stat on selected users; If zero characters selected setting stat on main character; To see all paramater call command without any", SetStat);
			SmartConsole.RegisterCommand("unset_stat", "Clear cheat value of stat on selected users; If zero characters selected clear cheat value of stat on main character; To see all paramater call command without any", UnsetStat);
			SmartConsole.RegisterCommand("dump_skillchecks_info", DumpSkillchecksInfo);
			SmartConsole.RegisterCommand("dump_area_cr", "Dumps current area CR, respecting overrides", DumpCurrentAreaCR);
		}
	}

	private static void SetStat(string parameters)
	{
		string statTypeStr = Utilities.GetParamString(parameters, 1, "Can't parse stat type from given parameters");
		StatType statType = EnumUtils.GetValues<StatType>().FirstOrDefault((StatType i) => i.ToString().Equals(statTypeStr));
		if (statType == StatType.Unknown)
		{
			PFLog.SmartConsole.Log("Can't find stat type: " + statTypeStr);
			return;
		}
		int? paramInt = Utilities.GetParamInt(parameters, 2, "Can't parse quantity from given parameters");
		if (paramInt.HasValue)
		{
			Utilities.GetUnitForCheat().Actor.SetStatCheat(statType, paramInt);
		}
	}

	private static void UnsetStat(string parameters)
	{
		string statTypeStr = Utilities.GetParamString(parameters, 1, "Can't parse stat type from given parameters");
		StatType statType = EnumUtils.GetValues<StatType>().FirstOrDefault((StatType i) => i.ToString().Equals(statTypeStr));
		if (statType == StatType.Unknown)
		{
			PFLog.SmartConsole.Log("Can't find stat type: " + statTypeStr);
		}
		else
		{
			Utilities.GetUnitForCheat().Actor.SetStatCheat(statType, null);
		}
	}

	private static void DumpSkillchecksInfo(string parameters)
	{
		string text = "";
		PFLog.SmartConsole.Log("SkillCheck stats are: ");
		foreach (KeyValuePair<StatType, Dictionary<string, int>> skillCheck in s_Instance.SkillChecks)
		{
			string text2 = string.Format("{0}: passed {1}, failed {2}", skillCheck.Key, skillCheck.Value.Get("passed", 0), skillCheck.Value.Get("failed", 0));
			PFLog.SmartConsole.Log(text2);
			text = text + text2 + "\n";
		}
		GUIUtility.systemCopyBuffer = text;
		PFLog.Default.Log("Skill check stats copied in buffer.");
		bool? paramBool = Utilities.GetParamBool(parameters, 1, null);
		if (!paramBool.HasValue || paramBool.Value)
		{
			s_Instance.SkillChecks.Clear();
			PFLog.Default.Log("Stats was purged");
		}
	}

	private static void DumpCurrentAreaCR(string parameters)
	{
		PFLog.SmartConsole.Log($"Current area CR: {Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0}");
	}

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
	{
		if (BuildModeUtility.IsDevelopment)
		{
			string key = (evt.ResultIsSuccess ? "passed" : "failed");
			if (!SkillChecks.ContainsKey(evt.StatType))
			{
				SkillChecks.Add(evt.StatType, new Dictionary<string, int>());
			}
			if (!SkillChecks.Get(evt.StatType).ContainsKey(key))
			{
				SkillChecks.Get(evt.StatType).Add(key, 0);
			}
			SkillChecks.Get(evt.StatType)[key] = SkillChecks.Get(evt.StatType).Get(key, 0) + 1;
		}
	}
}
