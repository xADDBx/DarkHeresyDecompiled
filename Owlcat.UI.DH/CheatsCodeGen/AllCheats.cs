using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View._TmpUI;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Action(FirstLaunchSettingsVM.ClearFirstLaunchPrefs), "void ClearFirstLaunchPrefs()", "clear_first_launch", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(FirstLaunchSettingsVM.SetFirstLaunchPrefs), "void SetFirstLaunchPrefs()", "set_first_launch", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(NetLobbyVM.ClearFirstLaunchPrefs), "void ClearFirstLaunchPrefs()", "clear_net_lobby_tutorial", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(NetLobbyVM.SetFirstLaunchPrefs), "void SetFirstLaunchPrefs()", "set_net_lobby_tutorial", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsOvertips.DumpOvertips), "void DumpOvertips()", "dump_overtips", "Print state of all live MapObject interaction overtip VMs.", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsOvertips.DumpOvertip), "void DumpOvertip(string nameOrId)", "dump_overtip", "Print full state for a MapObjectEntity by UniqueId or GameObject name.", "TraceFromGardenTrue", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "nameOrId",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsOvertips.DumpTraces), "void DumpTraces()", "dump_traces", "Print full state of every DetectiveTraceEntity and DetectiveTraceRootEntity in the current area.", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CursorTest.TestCursor), "void TestCursor()", "testcursor", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CursorTest.NextCursor), "void NextCursor()", "nextcursor", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CursorTest.PrevCursor), "void PrevCursor()", "prevcursor", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>();

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>();

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>();
}
