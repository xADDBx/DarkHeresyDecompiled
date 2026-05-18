using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking.Serialization;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[Serializable]
[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class CharGenUnitSaveData : IHashable, IOwlPackable, IOwlPackable<CharGenUnitSaveData>
{
	public enum ChargenDropdownType
	{
		ModifierType,
		ModifierSource,
		TalentType,
		TalentSource
	}

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Dictionary<ChargenDropdownType, Dictionary<string, bool>> m_DropdownsStates = new Dictionary<ChargenDropdownType, Dictionary<string, bool>>();

	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ChargenDropdownType PreferredModifierSortingType;

	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ChargenDropdownType PreferredTalentSortingType = ChargenDropdownType.TalentType;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenUnitSaveData",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("m_DropdownsStates", typeof(Dictionary<ChargenDropdownType, Dictionary<string, bool>>)),
			new FieldInfo("FavoriteFeatures", typeof(HashSet<string>)),
			new FieldInfo("PreferredModifierSortingType", typeof(ChargenDropdownType)),
			new FieldInfo("PreferredTalentSortingType", typeof(ChargenDropdownType))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public HashSet<string> FavoriteFeatures { get; private set; } = new HashSet<string>();


	public bool? IsDropdownOpen(ChargenDropdownType type, string id)
	{
		if (!m_DropdownsStates.TryGetValue(type, out var value) || !value.TryGetValue(id, out var value2))
		{
			return null;
		}
		return value2;
	}

	public void SetDropDownList(ChargenDropdownType type, Dictionary<string, bool> dropdownsStates)
	{
		if (!m_DropdownsStates.TryGetValue(type, out var dropdowns))
		{
			dropdowns = new Dictionary<string, bool>();
			m_DropdownsStates[type] = dropdowns;
		}
		dropdownsStates.ForEach(delegate(KeyValuePair<string, bool> d)
		{
			dropdowns[d.Key] = d.Value;
		});
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenUnitSaveData source = new CharGenUnitSaveData();
		result = Unsafe.As<CharGenUnitSaveData, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CharGenUnitSaveData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_DropdownsStates", ref m_DropdownsStates, state);
		HashSet<string> value = FavoriteFeatures;
		formatter.Field(1, "FavoriteFeatures", ref value, state);
		formatter.EnumField(2, "PreferredModifierSortingType", ref PreferredModifierSortingType, state);
		formatter.EnumField(3, "PreferredTalentSortingType", ref PreferredTalentSortingType, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenUnitSaveData>();
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
				m_DropdownsStates = formatter.ReadPackable<Dictionary<ChargenDropdownType, Dictionary<string, bool>>>(state);
				break;
			case 1:
				FavoriteFeatures = formatter.ReadPackable<HashSet<string>>(state);
				break;
			case 2:
				PreferredModifierSortingType = formatter.ReadEnum<ChargenDropdownType>(state);
				break;
			case 3:
				PreferredTalentSortingType = formatter.ReadEnum<ChargenDropdownType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
