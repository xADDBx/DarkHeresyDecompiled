using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Analytics;

namespace Kingmaker.Code.Middleware.Metrics;

public abstract class MetricsEvent
{
	public readonly struct PauseToken : IDisposable
	{
		private readonly MetricsEvent _owner;

		public PauseToken(MetricsEvent owner)
		{
			_owner = owner;
		}

		public void Dispose()
		{
			_owner._pauseCounter--;
		}
	}

	private static readonly Dictionary<string, AnalyticsParameter> Parameters = new Dictionary<string, AnalyticsParameter>();

	private static int _eventSessionIndex;

	private const int MEMORY_DURATION = 1000;

	private const int MEMORY_SIZE = 100;

	private int _pauseCounter;

	public static List<(long, int)> Memory = new List<(long, int)>(100);

	protected int Hash;

	private static StringBuilder _debugLogBuilder = new StringBuilder(100);

	protected abstract string Name { get; }

	public PauseToken Pause()
	{
		_pauseCounter++;
		return new PauseToken(this);
	}

	protected void AddParam(string paramName, string paramValue)
	{
		if (!Metrics.Enabled || _pauseCounter > 0)
		{
			return;
		}
		try
		{
			AddParamInternal(paramName, paramValue);
			Hash = HashCode.Combine(Hash, paramName, paramValue);
		}
		catch (Exception ex)
		{
			PFLog.Metrics.Exception(ex);
		}
	}

	protected void AddParam(string paramName, IEnumerable<string> paramValue)
	{
		if (!Metrics.Enabled || _pauseCounter > 0 || paramValue == null)
		{
			return;
		}
		try
		{
			Parameters.Add(paramName, new AnalyticsParameter(paramValue));
			Hash = HashCode.Combine(Hash, paramName, paramValue.GetHashCode());
		}
		catch (Exception ex)
		{
			PFLog.Metrics.Exception(ex);
		}
	}

	public void Send()
	{
		if (!Metrics.Enabled || _pauseCounter > 0)
		{
			return;
		}
		if (CheckSpamAndProcessMemory())
		{
			Parameters.Clear();
			Hash = 0;
			return;
		}
		try
		{
			AddSpecificEventDefaultParameters();
			AddDefaultParameters();
			AbolethService.Instance.TrackEvent(Name, Parameters);
		}
		catch (Exception ex)
		{
			PFLog.Metrics.Exception(ex);
		}
		finally
		{
			Parameters.Clear();
			Hash = 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddParamInternal(string paramName, string paramValue)
	{
		Parameters.Add(paramName, paramValue);
	}

	private void AddDefaultParameters()
	{
		AddParamInternal("event_session_index", _eventSessionIndex++.ToString());
		if (!string.IsNullOrEmpty(Game.Instance.Player.GameId))
		{
			AddParamInternal("game_id", Game.Instance.Player.GameId);
		}
		if (Game.Instance.CurrentlyLoadedArea == null)
		{
			return;
		}
		AddParamInternal("save_id", Game.Instance.SaveId);
		AddParamInternal("time_game", Convert.ToInt64(Game.Instance.Controllers.TimeController.GameTime.TotalMilliseconds).ToString());
		AddParamInternal("location_id", Game.Instance.LoadedArea?.Blueprint.AssetGuid);
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			AddParamInternal("encounter_id", current.Blueprint.AssetGuid);
			int combatRound = Game.Instance.Controllers.TurnController.CombatRound;
			if (combatRound > 0)
			{
				AddParamInternal("encounter_round", combatRound.ToString());
			}
		}
	}

	protected virtual void AddSpecificEventDefaultParameters()
	{
	}

	private bool CheckSpamAndProcessMemory()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		int num2 = 0;
		while (num2 < Memory.Count)
		{
			(long, int) tuple = Memory[num2];
			if (num - tuple.Item1 > 1000)
			{
				List<(long, int)> memory = Memory;
				int index = num2;
				List<(long, int)> memory2 = Memory;
				memory[index] = memory2[memory2.Count - 1];
				Memory.RemoveAt(Memory.Count - 1);
			}
			else
			{
				if (tuple.Item2 == Hash)
				{
					return true;
				}
				num2++;
			}
		}
		Memory.Add((num, Hash));
		return false;
	}

	[Conditional("UNITY_EDITOR")]
	private void LogParamForDebug(string paramName, string paramValue)
	{
		_debugLogBuilder.AppendLine(paramName + ": " + paramValue);
	}

	[Conditional("UNITY_EDITOR")]
	private void LogParamForDebug(string paramName, IEnumerable<string> paramValue)
	{
		_debugLogBuilder.Append(paramName);
		_debugLogBuilder.Append(":");
		foreach (string item in paramValue)
		{
			_debugLogBuilder.Append(item);
			_debugLogBuilder.Append("|");
		}
		_debugLogBuilder.AppendLine();
	}

	[Conditional("UNITY_EDITOR")]
	private void LogEventForDebug()
	{
		PFLog.Metrics.Log($"Metrics event: {Name} {Hash}:\n{_debugLogBuilder}");
		_debugLogBuilder.Clear();
	}

	[Conditional("UNITY_EDITOR")]
	private void LogEventSpamForDebug()
	{
		string text = "";
		foreach (KeyValuePair<string, AnalyticsParameter> parameter in Parameters)
		{
			text += $"\n{parameter.Key} : {parameter.Value}";
		}
		PFLog.Metrics.Warning($"trying to repeat event {Name} {Hash}:\n{text}");
	}
}
