using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.Logging;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.GameCommands.Cheats;

[OwlPackable(OwlPackableMode.Generate)]
public class RunCheatCommandGameCommand : GameCommand, IOwlPackable<RunCheatCommandGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_Command;

	[JsonProperty]
	[OwlPackInclude]
	private string[] m_Args;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RunCheatCommandGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Command", typeof(string)),
			new FieldInfo("m_Args", typeof(string[]))
		}
	};

	public override bool IsSynchronized => true;

	private RunCheatCommandGameCommand()
	{
	}

	[JsonConstructor]
	public RunCheatCommandGameCommand(string command, string[] args)
		: this(command, args, null)
	{
	}

	public RunCheatCommandGameCommand(string command, string[] args, TaskCompletionSource<bool> tcs)
	{
		m_Command = command;
		m_Args = args;
		m_Tcs = tcs;
	}

	protected override async void ExecuteInternal()
	{
		try
		{
			await ExecuteImpl(m_Command, m_Args);
			if (m_Tcs != null)
			{
				m_Tcs.SetResult(result: true);
				return;
			}
			CheatGameCommandSystem.Logger.Log("Executed command {0} {1} received from other player", m_Command, string.Join(" ", m_Args));
		}
		catch (Exception ex)
		{
			if (m_Tcs != null)
			{
				m_Tcs.SetException(ex);
			}
			else
			{
				CheatGameCommandSystem.Logger.Exception(ex);
			}
		}
	}

	private static Task ExecuteImpl(string command, string[] args)
	{
		return CheatsManagerHolder.System.CommandExecutor.ExecuteCommandWithDefaultLogging(command, args);
	}

	public static Task Create(string command, string[] args)
	{
		if (!Application.isPlaying || !Game.HasInstance || Game.Instance.CurrentModeType == GameModeType.None)
		{
			return ExecuteImpl(command, args);
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		RunCheatCommandGameCommand cmd = new RunCheatCommandGameCommand(command, args, taskCompletionSource);
		Game.Instance.GameCommandQueue.AddCommand(cmd);
		return taskCompletionSource.Task;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RunCheatCommandGameCommand source = new RunCheatCommandGameCommand();
		result = Unsafe.As<RunCheatCommandGameCommand, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<RunCheatCommandGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Command", ref m_Command, state);
		formatter.Field(1, "m_Args", ref m_Args, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RunCheatCommandGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_Command = formatter.ReadString(state);
				break;
			case 1:
				m_Args = formatter.ReadPackable<string[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
