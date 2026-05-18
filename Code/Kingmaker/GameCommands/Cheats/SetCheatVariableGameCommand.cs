using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.Logging;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands.Cheats;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SetCheatVariableGameCommand : GameCommand, IMemoryPackable<SetCheatVariableGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetCheatVariableGameCommand>
{
	[Preserve]
	private sealed class SetCheatVariableGameCommandFormatter : MemoryPackFormatter<SetCheatVariableGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetCheatVariableGameCommand value)
		{
			SetCheatVariableGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetCheatVariableGameCommand value)
		{
			SetCheatVariableGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetCheatVariableGameCommand value)
		{
			SetCheatVariableGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetCheatVariableGameCommand value)
		{
			SetCheatVariableGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Command;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Value;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
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

	static SetCheatVariableGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetCheatVariableGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Command", typeof(string)),
				new FieldInfo("m_Value", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetCheatVariableGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetCheatVariableGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetCheatVariableGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetCheatVariableGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetCheatVariableGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Command);
		writer.WriteString(value.m_Value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetCheatVariableGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string command;
		string value2;
		if (memberCount == 2)
		{
			if (value != null)
			{
				command = value.m_Command;
				value2 = value.m_Value;
				command = reader.ReadString();
				value2 = reader.ReadString();
				goto IL_0093;
			}
			command = reader.ReadString();
			value2 = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetCheatVariableGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				command = null;
				value2 = null;
			}
			else
			{
				command = value.m_Command;
				value2 = value.m_Value;
			}
			if (memberCount != 0)
			{
				command = reader.ReadString();
				if (memberCount != 1)
				{
					value2 = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0093;
			}
		}
		value = new SetCheatVariableGameCommand
		{
			m_Command = command,
			m_Value = value2
		};
		return;
		IL_0093:
		value.m_Command = command;
		value.m_Value = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetCheatVariableGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Command");
		writer.WriteString(value.m_Command);
		writer.WriteProperty("m_Value");
		writer.WriteString(value.m_Value);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetCheatVariableGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string command;
		string value2;
		if (value == null)
		{
			command = null;
			value2 = null;
		}
		else
		{
			command = value.m_Command;
			value2 = value.m_Value;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Command"))
				{
					if (text == "m_Value")
					{
						value2 = reader.ReadString();
						array[1] = true;
					}
				}
				else
				{
					command = reader.ReadString();
					array[0] = true;
				}
			}
			else if (!(text == "m_Command"))
			{
				if (text == "m_Value")
				{
					value2 = reader.ReadString();
				}
			}
			else
			{
				command = reader.ReadString();
			}
		}
		if (value != null)
		{
			value.m_Command = command;
			value.m_Value = value2;
		}
		else
		{
			value = new SetCheatVariableGameCommand
			{
				m_Command = command,
				m_Value = value2
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
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
