using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PingDialogAnswerGameCommand : GameCommand, IMemoryPackable<PingDialogAnswerGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PingDialogAnswerGameCommand>
{
	[Preserve]
	private sealed class PingDialogAnswerGameCommandFormatter : MemoryPackFormatter<PingDialogAnswerGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingDialogAnswerGameCommand value)
		{
			PingDialogAnswerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PingDialogAnswerGameCommand value)
		{
			PingDialogAnswerGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerGameCommand value)
		{
			PingDialogAnswerGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PingDialogAnswerGameCommand value)
		{
			PingDialogAnswerGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Answer;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_IsHover;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private PingDialogAnswerGameCommand()
	{
	}

	public PingDialogAnswerGameCommand(string m_answer, bool m_ishover)
		: this()
	{
		m_Answer = m_answer;
		m_IsHover = m_ishover;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingDialogAnswerLocally(playerOrEmpty, m_Answer, m_IsHover);
	}

	static PingDialogAnswerGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PingDialogAnswerGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Answer", typeof(string)),
				new FieldInfo("m_IsHover", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingDialogAnswerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingDialogAnswerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingDialogAnswerGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Answer);
		writer.WriteUnmanaged(in value.m_IsHover);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		string answer;
		if (memberCount == 2)
		{
			if (value != null)
			{
				answer = value.m_Answer;
				value2 = value.m_IsHover;
				answer = reader.ReadString();
				reader.ReadUnmanaged<bool>(out value2);
				goto IL_0099;
			}
			answer = reader.ReadString();
			reader.ReadUnmanaged<bool>(out value2);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingDialogAnswerGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				answer = null;
				value2 = false;
			}
			else
			{
				answer = value.m_Answer;
				value2 = value.m_IsHover;
			}
			if (memberCount != 0)
			{
				answer = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value2);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new PingDialogAnswerGameCommand
		{
			m_Answer = answer,
			m_IsHover = value2
		};
		return;
		IL_0099:
		value.m_Answer = answer;
		value.m_IsHover = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PingDialogAnswerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Answer");
		writer.WriteString(value.m_Answer);
		writer.WriteProperty("m_IsHover");
		writer.WriteUnmanaged(value.m_IsHover);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PingDialogAnswerGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string answer;
		bool v;
		if (value == null)
		{
			answer = null;
			v = false;
		}
		else
		{
			answer = value.m_Answer;
			v = value.m_IsHover;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Answer"))
				{
					if (text == "m_IsHover")
					{
						reader.ReadUnmanaged<bool>(out v);
						array[1] = true;
					}
				}
				else
				{
					answer = reader.ReadString();
					array[0] = true;
				}
			}
			else if (!(text == "m_Answer"))
			{
				if (text == "m_IsHover")
				{
					reader.ReadUnmanaged<bool>(out v);
				}
			}
			else
			{
				answer = reader.ReadString();
			}
		}
		if (value != null)
		{
			value.m_Answer = answer;
			value.m_IsHover = v;
		}
		else
		{
			value = new PingDialogAnswerGameCommand
			{
				m_Answer = answer,
				m_IsHover = v
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
		PingDialogAnswerGameCommand source = new PingDialogAnswerGameCommand();
		result = Unsafe.As<PingDialogAnswerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingDialogAnswerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Answer", ref m_Answer, state);
		formatter.UnmanagedField(1, "m_IsHover", ref m_IsHover, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingDialogAnswerGameCommand>();
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
				m_Answer = formatter.ReadString(state);
				break;
			case 1:
				m_IsHover = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
