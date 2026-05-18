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
public sealed class CharGenSwitchPortraitTabGameCommand : GameCommand, IMemoryPackable<CharGenSwitchPortraitTabGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSwitchPortraitTabGameCommand>
{
	[Preserve]
	private sealed class CharGenSwitchPortraitTabGameCommandFormatter : MemoryPackFormatter<CharGenSwitchPortraitTabGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSwitchPortraitTabGameCommand value)
		{
			CharGenSwitchPortraitTabGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSwitchPortraitTabGameCommand value)
		{
			CharGenSwitchPortraitTabGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSwitchPortraitTabGameCommand value)
		{
			CharGenSwitchPortraitTabGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSwitchPortraitTabGameCommand value)
		{
			CharGenSwitchPortraitTabGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly CharGenPortraitTab m_Tab;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSwitchPortraitTabGameCommand(CharGenPortraitTab m_tab)
	{
		m_Tab = m_tab;
	}

	private CharGenSwitchPortraitTabGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhasePortraitHandler h)
		{
			h.HandlePortraitTabChange(m_Tab);
		});
	}

	static CharGenSwitchPortraitTabGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSwitchPortraitTabGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Tab", typeof(CharGenPortraitTab))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSwitchPortraitTabGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSwitchPortraitTabGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSwitchPortraitTabGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSwitchPortraitTabGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenPortraitTab>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CharGenPortraitTab>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSwitchPortraitTabGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Tab);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSwitchPortraitTabGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CharGenPortraitTab value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CharGenPortraitTab>(out value2);
			}
			else
			{
				value2 = value.m_Tab;
				reader.ReadUnmanaged<CharGenPortraitTab>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSwitchPortraitTabGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Tab : CharGenPortraitTab.Default);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CharGenPortraitTab>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSwitchPortraitTabGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSwitchPortraitTabGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Tab");
		writer.WriteUnmanaged(value.m_Tab);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSwitchPortraitTabGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		CharGenPortraitTab v = ((value != null) ? value.m_Tab : CharGenPortraitTab.Default);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Tab")
				{
					reader.ReadUnmanaged<CharGenPortraitTab>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Tab")
			{
				reader.ReadUnmanaged<CharGenPortraitTab>(out v);
			}
		}
		_ = value;
		value = new CharGenSwitchPortraitTabGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSwitchPortraitTabGameCommand source = new CharGenSwitchPortraitTabGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSwitchPortraitTabGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSwitchPortraitTabGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CharGenPortraitTab value = m_Tab;
		formatter.EnumField(0, "m_Tab", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSwitchPortraitTabGameCommand>();
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
				Unsafe.AsRef(in m_Tab) = formatter.ReadEnum<CharGenPortraitTab>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
