using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[OwlPackable(OwlPackableMode.Generate)]
public class CompanionStoriesManager : IHashable, IOwlPackable, IOwlPackable<CompanionStoriesManager>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CompanionStoriesManager",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Stories", typeof(Dictionary<BlueprintUnit, List<BlueprintCompanionStory>>))
		}
	};

	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintUnit, BlueprintCompanionStory>))]
	[OwlPackInclude]
	private Dictionary<BlueprintUnit, List<BlueprintCompanionStory>> m_Stories { get; set; } = new Dictionary<BlueprintUnit, List<BlueprintCompanionStory>>();


	public IEnumerable<BlueprintCompanionStory> Get(BaseUnitEntity character)
	{
		if (!ConfigRoot.Instance.CharGenRoot.CustomCompanions.Any((BlueprintUnitReference r) => r.Is(character.Blueprint)))
		{
			return Get(character.Blueprint);
		}
		return from r in ConfigRoot.Instance.CharGenRoot.CustomCompanionStories
			select r.Get() into st
			where st.Gender == character.Gender && !st.IsDlcRestricted()
			select st;
	}

	private IEnumerable<BlueprintCompanionStory> Get(BlueprintUnit character)
	{
		m_Stories.TryGetValue(character, out var value);
		return value.EmptyIfNull();
	}

	public bool IsUnlocked(BlueprintCompanionStory companionStory)
	{
		foreach (KeyValuePair<BlueprintUnit, List<BlueprintCompanionStory>> story in m_Stories)
		{
			if (story.Value.Contains(companionStory))
			{
				return true;
			}
		}
		return false;
	}

	public void Unlock(BlueprintCompanionStory story)
	{
		if (!m_Stories.TryGetValue(story.Companion, out var value))
		{
			value = new List<BlueprintCompanionStory>();
			m_Stories.Add(story.Companion, value);
		}
		else if (value.Contains(story))
		{
			return;
		}
		value.Add(story);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintUnit, BlueprintCompanionStory>.GetHash128(m_Stories);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CompanionStoriesManager source = new CompanionStoriesManager();
		result = Unsafe.As<CompanionStoriesManager, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CompanionStoriesManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<BlueprintUnit, List<BlueprintCompanionStory>> value = m_Stories;
		formatter.Field(0, "m_Stories", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CompanionStoriesManager>();
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
				m_Stories = formatter.ReadPackable<Dictionary<BlueprintUnit, List<BlueprintCompanionStory>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
