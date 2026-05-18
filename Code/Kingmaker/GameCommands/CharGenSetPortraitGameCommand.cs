using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPortraitGameCommand : GameCommand, IMemoryPackable<CharGenSetPortraitGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetPortraitGameCommand>
{
	[Preserve]
	private sealed class CharGenSetPortraitGameCommandFormatter : MemoryPackFormatter<CharGenSetPortraitGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintPortraitReference m_Blueprint;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly Guid m_CustomPortraitGuid;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetPortraitGameCommand([NotNull] BlueprintPortraitReference m_blueprint, Guid m_customPortraitGuid)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
		m_CustomPortraitGuid = m_customPortraitGuid;
	}

	private CharGenSetPortraitGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSetPortraitGameCommand([NotNull] BlueprintPortrait blueprint)
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
		m_Blueprint = blueprint.ToReference<BlueprintPortraitReference>();
		PortraitData data = blueprint.Data;
		if (SavePacker.TryGetGuidFromPortrait(data, out m_CustomPortraitGuid))
		{
			PFLog.GameCommands.Log($"[CharGenSetPortraitGameCommand] [Create] CustomPortrait '{data.CustomId}' -> '{m_CustomPortraitGuid}'");
			if (PhotonManager.Initialized)
			{
				PhotonManager.Instance.PortraitSyncer.SetNewPortraitForSending(m_CustomPortraitGuid);
			}
		}
		else if (PhotonManager.Initialized)
		{
			PhotonManager.Instance.PortraitSyncer.ClearPortraitForSending();
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintPortrait blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenSetPortraitGameCommand] BlueprintPortrait was not found id=" + m_Blueprint.Guid);
			return;
		}
		if (m_CustomPortraitGuid != Guid.Empty)
		{
			string portraitId;
			bool flag = SavePacker.TryGetPortraitIdFromGuid(m_CustomPortraitGuid, out portraitId);
			PFLog.GameCommands.Log($"[CharGenSetPortraitGameCommand] [Execute] CustomPortrait '{m_CustomPortraitGuid}' -> '{portraitId}' found={flag}");
			if (PhotonManager.Initialized)
			{
				PhotonManager.Instance.PortraitSyncer.SetPortraitForReceiving(m_CustomPortraitGuid, blueprint, flag);
				if (!flag)
				{
					return;
				}
			}
			blueprint.Data = new PortraitData(portraitId);
		}
		EventBus.RaiseEvent(delegate(ICharGenPortraitHandler h)
		{
			h.HandleSetPortrait(blueprint);
		});
	}

	static CharGenSetPortraitGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetPortraitGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Blueprint", typeof(BlueprintPortraitReference)),
				new FieldInfo("m_CustomPortraitGuid", typeof(Guid))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortraitGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPortraitGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortraitGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPortraitGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetPortraitGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Blueprint);
		writer.WriteUnmanaged(in value.m_CustomPortraitGuid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortraitGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPortraitReference value2;
		Guid value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintPortraitReference>();
				reader.ReadUnmanaged<Guid>(out value3);
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_CustomPortraitGuid;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<Guid>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPortraitGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = default(Guid);
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_CustomPortraitGuid;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<Guid>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetPortraitGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetPortraitGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Blueprint");
		writer.WritePackable(value.m_Blueprint);
		writer.WriteProperty("m_CustomPortraitGuid");
		writer.WriteUnmanaged(value.m_CustomPortraitGuid);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetPortraitGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintPortraitReference val;
		Guid v;
		if (value == null)
		{
			val = null;
			v = default(Guid);
		}
		else
		{
			val = value.m_Blueprint;
			v = value.m_CustomPortraitGuid;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Blueprint"))
				{
					if (text == "m_CustomPortraitGuid")
					{
						reader.ReadUnmanaged<Guid>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<BlueprintPortraitReference>();
					array[0] = true;
				}
			}
			else if (!(text == "m_Blueprint"))
			{
				if (text == "m_CustomPortraitGuid")
				{
					reader.ReadUnmanaged<Guid>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharGenSetPortraitGameCommand(val, v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetPortraitGameCommand source = new CharGenSetPortraitGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetPortraitGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetPortraitGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintPortraitReference value = m_Blueprint;
		formatter.Field(0, "m_Blueprint", ref value, state);
		Guid value2 = m_CustomPortraitGuid;
		formatter.Field(1, "m_CustomPortraitGuid", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetPortraitGameCommand>();
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
				Unsafe.AsRef(in m_Blueprint) = formatter.ReadPackable<BlueprintPortraitReference>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_CustomPortraitGuid) = formatter.ReadPackable<Guid>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
