using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Code.Gameplay.Controllers;

public static class MoraleCheats
{
	private static LogChannel Logger => PFLog.SmartConsole;

	[Cheat(Name = "morale_disable_changes")]
	public static bool MoraleDisableChanges { get; set; }

	[Cheat(Name = "morale_add", Description = "Increases or decrease morale by specified value (default = 1)")]
	public static void MoraleAdd(int value = 1)
	{
		BaseUnitEntity unitForCheat = Utilities.GetUnitForCheat();
		Rulebook.Trigger(new RulePerformMoraleChange(unitForCheat, unitForCheat, MoraleEventType.ForcedChangeMorale, value));
	}

	[Cheat(Name = "morale_print_groups", Description = "Prints all morale groups in combat.")]
	public static void MoralePrintGroups()
	{
		int num = 0;
		foreach (MoraleGroup moraleGroup in Game.Instance.Controllers.MoraleController.MoraleGroups)
		{
			if (moraleGroup.IsPlayerEnemy)
			{
				num++;
			}
			Logger.Log(moraleGroup.IsPlayerGroup ? "Player Group:" : $"Enemy Group ({num}):");
			Logger.Log($"    Power balance:  {moraleGroup.PowerValue}");
			Logger.Log($"    Power state:    {moraleGroup.PowerBalanceState}");
			Logger.Log("    Factions:       " + string.Join(", ", moraleGroup.Factions.Select((BlueprintFaction i) => i.name)));
			Logger.Log("--------");
		}
	}
}
