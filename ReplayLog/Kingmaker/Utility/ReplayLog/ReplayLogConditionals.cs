namespace Kingmaker.Utility.ReplayLog;

public static class ReplayLogConditionals
{
	public static ConditionalReplayLogSettings Parts = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Signals = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Commands = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Fow = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings SleepingUnits = new ConditionalReplayLogSettings(log: true);

	public static ConditionalReplayLogSettings Servoskull = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings TurnController = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings AI = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Facts = new ConditionalReplayLogSettings(log: true);

	public static ConditionalReplayLogSettings Quest = new ConditionalReplayLogSettings(log: true);

	public static ConditionalReplayLogSettings Barks = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Morale = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Interact = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings Animation = new ConditionalReplayLogSettings(log: false, writeStacktrace: true, usePfLog: false, useUnityLog: false);

	public static ConditionalReplayLogSettings Etudes = new ConditionalReplayLogSettings(log: false);

	public static ConditionalReplayLogSettings StopMoveAgent = new ConditionalReplayLogSettings(log: true, writeStacktrace: false, usePfLog: false, useUnityLog: false);

	public static ConditionalReplayLogSettings Rotation = new ConditionalReplayLogSettings(log: true, writeStacktrace: true, usePfLog: false, useUnityLog: false);

	public static ConditionalReplayLogSettings UnitGroups = new ConditionalReplayLogSettings(log: true, writeStacktrace: true, usePfLog: false, useUnityLog: false);

	public static ConditionalReplayLogSettings UnitMemory = new ConditionalReplayLogSettings(log: true, writeStacktrace: true, usePfLog: false, useUnityLog: false);
}
