using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class DialogAnswerGameCommand : GameCommand, IMemoryPackable<DialogAnswerGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<DialogAnswerGameCommand>
{
	[Preserve]
	private sealed class DialogAnswerGameCommandFormatter : MemoryPackFormatter<DialogAnswerGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DialogAnswerGameCommand value)
		{
			DialogAnswerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref DialogAnswerGameCommand value)
		{
			DialogAnswerGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DialogAnswerGameCommand value)
		{
			DialogAnswerGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref DialogAnswerGameCommand value)
		{
			DialogAnswerGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_Tick;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Answer;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private DialogAnswerGameCommand()
	{
	}

	[JsonConstructor]
	public DialogAnswerGameCommand(int tick, [NotNull] string answer)
	{
		m_Tick = tick;
		m_Answer = answer;
	}

	protected override void ExecuteInternal()
	{
		if (!Game.Instance.Controllers.DialogController.CuePlayScheduled && m_Tick >= Game.Instance.Controllers.DialogController.CurrentCueUpdateTick)
		{
			Game.Instance.Controllers.DialogController.SelectAnswer(m_Answer);
		}
	}

	static DialogAnswerGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "DialogAnswerGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Tick", typeof(int)),
				new FieldInfo("m_Answer", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DialogAnswerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DialogAnswerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DialogAnswerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DialogAnswerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DialogAnswerGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(2, in value.m_Tick);
		writer.WriteString(value.m_Answer);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DialogAnswerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		string answer;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Tick;
				answer = value.m_Answer;
				reader.ReadUnmanaged<int>(out value2);
				answer = reader.ReadString();
				goto IL_0099;
			}
			reader.ReadUnmanaged<int>(out value2);
			answer = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DialogAnswerGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				answer = null;
			}
			else
			{
				value2 = value.m_Tick;
				answer = value.m_Answer;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					answer = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new DialogAnswerGameCommand
		{
			m_Tick = value2,
			m_Answer = answer
		};
		return;
		IL_0099:
		value.m_Tick = value2;
		value.m_Answer = answer;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref DialogAnswerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Tick");
		writer.WriteUnmanaged(value.m_Tick);
		writer.WriteProperty("m_Answer");
		writer.WriteString(value.m_Answer);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref DialogAnswerGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v;
		string answer;
		if (value == null)
		{
			v = 0;
			answer = null;
		}
		else
		{
			v = value.m_Tick;
			answer = value.m_Answer;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Tick"))
				{
					if (text == "m_Answer")
					{
						answer = reader.ReadString();
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_Tick"))
			{
				if (text == "m_Answer")
				{
					answer = reader.ReadString();
				}
			}
			else
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		if (value != null)
		{
			value.m_Tick = v;
			value.m_Answer = answer;
		}
		else
		{
			value = new DialogAnswerGameCommand
			{
				m_Tick = v,
				m_Answer = answer
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
		DialogAnswerGameCommand source = new DialogAnswerGameCommand();
		result = Unsafe.As<DialogAnswerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DialogAnswerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Tick", ref m_Tick, state);
		formatter.StringField(1, "m_Answer", ref m_Answer, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DialogAnswerGameCommand>();
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
				m_Tick = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_Answer = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
