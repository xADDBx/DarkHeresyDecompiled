using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Paths;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSelectCareerPathGameCommand : GameCommand, IMemoryPackable<CharGenSelectCareerPathGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSelectCareerPathGameCommand>
{
	[Preserve]
	private sealed class CharGenSelectCareerPathGameCommandFormatter : MemoryPackFormatter<CharGenSelectCareerPathGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSelectCareerPathGameCommand value)
		{
			CharGenSelectCareerPathGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSelectCareerPathGameCommand value)
		{
			CharGenSelectCareerPathGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSelectCareerPathGameCommand value)
		{
			CharGenSelectCareerPathGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSelectCareerPathGameCommand value)
		{
			CharGenSelectCareerPathGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintCareerPathReference m_CareerPathRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSelectCareerPathGameCommand([NotNull] BlueprintCareerPathReference m_careerPathRef)
	{
		m_CareerPathRef = m_careerPathRef;
	}

	private CharGenSelectCareerPathGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSelectCareerPathGameCommand([NotNull] BlueprintCareerPath careerPath)
		: this(careerPath.ToReference<BlueprintCareerPathReference>())
	{
		if (careerPath == null)
		{
			throw new ArgumentNullException("careerPath");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintCareerPath blueprintCareerPath = m_CareerPathRef.Get();
		if (blueprintCareerPath == null)
		{
			PFLog.GameCommands.Error("[CharGenSelectCareerPathGameCommand] BlueprintCareerPath not found #" + m_CareerPathRef.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenCareerPathHandler h)
		{
			h.HandleCareerPath(blueprintCareerPath);
		});
	}

	static CharGenSelectCareerPathGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSelectCareerPathGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_CareerPathRef", typeof(BlueprintCareerPathReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectCareerPathGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSelectCareerPathGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectCareerPathGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSelectCareerPathGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSelectCareerPathGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_CareerPathRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSelectCareerPathGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintCareerPathReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintCareerPathReference>();
			}
			else
			{
				value2 = value.m_CareerPathRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSelectCareerPathGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_CareerPathRef : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSelectCareerPathGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSelectCareerPathGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_CareerPathRef");
		writer.WritePackable(value.m_CareerPathRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSelectCareerPathGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintCareerPathReference val = ((value != null) ? value.m_CareerPathRef : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_CareerPathRef")
				{
					val = reader.ReadPackable<BlueprintCareerPathReference>();
					array[0] = true;
				}
			}
			else if (text == "m_CareerPathRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharGenSelectCareerPathGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSelectCareerPathGameCommand source = new CharGenSelectCareerPathGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSelectCareerPathGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSelectCareerPathGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintCareerPathReference value = m_CareerPathRef;
		formatter.Field(0, "m_CareerPathRef", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSelectCareerPathGameCommand>();
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
				Unsafe.AsRef(in m_CareerPathRef) = formatter.ReadPackable<BlueprintCareerPathReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
