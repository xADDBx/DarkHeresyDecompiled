using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.UnitLogic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharacterDollEntry : IMemoryPackable<CharacterDollEntry>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<CharacterDollEntry>
{
	[Preserve]
	private sealed class CharacterDollEntryFormatter : MemoryPackFormatter<CharacterDollEntry>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterDollEntry value)
		{
			CharacterDollEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterDollEntry value)
		{
			CharacterDollEntry.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharacterDollEntry value)
		{
			CharacterDollEntry.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterDollEntry value)
		{
			CharacterDollEntry.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly Gender Gender;

	[CanBeNull]
	[JsonProperty]
	[MemoryPackInclude]
	public readonly BlueprintRaceVisualPresetReference RacePreset;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly List<string> EquipmentEntityIds;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly Dictionary<string, int> EntityRampIdices;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly Dictionary<string, int> EntitySecondaryRampIdices;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly bool LeftHanded;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly int ClothesPrimaryIndex;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly int ClothesSecondaryIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public CharacterDollEntry([NotNull] DollData data)
	{
		Gender = data.Gender;
		RacePreset = data.RacePreset?.ToReference<BlueprintRaceVisualPresetReference>();
		EquipmentEntityIds = new List<string>(data.EquipmentEntityIds);
		EntityRampIdices = new Dictionary<string, int>(data.EntityRampIdices);
		EntitySecondaryRampIdices = new Dictionary<string, int>(data.EntitySecondaryRampIdices);
		LeftHanded = data.LeftHanded;
		ClothesPrimaryIndex = data.ClothesPrimaryIndex;
		ClothesSecondaryIndex = data.ClothesSecondaryIndex;
	}

	[MemoryPackConstructor]
	public CharacterDollEntry(Gender gender, BlueprintRaceVisualPresetReference racePreset, List<string> equipmentEntityIds, Dictionary<string, int> entityRampIdices, Dictionary<string, int> entitySecondaryRampIdices, bool leftHanded, int clothesPrimaryIndex, int clothesSecondaryIndex)
	{
		Gender = gender;
		RacePreset = racePreset;
		EquipmentEntityIds = equipmentEntityIds;
		EntityRampIdices = entityRampIdices;
		EntitySecondaryRampIdices = entitySecondaryRampIdices;
		LeftHanded = leftHanded;
		ClothesPrimaryIndex = clothesPrimaryIndex;
		ClothesSecondaryIndex = clothesSecondaryIndex;
	}

	public CharacterDollEntry(OwlPackConstructorParameter _)
	{
	}

	[NotNull]
	public DollData ToDollData()
	{
		return new DollData
		{
			Gender = Gender,
			RacePreset = RacePreset,
			EquipmentEntityIds = new List<string>(EquipmentEntityIds ?? new List<string>()),
			EntityRampIdices = new Dictionary<string, int>(EntityRampIdices ?? new Dictionary<string, int>()),
			EntitySecondaryRampIdices = new Dictionary<string, int>(EntitySecondaryRampIdices ?? new Dictionary<string, int>()),
			LeftHanded = LeftHanded,
			ClothesPrimaryIndex = ClothesPrimaryIndex,
			ClothesSecondaryIndex = ClothesSecondaryIndex
		};
	}

	static CharacterDollEntry()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharacterDollEntry",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterDollEntry>())
		{
			MemoryPackFormatterProvider.Register(new CharacterDollEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterDollEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharacterDollEntry>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Gender>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Gender>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<string>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, int>>())
		{
			MemoryPackFormatterProvider.Register(new DictionaryFormatter<string, int>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterDollEntry? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(8, in value.Gender);
		writer.WritePackable(in value.RacePreset);
		writer.WriteValue(in value.EquipmentEntityIds);
		writer.WriteValue(in value.EntityRampIdices);
		writer.WriteValue(in value.EntitySecondaryRampIdices);
		writer.WriteUnmanaged(in value.LeftHanded, in value.ClothesPrimaryIndex, in value.ClothesSecondaryIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharacterDollEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Gender value2;
		BlueprintRaceVisualPresetReference value3;
		List<string> value4;
		Dictionary<string, int> value5;
		Dictionary<string, int> value6;
		bool value7;
		int value8;
		int value9;
		if (memberCount == 8)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<Gender>(out value2);
				value3 = reader.ReadPackable<BlueprintRaceVisualPresetReference>();
				value4 = reader.ReadValue<List<string>>();
				value5 = reader.ReadValue<Dictionary<string, int>>();
				value6 = reader.ReadValue<Dictionary<string, int>>();
				reader.ReadUnmanaged<bool, int, int>(out value7, out value8, out value9);
			}
			else
			{
				value2 = value.Gender;
				value3 = value.RacePreset;
				value4 = value.EquipmentEntityIds;
				value5 = value.EntityRampIdices;
				value6 = value.EntitySecondaryRampIdices;
				value7 = value.LeftHanded;
				value8 = value.ClothesPrimaryIndex;
				value9 = value.ClothesSecondaryIndex;
				reader.ReadUnmanaged<Gender>(out value2);
				reader.ReadPackable(ref value3);
				reader.ReadValue(ref value4);
				reader.ReadValue(ref value5);
				reader.ReadValue(ref value6);
				reader.ReadUnmanaged<bool>(out value7);
				reader.ReadUnmanaged<int>(out value8);
				reader.ReadUnmanaged<int>(out value9);
			}
		}
		else
		{
			if (memberCount > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharacterDollEntry), 8, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = Gender.Male;
				value3 = null;
				value4 = null;
				value5 = null;
				value6 = null;
				value7 = false;
				value8 = 0;
				value9 = 0;
			}
			else
			{
				value2 = value.Gender;
				value3 = value.RacePreset;
				value4 = value.EquipmentEntityIds;
				value5 = value.EntityRampIdices;
				value6 = value.EntitySecondaryRampIdices;
				value7 = value.LeftHanded;
				value8 = value.ClothesPrimaryIndex;
				value9 = value.ClothesSecondaryIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Gender>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadValue(ref value4);
						if (memberCount != 3)
						{
							reader.ReadValue(ref value5);
							if (memberCount != 4)
							{
								reader.ReadValue(ref value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<bool>(out value7);
									if (memberCount != 6)
									{
										reader.ReadUnmanaged<int>(out value8);
										if (memberCount != 7)
										{
											reader.ReadUnmanaged<int>(out value9);
											_ = 8;
										}
									}
								}
							}
						}
					}
				}
			}
			_ = value;
		}
		value = new CharacterDollEntry(value2, value3, value4, value5, value6, value7, value8, value9);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterDollEntry? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Gender");
		writer.WriteUnmanaged(value.Gender);
		writer.WriteProperty("RacePreset");
		writer.WritePackable(value.RacePreset);
		writer.WriteProperty("EquipmentEntityIds");
		writer.WriteValue(value.EquipmentEntityIds);
		writer.WriteProperty("EntityRampIdices");
		writer.WriteValue(value.EntityRampIdices);
		writer.WriteProperty("EntitySecondaryRampIdices");
		writer.WriteValue(value.EntitySecondaryRampIdices);
		writer.WriteProperty("LeftHanded");
		writer.WriteUnmanaged(value.LeftHanded);
		writer.WriteProperty("ClothesPrimaryIndex");
		writer.WriteUnmanaged(value.ClothesPrimaryIndex);
		writer.WriteProperty("ClothesSecondaryIndex");
		writer.WriteUnmanaged(value.ClothesSecondaryIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterDollEntry? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		Gender v;
		BlueprintRaceVisualPresetReference val;
		List<string> val2;
		Dictionary<string, int> val3;
		Dictionary<string, int> val4;
		bool v2;
		int v3;
		int v4;
		if (value == null)
		{
			v = Gender.Male;
			val = null;
			val2 = null;
			val3 = null;
			val4 = null;
			v2 = false;
			v3 = 0;
			v4 = 0;
		}
		else
		{
			v = value.Gender;
			val = value.RacePreset;
			val2 = value.EquipmentEntityIds;
			val3 = value.EntityRampIdices;
			val4 = value.EntitySecondaryRampIdices;
			v2 = value.LeftHanded;
			v3 = value.ClothesPrimaryIndex;
			v4 = value.ClothesSecondaryIndex;
		}
		bool[] array = new bool[8];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "Gender":
					reader.ReadUnmanaged<Gender>(out v);
					array[0] = true;
					break;
				case "RacePreset":
					val = reader.ReadPackable<BlueprintRaceVisualPresetReference>();
					array[1] = true;
					break;
				case "EquipmentEntityIds":
					val2 = reader.ReadValue<List<string>>();
					array[2] = true;
					break;
				case "EntityRampIdices":
					val3 = reader.ReadValue<Dictionary<string, int>>();
					array[3] = true;
					break;
				case "EntitySecondaryRampIdices":
					val4 = reader.ReadValue<Dictionary<string, int>>();
					array[4] = true;
					break;
				case "LeftHanded":
					reader.ReadUnmanaged<bool>(out v2);
					array[5] = true;
					break;
				case "ClothesPrimaryIndex":
					reader.ReadUnmanaged<int>(out v3);
					array[6] = true;
					break;
				case "ClothesSecondaryIndex":
					reader.ReadUnmanaged<int>(out v4);
					array[7] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "Gender":
					reader.ReadUnmanaged<Gender>(out v);
					break;
				case "RacePreset":
					reader.ReadPackable(ref val);
					break;
				case "EquipmentEntityIds":
					reader.ReadValue(ref val2);
					break;
				case "EntityRampIdices":
					reader.ReadValue(ref val3);
					break;
				case "EntitySecondaryRampIdices":
					reader.ReadValue(ref val4);
					break;
				case "LeftHanded":
					reader.ReadUnmanaged<bool>(out v2);
					break;
				case "ClothesPrimaryIndex":
					reader.ReadUnmanaged<int>(out v3);
					break;
				case "ClothesSecondaryIndex":
					reader.ReadUnmanaged<int>(out v4);
					break;
				}
			}
		}
		_ = value;
		value = new CharacterDollEntry(v, val, val2, val3, val4, v2, v3, v4);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharacterDollEntry source = new CharacterDollEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharacterDollEntry, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CharacterDollEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharacterDollEntry>();
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
