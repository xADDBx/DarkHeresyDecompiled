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
public sealed class PingDialogAnswerVoteGameCommand : GameCommand, IMemoryPackable<PingDialogAnswerVoteGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PingDialogAnswerVoteGameCommand>
{
	[Preserve]
	private sealed class PingDialogAnswerVoteGameCommandFormatter : MemoryPackFormatter<PingDialogAnswerVoteGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingDialogAnswerVoteGameCommand value)
		{
			PingDialogAnswerVoteGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PingDialogAnswerVoteGameCommand value)
		{
			PingDialogAnswerVoteGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerVoteGameCommand value)
		{
			PingDialogAnswerVoteGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PingDialogAnswerVoteGameCommand value)
		{
			PingDialogAnswerVoteGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_Answer;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingDialogAnswerVoteGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingDialogAnswerVoteGameCommand(string m_answer)
		: this()
	{
		m_Answer = m_answer;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingDialogAnswerVoteLocally(playerOrEmpty, m_Answer);
	}

	static PingDialogAnswerVoteGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PingDialogAnswerVoteGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Answer", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerVoteGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingDialogAnswerVoteGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerVoteGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingDialogAnswerVoteGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingDialogAnswerVoteGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.m_Answer);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerVoteGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string answer;
		if (memberCount == 1)
		{
			if (value == null)
			{
				answer = reader.ReadString();
			}
			else
			{
				answer = value.m_Answer;
				answer = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingDialogAnswerVoteGameCommand), 1, memberCount);
				return;
			}
			answer = ((value != null) ? value.m_Answer : null);
			if (memberCount != 0)
			{
				answer = reader.ReadString();
				_ = 1;
			}
			_ = value;
		}
		value = new PingDialogAnswerVoteGameCommand(answer);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PingDialogAnswerVoteGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Answer");
		writer.WriteString(value.m_Answer);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PingDialogAnswerVoteGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string answer = ((value != null) ? value.m_Answer : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Answer")
				{
					answer = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text == "m_Answer")
			{
				answer = reader.ReadString();
			}
		}
		_ = value;
		value = new PingDialogAnswerVoteGameCommand(answer);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PingDialogAnswerVoteGameCommand source = new PingDialogAnswerVoteGameCommand();
		result = Unsafe.As<PingDialogAnswerVoteGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingDialogAnswerVoteGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Answer", ref m_Answer, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingDialogAnswerVoteGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
