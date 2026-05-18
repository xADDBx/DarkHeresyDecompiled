using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSelectItemGameCommand : GameCommand, IMemoryPackable<CharGenSelectItemGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSelectItemGameCommand>
{
	[Preserve]
	private sealed class CharGenSelectItemGameCommandFormatter : MemoryPackFormatter<CharGenSelectItemGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSelectItemGameCommand value)
		{
			CharGenSelectItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSelectItemGameCommand value)
		{
			CharGenSelectItemGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSelectItemGameCommand value)
		{
			CharGenSelectItemGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSelectItemGameCommand value)
		{
			CharGenSelectItemGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly FeatureGroup m_FeatureGroup;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintFeatureReference m_BlueprintFeature;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSelectItemGameCommand(FeatureGroup m_featureGroup, [NotNull] BlueprintFeatureReference m_blueprintFeature)
	{
		if (m_blueprintFeature == null)
		{
			throw new ArgumentNullException("m_blueprintFeature");
		}
		m_FeatureGroup = m_featureGroup;
		m_BlueprintFeature = m_blueprintFeature;
	}

	private CharGenSelectItemGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSelectItemGameCommand(FeatureGroup featureGroup, [NotNull] BlueprintFeature feature)
		: this(featureGroup, feature.ToReference<BlueprintFeatureReference>())
	{
		if (feature == null)
		{
			throw new ArgumentNullException("feature");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintFeature blueprintFeature = m_BlueprintFeature.Get();
		if (blueprintFeature == null)
		{
			PFLog.GameCommands.Error("[CharGenSelectItemGameCommand] BlueprintFeature not found #" + m_BlueprintFeature.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenSelectItemHandler h)
		{
			h.HandleSelectItem(m_FeatureGroup, blueprintFeature);
		});
	}

	static CharGenSelectItemGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSelectItemGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_FeatureGroup", typeof(FeatureGroup)),
				new FieldInfo("m_BlueprintFeature", typeof(BlueprintFeatureReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSelectItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSelectItemGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FeatureGroup>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<FeatureGroup>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSelectItemGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(2, in value.m_FeatureGroup);
		writer.WritePackable(in value.m_BlueprintFeature);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSelectItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		FeatureGroup value2;
		BlueprintFeatureReference value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<FeatureGroup>(out value2);
				value3 = reader.ReadPackable<BlueprintFeatureReference>();
			}
			else
			{
				value2 = value.m_FeatureGroup;
				value3 = value.m_BlueprintFeature;
				reader.ReadUnmanaged<FeatureGroup>(out value2);
				reader.ReadPackable(ref value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSelectItemGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = FeatureGroup.None;
				value3 = null;
			}
			else
			{
				value2 = value.m_FeatureGroup;
				value3 = value.m_BlueprintFeature;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<FeatureGroup>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSelectItemGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSelectItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_FeatureGroup");
		writer.WriteUnmanaged(value.m_FeatureGroup);
		writer.WriteProperty("m_BlueprintFeature");
		writer.WritePackable(value.m_BlueprintFeature);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSelectItemGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		FeatureGroup v;
		BlueprintFeatureReference val;
		if (value == null)
		{
			v = FeatureGroup.None;
			val = null;
		}
		else
		{
			v = value.m_FeatureGroup;
			val = value.m_BlueprintFeature;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_FeatureGroup"))
				{
					if (text == "m_BlueprintFeature")
					{
						val = reader.ReadPackable<BlueprintFeatureReference>();
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<FeatureGroup>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_FeatureGroup"))
			{
				if (text == "m_BlueprintFeature")
				{
					reader.ReadPackable(ref val);
				}
			}
			else
			{
				reader.ReadUnmanaged<FeatureGroup>(out v);
			}
		}
		_ = value;
		value = new CharGenSelectItemGameCommand(v, val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSelectItemGameCommand source = new CharGenSelectItemGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSelectItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSelectItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		FeatureGroup value = m_FeatureGroup;
		formatter.EnumField(0, "m_FeatureGroup", ref value, state);
		BlueprintFeatureReference value2 = m_BlueprintFeature;
		formatter.Field(1, "m_BlueprintFeature", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSelectItemGameCommand>();
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
				Unsafe.AsRef(in m_FeatureGroup) = formatter.ReadEnum<FeatureGroup>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_BlueprintFeature) = formatter.ReadPackable<BlueprintFeatureReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
