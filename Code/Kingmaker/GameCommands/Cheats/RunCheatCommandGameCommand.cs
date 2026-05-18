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
using UnityEngine;

namespace Kingmaker.GameCommands.Cheats;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class RunCheatCommandGameCommand : GameCommand, IMemoryPackable<RunCheatCommandGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<RunCheatCommandGameCommand>
{
	[Preserve]
	private sealed class RunCheatCommandGameCommandFormatter : MemoryPackFormatter<RunCheatCommandGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RunCheatCommandGameCommand value)
		{
			RunCheatCommandGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref RunCheatCommandGameCommand value)
		{
			RunCheatCommandGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RunCheatCommandGameCommand value)
		{
			RunCheatCommandGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref RunCheatCommandGameCommand value)
		{
			RunCheatCommandGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Command;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string[] m_Args;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
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

	static RunCheatCommandGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "RunCheatCommandGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Command", typeof(string)),
				new FieldInfo("m_Args", typeof(string[]))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RunCheatCommandGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RunCheatCommandGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RunCheatCommandGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RunCheatCommandGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RunCheatCommandGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Command);
		writer.WriteArray(value.m_Args);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RunCheatCommandGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string[] value2;
		string command;
		if (memberCount == 2)
		{
			if (value != null)
			{
				command = value.m_Command;
				value2 = value.m_Args;
				command = reader.ReadString();
				reader.ReadArray(ref value2);
				goto IL_0098;
			}
			command = reader.ReadString();
			value2 = reader.ReadArray<string>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RunCheatCommandGameCommand), 2, memberCount);
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
				value2 = value.m_Args;
			}
			if (memberCount != 0)
			{
				command = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadArray(ref value2);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0098;
			}
		}
		value = new RunCheatCommandGameCommand
		{
			m_Command = command,
			m_Args = value2
		};
		return;
		IL_0098:
		value.m_Command = command;
		value.m_Args = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref RunCheatCommandGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Command");
		writer.WriteString(value.m_Command);
		writer.WriteProperty("m_Args");
		writer.WriteArray(value.m_Args);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref RunCheatCommandGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string command;
		string[] value2;
		if (value == null)
		{
			command = null;
			value2 = null;
		}
		else
		{
			command = value.m_Command;
			value2 = value.m_Args;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Command"))
				{
					if (text == "m_Args")
					{
						value2 = reader.ReadArray<string>();
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
				if (text == "m_Args")
				{
					reader.ReadArray(ref value2);
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
			value.m_Args = value2;
		}
		else
		{
			value = new RunCheatCommandGameCommand
			{
				m_Command = command,
				m_Args = value2
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
