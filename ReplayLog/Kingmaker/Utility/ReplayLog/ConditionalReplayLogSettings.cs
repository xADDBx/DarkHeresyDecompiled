namespace Kingmaker.Utility.ReplayLog;

public sealed class ConditionalReplayLogSettings : ReplayLogSettings
{
	public new bool Log;

	public ConditionalReplayLogSettings(bool log)
	{
		Log = log;
	}

	public ConditionalReplayLogSettings(bool log, bool writeStacktrace, bool usePfLog, bool useUnityLog)
		: base(writeStacktrace, usePfLog, useUnityLog)
	{
		Log = log;
	}
}
