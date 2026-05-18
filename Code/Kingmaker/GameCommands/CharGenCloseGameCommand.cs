using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenCloseGameCommand : GameCommand, IMemoryPackable<CharGenCloseGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenCloseGameCommand>
{
	[Preserve]
	private sealed class CharGenCloseGameCommandFormatter : MemoryPackFormatter<CharGenCloseGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenCloseGameCommand value)
		{
			CharGenCloseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenCloseGameCommand value)
		{
			CharGenCloseGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenCloseGameCommand value)
		{
			CharGenCloseGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenCloseGameCommand value)
		{
			CharGenCloseGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly bool m_WithComplete;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly bool m_SyncPortrait;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenCloseGameCommand(bool m_withComplete, bool m_syncPortrait)
	{
		m_WithComplete = m_withComplete;
		m_SyncPortrait = m_syncPortrait;
	}

	private CharGenCloseGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenCloseHandler h)
		{
			h.HandleClose(m_WithComplete, m_SyncPortrait);
		});
	}

	static CharGenCloseGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenCloseGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_WithComplete", typeof(bool)),
				new FieldInfo("m_SyncPortrait", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenCloseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenCloseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenCloseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenCloseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenCloseGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_WithComplete, in value.m_SyncPortrait);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenCloseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.m_WithComplete;
				value3 = value.m_SyncPortrait;
				reader.ReadUnmanaged<bool>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenCloseGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = false;
				value3 = false;
			}
			else
			{
				value2 = value.m_WithComplete;
				value3 = value.m_SyncPortrait;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenCloseGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenCloseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_WithComplete");
		writer.WriteUnmanaged(value.m_WithComplete);
		writer.WriteProperty("m_SyncPortrait");
		writer.WriteUnmanaged(value.m_SyncPortrait);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenCloseGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		bool v;
		bool v2;
		if (value == null)
		{
			v = false;
			v2 = false;
		}
		else
		{
			v = value.m_WithComplete;
			v2 = value.m_SyncPortrait;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_WithComplete"))
				{
					if (text == "m_SyncPortrait")
					{
						reader.ReadUnmanaged<bool>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<bool>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_WithComplete"))
			{
				if (text == "m_SyncPortrait")
				{
					reader.ReadUnmanaged<bool>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<bool>(out v);
			}
		}
		_ = value;
		value = new CharGenCloseGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenCloseGameCommand source = new CharGenCloseGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenCloseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenCloseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = m_WithComplete;
		formatter.UnmanagedField(0, "m_WithComplete", ref value, state);
		bool value2 = m_SyncPortrait;
		formatter.UnmanagedField(1, "m_SyncPortrait", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenCloseGameCommand>();
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
				Unsafe.AsRef(in m_WithComplete) = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_SyncPortrait) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
