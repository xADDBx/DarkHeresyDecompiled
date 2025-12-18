using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.Logging;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands.Cheats;

[OwlPackable(OwlPackableMode.Generate)]
public class RunExternalCheatGameCommand : GameCommand, IOwlPackable<RunExternalCheatGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_Command;

	[JsonProperty]
	[OwlPackInclude]
	private string m_CommandWithArgs;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RunExternalCheatGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Command", typeof(string)),
			new FieldInfo("m_CommandWithArgs", typeof(string))
		}
	};

	public override bool IsSynchronized => true;

	private RunExternalCheatGameCommand()
	{
	}

	[JsonConstructor]
	public RunExternalCheatGameCommand(string command, string commandWithArgs)
		: this(command, commandWithArgs, null)
	{
	}

	public RunExternalCheatGameCommand(string command, string commandWithArgs, TaskCompletionSource<bool> tcs)
	{
		m_Command = command;
		m_CommandWithArgs = commandWithArgs;
		m_Tcs = tcs;
	}

	protected override async void ExecuteInternal()
	{
		try
		{
			await ExecuteImpl(m_Command, m_CommandWithArgs);
			if (m_Tcs != null)
			{
				m_Tcs.SetResult(result: true);
				return;
			}
			CheatGameCommandSystem.Logger.Log("Executed external command {0} from other player", m_CommandWithArgs);
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

	private static Task ExecuteImpl(string command, string commandWithArgs)
	{
		return CheatsManagerHolder.System.ExternalExecutor.ExecuteExternalWithDefaultLogging(command, commandWithArgs);
	}

	public static Task Create(string command, string commandWithArgs)
	{
		if (!Game.HasInstance || Game.Instance.CurrentModeType == GameModeType.None)
		{
			return ExecuteImpl(command, commandWithArgs);
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		RunExternalCheatGameCommand cmd = new RunExternalCheatGameCommand(command, commandWithArgs, taskCompletionSource);
		Game.Instance.GameCommandQueue.AddCommand(cmd);
		return taskCompletionSource.Task;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RunExternalCheatGameCommand source = new RunExternalCheatGameCommand();
		result = Unsafe.As<RunExternalCheatGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RunExternalCheatGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Command", ref m_Command, state);
		formatter.StringField(1, "m_CommandWithArgs", ref m_CommandWithArgs, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RunExternalCheatGameCommand>();
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
				m_CommandWithArgs = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
