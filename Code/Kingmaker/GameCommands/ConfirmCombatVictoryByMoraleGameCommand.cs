using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Gameplay.Features.Encounter;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class ConfirmCombatVictoryByMoraleGameCommand : GameCommand, IMemoryPackable<ConfirmCombatVictoryByMoraleGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<ConfirmCombatVictoryByMoraleGameCommand>
{
	[Preserve]
	private sealed class ConfirmCombatVictoryByMoraleGameCommandFormatter : MemoryPackFormatter<ConfirmCombatVictoryByMoraleGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ConfirmCombatVictoryByMoraleGameCommand value)
		{
			ConfirmCombatVictoryByMoraleGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ConfirmCombatVictoryByMoraleGameCommand value)
		{
			ConfirmCombatVictoryByMoraleGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ConfirmCombatVictoryByMoraleGameCommand value)
		{
			ConfirmCombatVictoryByMoraleGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref ConfirmCombatVictoryByMoraleGameCommand value)
		{
			ConfirmCombatVictoryByMoraleGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool _confirmed;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public ConfirmCombatVictoryByMoraleGameCommand()
	{
	}

	[JsonConstructor]
	public ConfirmCombatVictoryByMoraleGameCommand(bool confirmed)
	{
		_confirmed = confirmed;
	}

	protected override void ExecuteInternal()
	{
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			current.SetVictoryByMoraleConfirmed(_confirmed);
		}
		else
		{
			PFLog.Default.Error("No active encounter to confirm victory by morale in it");
		}
	}

	static ConfirmCombatVictoryByMoraleGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "ConfirmCombatVictoryByMoraleGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("_confirmed", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ConfirmCombatVictoryByMoraleGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ConfirmCombatVictoryByMoraleGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ConfirmCombatVictoryByMoraleGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ConfirmCombatVictoryByMoraleGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ConfirmCombatVictoryByMoraleGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value._confirmed);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ConfirmCombatVictoryByMoraleGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value._confirmed;
				reader.ReadUnmanaged<bool>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<bool>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ConfirmCombatVictoryByMoraleGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value._confirmed;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new ConfirmCombatVictoryByMoraleGameCommand
		{
			_confirmed = value2
		};
		return;
		IL_006b:
		value._confirmed = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref ConfirmCombatVictoryByMoraleGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("_confirmed");
		writer.WriteUnmanaged(value._confirmed);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref ConfirmCombatVictoryByMoraleGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		bool v = value != null && value._confirmed;
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "_confirmed")
				{
					reader.ReadUnmanaged<bool>(out v);
					array[0] = true;
				}
			}
			else if (text == "_confirmed")
			{
				reader.ReadUnmanaged<bool>(out v);
			}
		}
		if (value != null)
		{
			value._confirmed = v;
		}
		else
		{
			value = new ConfirmCombatVictoryByMoraleGameCommand
			{
				_confirmed = v
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
		ConfirmCombatVictoryByMoraleGameCommand source = new ConfirmCombatVictoryByMoraleGameCommand();
		result = Unsafe.As<ConfirmCombatVictoryByMoraleGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConfirmCombatVictoryByMoraleGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "_confirmed", ref _confirmed, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConfirmCombatVictoryByMoraleGameCommand>();
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
				_confirmed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
