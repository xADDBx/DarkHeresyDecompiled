using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class LoadAreaGameCommand : GameCommand, IMemoryPackable<LoadAreaGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<LoadAreaGameCommand>
{
	[Preserve]
	private sealed class LoadAreaGameCommandFormatter : MemoryPackFormatter<LoadAreaGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref LoadAreaGameCommand value)
		{
			LoadAreaGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref LoadAreaGameCommand value)
		{
			LoadAreaGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LoadAreaGameCommand value)
		{
			LoadAreaGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref LoadAreaGameCommand value)
		{
			LoadAreaGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintAreaEnterPointReference m_EnterPoint;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly AutoSaveMode m_AutoSaveMode;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public LoadAreaGameCommand([NotNull] BlueprintAreaEnterPointReference m_enterPoint, AutoSaveMode m_autoSaveMode)
	{
		m_EnterPoint = m_enterPoint;
		m_AutoSaveMode = m_autoSaveMode;
		if (m_EnterPoint == null)
		{
			throw new ArgumentNullException("m_enterPoint");
		}
		if ((BlueprintAreaEnterPoint)m_EnterPoint == null)
		{
			throw new NullReferenceException("EnterPoint was not found! " + m_EnterPoint.Guid);
		}
	}

	private LoadAreaGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		BlueprintAreaEnterPoint blueprintAreaEnterPoint = m_EnterPoint;
		if (blueprintAreaEnterPoint == null)
		{
			PFLog.GameCommands.Log("[LoadAreaGameCommand] EnterPoint was not found! " + m_EnterPoint.Guid);
		}
		else
		{
			Game.Instance.LoadArea(blueprintAreaEnterPoint, m_AutoSaveMode);
		}
	}

	static LoadAreaGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "LoadAreaGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_EnterPoint", typeof(BlueprintAreaEnterPointReference)),
				new FieldInfo("m_AutoSaveMode", typeof(AutoSaveMode))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LoadAreaGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new LoadAreaGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LoadAreaGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<LoadAreaGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AutoSaveMode>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<AutoSaveMode>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref LoadAreaGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_EnterPoint);
		writer.WriteUnmanaged(in value.m_AutoSaveMode);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LoadAreaGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintAreaEnterPointReference value2;
		AutoSaveMode value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintAreaEnterPointReference>();
				reader.ReadUnmanaged<AutoSaveMode>(out value3);
			}
			else
			{
				value2 = value.m_EnterPoint;
				value3 = value.m_AutoSaveMode;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<AutoSaveMode>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LoadAreaGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = AutoSaveMode.None;
			}
			else
			{
				value2 = value.m_EnterPoint;
				value3 = value.m_AutoSaveMode;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<AutoSaveMode>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new LoadAreaGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref LoadAreaGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_EnterPoint");
		writer.WritePackable(value.m_EnterPoint);
		writer.WriteProperty("m_AutoSaveMode");
		writer.WriteUnmanaged(value.m_AutoSaveMode);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref LoadAreaGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintAreaEnterPointReference val;
		AutoSaveMode v;
		if (value == null)
		{
			val = null;
			v = AutoSaveMode.None;
		}
		else
		{
			val = value.m_EnterPoint;
			v = value.m_AutoSaveMode;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_EnterPoint"))
				{
					if (text == "m_AutoSaveMode")
					{
						reader.ReadUnmanaged<AutoSaveMode>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<BlueprintAreaEnterPointReference>();
					array[0] = true;
				}
			}
			else if (!(text == "m_EnterPoint"))
			{
				if (text == "m_AutoSaveMode")
				{
					reader.ReadUnmanaged<AutoSaveMode>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new LoadAreaGameCommand(val, v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		LoadAreaGameCommand source = new LoadAreaGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<LoadAreaGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LoadAreaGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintAreaEnterPointReference value = m_EnterPoint;
		formatter.Field(0, "m_EnterPoint", ref value, state);
		AutoSaveMode value2 = m_AutoSaveMode;
		formatter.EnumField(1, "m_AutoSaveMode", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LoadAreaGameCommand>();
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
				Unsafe.AsRef(in m_EnterPoint) = formatter.ReadPackable<BlueprintAreaEnterPointReference>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_AutoSaveMode) = formatter.ReadEnum<AutoSaveMode>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
