using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.Logging;
using Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands.Cheats;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class RunExternalCheatGameCommand : GameCommand, IMemoryPackable<RunExternalCheatGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<RunExternalCheatGameCommand>
{
	[Preserve]
	private sealed class RunExternalCheatGameCommandFormatter : MemoryPackFormatter<RunExternalCheatGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RunExternalCheatGameCommand value)
		{
			RunExternalCheatGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref RunExternalCheatGameCommand value)
		{
			RunExternalCheatGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RunExternalCheatGameCommand value)
		{
			RunExternalCheatGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref RunExternalCheatGameCommand value)
		{
			RunExternalCheatGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Command;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_CommandWithArgs;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
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

	[SkipAnalysis]
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

	static RunExternalCheatGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "RunExternalCheatGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Command", typeof(string)),
				new FieldInfo("m_CommandWithArgs", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RunExternalCheatGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RunExternalCheatGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RunExternalCheatGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RunExternalCheatGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RunExternalCheatGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Command);
		writer.WriteString(value.m_CommandWithArgs);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RunExternalCheatGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string command;
		string commandWithArgs;
		if (memberCount == 2)
		{
			if (value != null)
			{
				command = value.m_Command;
				commandWithArgs = value.m_CommandWithArgs;
				command = reader.ReadString();
				commandWithArgs = reader.ReadString();
				goto IL_0093;
			}
			command = reader.ReadString();
			commandWithArgs = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RunExternalCheatGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				command = null;
				commandWithArgs = null;
			}
			else
			{
				command = value.m_Command;
				commandWithArgs = value.m_CommandWithArgs;
			}
			if (memberCount != 0)
			{
				command = reader.ReadString();
				if (memberCount != 1)
				{
					commandWithArgs = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0093;
			}
		}
		value = new RunExternalCheatGameCommand
		{
			m_Command = command,
			m_CommandWithArgs = commandWithArgs
		};
		return;
		IL_0093:
		value.m_Command = command;
		value.m_CommandWithArgs = commandWithArgs;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref RunExternalCheatGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Command");
		writer.WriteString(value.m_Command);
		writer.WriteProperty("m_CommandWithArgs");
		writer.WriteString(value.m_CommandWithArgs);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref RunExternalCheatGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string command;
		string commandWithArgs;
		if (value == null)
		{
			command = null;
			commandWithArgs = null;
		}
		else
		{
			command = value.m_Command;
			commandWithArgs = value.m_CommandWithArgs;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Command"))
				{
					if (text == "m_CommandWithArgs")
					{
						commandWithArgs = reader.ReadString();
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
				if (text == "m_CommandWithArgs")
				{
					commandWithArgs = reader.ReadString();
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
			value.m_CommandWithArgs = commandWithArgs;
		}
		else
		{
			value = new RunExternalCheatGameCommand
			{
				m_Command = command,
				m_CommandWithArgs = commandWithArgs
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
