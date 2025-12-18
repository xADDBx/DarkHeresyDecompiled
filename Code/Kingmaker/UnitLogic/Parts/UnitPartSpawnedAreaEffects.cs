using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartSpawnedAreaEffects : BaseUnitPart, IHashable, IOwlPackable<UnitPartSpawnedAreaEffects>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Entry : IHashable, IOwlPackable, IOwlPackable<Entry>
	{
		[JsonProperty]
		[OwlPackInclude]
		public EntityFactSource Source;

		[JsonProperty]
		[OwlPackInclude]
		public EntityRef<AreaEffectEntity> EntityRef;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Entry",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Source", typeof(EntityFactSource)),
				new FieldInfo("EntityRef", typeof(EntityRef<AreaEffectEntity>))
			}
		};

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = ClassHasher<EntityFactSource>.GetHash128(Source);
			result.Append(ref val);
			EntityRef<AreaEffectEntity> obj = EntityRef;
			Hash128 val2 = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Entry source = new Entry();
			result = Unsafe.As<Entry, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Entry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Source", ref Source, state);
			formatter.Field(1, "EntityRef", ref EntityRef, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Entry>();
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
					Source = formatter.ReadPackable<EntityFactSource>(state);
					break;
				case 1:
					EntityRef = formatter.ReadPackable<EntityRef<AreaEffectEntity>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartSpawnedAreaEffects",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Entries", typeof(List<Entry>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<Entry> m_Entries { get; set; } = new List<Entry>();


	public void Add(EntityFact sourceFact, BlueprintComponent sourceComponent, AreaEffectEntity areaEffect)
	{
		Entry entry = m_Entries.FirstItem((Entry i) => i.Source.IsFrom(sourceFact, sourceComponent));
		if (entry != null)
		{
			PFLog.Default.ErrorWithReport($"AreaEffect from {sourceFact}.{sourceComponent} already exists");
			m_Entries.Remove(entry);
		}
		entry = new Entry
		{
			Source = new EntityFactSource(sourceFact, sourceComponent),
			EntityRef = areaEffect
		};
		m_Entries.Add(entry);
	}

	public void RemoveAndEnd(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		Entry entry = m_Entries.FirstItem((Entry i) => i.Source.IsFrom(sourceFact, sourceComponent));
		entry?.EntityRef.Entity?.ForceEnd();
		m_Entries.Remove(entry);
		if (m_Entries.Empty())
		{
			RemoveSelf();
		}
	}

	public AreaEffectEntity Get(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		return m_Entries.FirstItem((Entry i) => i.Source.IsFrom(sourceFact, sourceComponent))?.EntityRef.Entity;
	}

	public AreaEffectEntity Get(BlueprintAreaEffect blueprint)
	{
		return m_Entries.FirstItem((Entry i) => i.EntityRef.Entity?.Blueprint == blueprint)?.EntityRef.Entity;
	}

	public bool Contains(BlueprintAreaEffect areaEffect)
	{
		return m_Entries.Any((Entry x) => x.EntityRef.Entity?.Blueprint == areaEffect);
	}

	public bool Contains(AreaEffectEntity areaEffect)
	{
		return m_Entries.Any((Entry x) => x.EntityRef.Entity == areaEffect);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Entry> entries = m_Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<Entry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartSpawnedAreaEffects source = new UnitPartSpawnedAreaEffects();
		result = Unsafe.As<UnitPartSpawnedAreaEffects, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartSpawnedAreaEffects>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<Entry> value = m_Entries;
		formatter.Field(0, "m_Entries", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartSpawnedAreaEffects>();
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
				m_Entries = formatter.ReadPackable<List<Entry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
