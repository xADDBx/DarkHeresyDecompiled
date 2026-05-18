using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenChangeAppearancePageGameCommand : GameCommand, IMemoryPackable<CharGenChangeAppearancePageGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenChangeAppearancePageGameCommand>
{
	[Preserve]
	private sealed class CharGenChangeAppearancePageGameCommandFormatter : MemoryPackFormatter<CharGenChangeAppearancePageGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenChangeAppearancePageGameCommand value)
		{
			CharGenChangeAppearancePageGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenChangeAppearancePageGameCommand value)
		{
			CharGenChangeAppearancePageGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenChangeAppearancePageGameCommand value)
		{
			CharGenChangeAppearancePageGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenChangeAppearancePageGameCommand value)
		{
			CharGenChangeAppearancePageGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly CharGenAppearancePageType m_PageType;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenChangeAppearancePageGameCommand(CharGenAppearancePageType m_pageType)
	{
		m_PageType = m_pageType;
	}

	private CharGenChangeAppearancePageGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhaseHandler h)
		{
			h.HandleAppearancePageChange(m_PageType);
		});
	}

	static CharGenChangeAppearancePageGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenChangeAppearancePageGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_PageType", typeof(CharGenAppearancePageType))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeAppearancePageGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenChangeAppearancePageGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeAppearancePageGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenChangeAppearancePageGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenAppearancePageType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CharGenAppearancePageType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenChangeAppearancePageGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_PageType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenChangeAppearancePageGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CharGenAppearancePageType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CharGenAppearancePageType>(out value2);
			}
			else
			{
				value2 = value.m_PageType;
				reader.ReadUnmanaged<CharGenAppearancePageType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenChangeAppearancePageGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_PageType : CharGenAppearancePageType.General);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CharGenAppearancePageType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenChangeAppearancePageGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenChangeAppearancePageGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_PageType");
		writer.WriteUnmanaged(value.m_PageType);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenChangeAppearancePageGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		CharGenAppearancePageType v = ((value != null) ? value.m_PageType : CharGenAppearancePageType.General);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_PageType")
				{
					reader.ReadUnmanaged<CharGenAppearancePageType>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_PageType")
			{
				reader.ReadUnmanaged<CharGenAppearancePageType>(out v);
			}
		}
		_ = value;
		value = new CharGenChangeAppearancePageGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenChangeAppearancePageGameCommand source = new CharGenChangeAppearancePageGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenChangeAppearancePageGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenChangeAppearancePageGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CharGenAppearancePageType value = m_PageType;
		formatter.EnumField(0, "m_PageType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenChangeAppearancePageGameCommand>();
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
				Unsafe.AsRef(in m_PageType) = formatter.ReadEnum<CharGenAppearancePageType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
