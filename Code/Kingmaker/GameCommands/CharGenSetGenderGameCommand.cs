using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Base;
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
public sealed class CharGenSetGenderGameCommand : GameCommand, IMemoryPackable<CharGenSetGenderGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetGenderGameCommand>
{
	[Preserve]
	private sealed class CharGenSetGenderGameCommandFormatter : MemoryPackFormatter<CharGenSetGenderGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetGenderGameCommand value)
		{
			CharGenSetGenderGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetGenderGameCommand value)
		{
			CharGenSetGenderGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetGenderGameCommand value)
		{
			CharGenSetGenderGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetGenderGameCommand value)
		{
			CharGenSetGenderGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly Gender m_Gender;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_Index;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetGenderGameCommand(Gender m_gender, int m_index)
	{
		m_Gender = m_gender;
		m_Index = m_index;
	}

	private CharGenSetGenderGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetGender(m_Gender, m_Index);
		});
	}

	static CharGenSetGenderGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetGenderGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Gender", typeof(Gender)),
				new FieldInfo("m_Index", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetGenderGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetGenderGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetGenderGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetGenderGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Gender>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Gender>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetGenderGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_Gender, in value.m_Index);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetGenderGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Gender value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<Gender, int>(out value2, out value3);
			}
			else
			{
				value2 = value.m_Gender;
				value3 = value.m_Index;
				reader.ReadUnmanaged<Gender>(out value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetGenderGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = Gender.Male;
				value3 = 0;
			}
			else
			{
				value2 = value.m_Gender;
				value3 = value.m_Index;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Gender>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetGenderGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetGenderGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Gender");
		writer.WriteUnmanaged(value.m_Gender);
		writer.WriteProperty("m_Index");
		writer.WriteUnmanaged(value.m_Index);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetGenderGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		Gender v;
		int v2;
		if (value == null)
		{
			v = Gender.Male;
			v2 = 0;
		}
		else
		{
			v = value.m_Gender;
			v2 = value.m_Index;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Gender"))
				{
					if (text == "m_Index")
					{
						reader.ReadUnmanaged<int>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<Gender>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_Gender"))
			{
				if (text == "m_Index")
				{
					reader.ReadUnmanaged<int>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<Gender>(out v);
			}
		}
		_ = value;
		value = new CharGenSetGenderGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetGenderGameCommand source = new CharGenSetGenderGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetGenderGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetGenderGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Gender value = m_Gender;
		formatter.EnumField(0, "m_Gender", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetGenderGameCommand>();
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
				Unsafe.AsRef(in m_Gender) = formatter.ReadEnum<Gender>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
