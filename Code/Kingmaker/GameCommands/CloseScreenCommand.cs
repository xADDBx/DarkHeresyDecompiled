using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CloseScreenCommand : GameCommandWithSynchronized, IMemoryPackable<CloseScreenCommand>, IMemoryPackFormatterRegister, IOwlPackable<CloseScreenCommand>
{
	[Preserve]
	private sealed class CloseScreenCommandFormatter : MemoryPackFormatter<CloseScreenCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CloseScreenCommand value)
		{
			CloseScreenCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CloseScreenCommand value)
		{
			CloseScreenCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CloseScreenCommand value)
		{
			CloseScreenCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CloseScreenCommand value)
		{
			CloseScreenCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly byte m_Screen;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	private CloseScreenCommand(byte m_screen)
	{
		m_Screen = m_screen;
	}

	private CloseScreenCommand(OwlPackConstructorParameter _)
	{
	}

	public CloseScreenCommand(IScreenUIHandler.ScreenType screen, bool isSynchronized)
		: this((byte)screen)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IScreenUIHandler h)
		{
			h.CloseScreen((IScreenUIHandler.ScreenType)m_Screen);
		});
	}

	static CloseScreenCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CloseScreenCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Screen", typeof(byte))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CloseScreenCommand>())
		{
			MemoryPackFormatterProvider.Register(new CloseScreenCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CloseScreenCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CloseScreenCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CloseScreenCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Screen);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CloseScreenCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<byte>(out value2);
			}
			else
			{
				value2 = value.m_Screen;
				reader.ReadUnmanaged<byte>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CloseScreenCommand), 1, memberCount);
				return;
			}
			value2 = (byte)((value != null) ? value.m_Screen : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CloseScreenCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CloseScreenCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Screen");
		writer.WriteUnmanaged(value.m_Screen);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CloseScreenCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		byte v = (byte)((value != null) ? value.m_Screen : 0);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Screen")
				{
					reader.ReadUnmanaged<byte>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Screen")
			{
				reader.ReadUnmanaged<byte>(out v);
			}
		}
		_ = value;
		value = new CloseScreenCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CloseScreenCommand source = new CloseScreenCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CloseScreenCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CloseScreenCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		byte value = m_Screen;
		formatter.UnmanagedField(0, "m_Screen", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CloseScreenCommand>();
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
				Unsafe.AsRef(in m_Screen) = formatter.ReadUnmanaged<byte>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
