using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class SettingGameCommand : GameCommand, IMemoryPackable<SettingGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SettingGameCommand>
{
	[Preserve]
	private sealed class SettingGameCommandFormatter : MemoryPackFormatter<SettingGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SettingGameCommand value)
		{
			SettingGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SettingGameCommand value)
		{
			SettingGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SettingGameCommand value)
		{
			SettingGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SettingGameCommand value)
		{
			SettingGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly List<BaseSettingNetData> m_Settings;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SettingGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SettingGameCommand(List<BaseSettingNetData> m_settings)
	{
		m_Settings = m_settings;
	}

	protected override void ExecuteInternal()
	{
		SettingsController.Instance.RevertAllTempValues();
		foreach (BaseSettingNetData setting in m_Settings)
		{
			setting.ForceSet();
		}
		Game.Instance.UISettingsManager.OnSettingsApplied();
		EventBus.RaiseEvent(delegate(ISaveSettingsHandler h)
		{
			h.HandleSaveSettings();
		});
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
	}

	static SettingGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SettingGameCommand",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SettingGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SettingGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SettingGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SettingGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<BaseSettingNetData>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<BaseSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SettingGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteValue(in value.m_Settings);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SettingGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<BaseSettingNetData> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadValue<List<BaseSettingNetData>>();
			}
			else
			{
				value2 = value.m_Settings;
				reader.ReadValue(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SettingGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Settings : null);
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SettingGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SettingGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Settings");
		writer.WriteValue(value.m_Settings);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SettingGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		List<BaseSettingNetData> val = ((value != null) ? value.m_Settings : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Settings")
				{
					val = reader.ReadValue<List<BaseSettingNetData>>();
					array[0] = true;
				}
			}
			else if (text == "m_Settings")
			{
				reader.ReadValue(ref val);
			}
		}
		_ = value;
		value = new SettingGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SettingGameCommand source = new SettingGameCommand();
		result = Unsafe.As<SettingGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SettingGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SettingGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
