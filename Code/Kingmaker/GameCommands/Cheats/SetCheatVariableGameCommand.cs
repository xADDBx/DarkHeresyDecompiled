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
public class SetCheatVariableGameCommand : GameCommand, IOwlPackable<SetCheatVariableGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_Command;

	[JsonProperty]
	[OwlPackInclude]
	private string m_Value;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SetCheatVariableGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Command", typeof(string)),
			new FieldInfo("m_Value", typeof(string))
		}
	};

	public override bool IsSynchronized => true;

	private SetCheatVariableGameCommand()
	{
	}

	[JsonConstructor]
	public SetCheatVariableGameCommand(string command, string value)
		: this(command, value, null)
	{
	}

	[JsonConstructor]
	public SetCheatVariableGameCommand(string command, string value, TaskCompletionSource<bool> tcs)
	{
		m_Command = command;
		m_Value = value;
	}

	protected override async void ExecuteInternal()
	{
		try
		{
			await ExecuteImpl(m_Command, m_Value);
			if (m_Tcs != null)
			{
				m_Tcs.SetResult(result: true);
				return;
			}
			CheatGameCommandSystem.Logger.Log("Set variable {0} to {1} on command from other player", m_Command, m_Value);
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

	private static Task ExecuteImpl(string command, string value)
	{
		return CheatsManagerHolder.System.VariableExecutor.ExecuteSetVariableWithDefaultLogging(command, value);
	}

	public static Task Create(string command, string value)
	{
		if (!Game.HasInstance || Game.Instance.CurrentModeType == GameModeType.None)
		{
			return ExecuteImpl(command, value);
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		SetCheatVariableGameCommand cmd = new SetCheatVariableGameCommand(command, value, taskCompletionSource);
		Game.Instance.GameCommandQueue.AddCommand(cmd);
		return taskCompletionSource.Task;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SetCheatVariableGameCommand source = new SetCheatVariableGameCommand();
		result = Unsafe.As<SetCheatVariableGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetCheatVariableGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Command", ref m_Command, state);
		formatter.StringField(1, "m_Value", ref m_Value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetCheatVariableGameCommand>();
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
				m_Value = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
