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
public sealed class CharGenShowClothGameCommand : GameCommand, IMemoryPackable<CharGenShowClothGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenShowClothGameCommand>
{
	[Preserve]
	private sealed class CharGenShowClothGameCommandFormatter : MemoryPackFormatter<CharGenShowClothGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenShowClothGameCommand value)
		{
			CharGenShowClothGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenShowClothGameCommand value)
		{
			CharGenShowClothGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenShowClothGameCommand value)
		{
			CharGenShowClothGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenShowClothGameCommand value)
		{
			CharGenShowClothGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly bool m_ShowCloth;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenShowClothGameCommand(bool m_showCloth)
	{
		m_ShowCloth = m_showCloth;
	}

	private CharGenShowClothGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleShowCloth(m_ShowCloth);
		});
		EventBus.RaiseEvent(delegate(ICharGenVisualHandler h)
		{
			h.HandleShowCloth(m_ShowCloth);
		});
	}

	static CharGenShowClothGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenShowClothGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_ShowCloth", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenShowClothGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenShowClothGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenShowClothGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenShowClothGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenShowClothGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_ShowCloth);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenShowClothGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool>(out value2);
			}
			else
			{
				value2 = value.m_ShowCloth;
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenShowClothGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value.m_ShowCloth;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenShowClothGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenShowClothGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_ShowCloth");
		writer.WriteUnmanaged(value.m_ShowCloth);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenShowClothGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		bool v = value != null && value.m_ShowCloth;
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_ShowCloth")
				{
					reader.ReadUnmanaged<bool>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_ShowCloth")
			{
				reader.ReadUnmanaged<bool>(out v);
			}
		}
		_ = value;
		value = new CharGenShowClothGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenShowClothGameCommand source = new CharGenShowClothGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenShowClothGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenShowClothGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = m_ShowCloth;
		formatter.UnmanagedField(0, "m_ShowCloth", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenShowClothGameCommand>();
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
				Unsafe.AsRef(in m_ShowCloth) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
