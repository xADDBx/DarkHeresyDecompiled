using System.Text;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Utility.ReplayLog;

public class ReplayLogSettings
{
	public static ReplayLogSettings Default = new ReplayLogSettings();

	public static ReplayLogSettings Stacktrace = new ReplayLogSettings(writeStacktrace: true, usePfLog: false, useUnityLog: false);

	public static ReplayLogSettings Log = new ReplayLogSettings(writeStacktrace: false, usePfLog: true, useUnityLog: true);

	public static ReplayLogSettings All = new ReplayLogSettings(writeStacktrace: true, usePfLog: true, useUnityLog: true);

	public static LogSeverity PfLogSeverity = LogSeverity.Warning;

	public static LogType UnityLogSeverity = LogType.Warning;

	private StringBuilder _sb;

	public bool WriteStacktrace;

	public bool UsePFLog;

	public bool UseUnityLog;

	public StringBuilder Sb => _sb ?? (_sb = new StringBuilder());

	public ReplayLogSettings()
	{
	}

	public ReplayLogSettings(bool writeStacktrace, bool usePfLog, bool useUnityLog)
	{
		WriteStacktrace = writeStacktrace;
		UsePFLog = usePfLog;
		UseUnityLog = useUnityLog;
	}
}
