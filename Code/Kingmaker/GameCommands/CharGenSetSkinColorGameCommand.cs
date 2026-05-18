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
public sealed class CharGenSetSkinColorGameCommand : GameCommand, IMemoryPackable<CharGenSetSkinColorGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetSkinColorGameCommand>
{
	[Preserve]
	private sealed class CharGenSetSkinColorGameCommandFormatter : MemoryPackFormatter<CharGenSetSkinColorGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetSkinColorGameCommand value)
		{
			CharGenSetSkinColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetSkinColorGameCommand value)
		{
			CharGenSetSkinColorGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetSkinColorGameCommand value)
		{
			CharGenSetSkinColorGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetSkinColorGameCommand value)
		{
			CharGenSetSkinColorGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_Index;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetSkinColorGameCommand(int m_index)
	{
		m_Index = m_index;
	}

	private CharGenSetSkinColorGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetSkinColor(m_Index);
		});
	}

	static CharGenSetSkinColorGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetSkinColorGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Index", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetSkinColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetSkinColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetSkinColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetSkinColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetSkinColorGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Index);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetSkinColorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int>(out value2);
			}
			else
			{
				value2 = value.m_Index;
				reader.ReadUnmanaged<int>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetSkinColorGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Index : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSetSkinColorGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetSkinColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Index");
		writer.WriteUnmanaged(value.m_Index);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetSkinColorGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v = ((value != null) ? value.m_Index : 0);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Index")
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Index")
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		_ = value;
		value = new CharGenSetSkinColorGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetSkinColorGameCommand source = new CharGenSetSkinColorGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetSkinColorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetSkinColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_Index;
		formatter.UnmanagedField(0, "m_Index", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetSkinColorGameCommand>();
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
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
