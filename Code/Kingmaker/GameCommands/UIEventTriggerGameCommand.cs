using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class UIEventTriggerGameCommand : GameCommand, IMemoryPackable<UIEventTriggerGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<UIEventTriggerGameCommand>
{
	[Preserve]
	private sealed class UIEventTriggerGameCommandFormatter : MemoryPackFormatter<UIEventTriggerGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UIEventTriggerGameCommand value)
		{
			UIEventTriggerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref UIEventTriggerGameCommand value)
		{
			UIEventTriggerGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UIEventTriggerGameCommand value)
		{
			UIEventTriggerGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref UIEventTriggerGameCommand value)
		{
			UIEventTriggerGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintComponentReference m_UIEventTrigger;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private UIEventTriggerGameCommand()
	{
	}

	[JsonConstructor]
	public UIEventTriggerGameCommand([NotNull] UIEventTrigger uiEventTrigger)
	{
		m_UIEventTrigger = uiEventTrigger;
	}

	protected override void ExecuteInternal()
	{
		if (m_UIEventTrigger.Get() is UIEventTrigger uIEventTrigger && uIEventTrigger.Conditions.Check())
		{
			uIEventTrigger.Actions.Run();
		}
	}

	static UIEventTriggerGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "UIEventTriggerGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_UIEventTrigger", typeof(BlueprintComponentReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UIEventTriggerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new UIEventTriggerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UIEventTriggerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UIEventTriggerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UIEventTriggerGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_UIEventTrigger);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UIEventTriggerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintComponentReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_UIEventTrigger;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<BlueprintComponentReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UIEventTriggerGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UIEventTrigger : default(BlueprintComponentReference));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new UIEventTriggerGameCommand
		{
			m_UIEventTrigger = value2
		};
		return;
		IL_0070:
		value.m_UIEventTrigger = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref UIEventTriggerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_UIEventTrigger");
		writer.WritePackable(value.m_UIEventTrigger);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref UIEventTriggerGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintComponentReference val = ((value != null) ? value.m_UIEventTrigger : default(BlueprintComponentReference));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_UIEventTrigger")
				{
					val = reader.ReadPackable<BlueprintComponentReference>();
					array[0] = true;
				}
			}
			else if (text == "m_UIEventTrigger")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_UIEventTrigger = val;
		}
		else
		{
			value = new UIEventTriggerGameCommand
			{
				m_UIEventTrigger = val
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UIEventTriggerGameCommand source = new UIEventTriggerGameCommand();
		result = Unsafe.As<UIEventTriggerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UIEventTriggerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UIEventTrigger", ref m_UIEventTrigger, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UIEventTriggerGameCommand>();
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
				m_UIEventTrigger = formatter.ReadPackable<BlueprintComponentReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
