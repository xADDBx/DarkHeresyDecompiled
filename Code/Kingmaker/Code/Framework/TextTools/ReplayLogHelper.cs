using System.Diagnostics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.ReplayLog;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Code.Framework.TextTools;

public static class ReplayLogHelper
{
	[StackTraceIgnore]
	[Conditional("REPLAY_LOG_ENABLED")]
	public static void Add(AbstractUnitEntityView unit, string message, ReplayLogSettings settings = null)
	{
	}

	[StackTraceIgnore]
	[Conditional("REPLAY_LOG_ENABLED")]
	public static void Add(Entity unit, string message, ConditionalReplayLogSettings settings)
	{
	}

	[StackTraceIgnore]
	[Conditional("REPLAY_LOG_ENABLED")]
	public static void Add(Entity unit, string message, ReplayLogSettings settings = null)
	{
	}

	[StackTraceIgnore]
	[Conditional("REPLAY_LOG_ENABLED")]
	public static void AddIf(bool condition, Entity unit, string message, ConditionalReplayLogSettings settings)
	{
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	public static void Append(string message, ReplayLogSettings settings)
	{
		settings.Sb.Append(message);
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	public static void Append(AbstractUnitEntity unit, string message, ConditionalReplayLogSettings settings)
	{
		settings.Sb.Append(unit.NameForLog() + " " + message);
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	public static void AddFromSb(ReplayLogSettings settings)
	{
		settings.Sb.ToString();
		settings.Sb.Clear();
	}

	public static ConditionalReplayLogSettings Stacktrace(this ConditionalReplayLogSettings settings)
	{
		settings.WriteStacktrace = true;
		return settings;
	}

	public static string NameForLog(this Entity unit)
	{
		if (unit != null)
		{
			if (unit is MechanicEntity mechanicEntity)
			{
				return mechanicEntity.UniqueId + "_" + mechanicEntity.Name;
			}
			return unit.UniqueId;
		}
		return string.Empty;
	}
}
