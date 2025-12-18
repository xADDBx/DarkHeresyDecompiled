using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[OwlPackable(OwlPackableMode.Generate)]
public class EntityFactSource : IHashable, IOwlPackable, IOwlPackable<EntityFactSource>
{
	public static readonly EntityFactSource Empty = new EntityFactSource();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityFactSource",
		OldNames = null,
		Fields = new FieldInfo[8]
		{
			new FieldInfo("m_EntityRef", typeof(EntityRef)),
			new FieldInfo("m_FactRef", typeof(EntityFactRef)),
			new FieldInfo("m_ComponentId", typeof(string)),
			new FieldInfo("m_UnitCondition", typeof(UnitCondition?)),
			new FieldInfo("m_Blueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("m_PathFeatureSource", typeof(BlueprintScriptableObject)),
			new FieldInfo("m_PathRank", typeof(int?)),
			new FieldInfo("m_FeatureRank", typeof(int?))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private EntityRef m_EntityRef { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private EntityFactRef m_FactRef { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private string m_ComponentId { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private UnitCondition? m_UnitCondition { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private BlueprintScriptableObject m_Blueprint { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private BlueprintScriptableObject m_PathFeatureSource { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private int? m_PathRank { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private int? m_FeatureRank { get; set; }

	[CanBeNull]
	public EntityFact Fact => m_FactRef.Fact;

	[CanBeNull]
	public Entity Entity => ((Entity)m_EntityRef.Entity) ?? m_FactRef.Entity;

	[CanBeNull]
	public Etude Etude => Fact as Etude;

	[CanBeNull]
	public UnitCondition? UnitCondition => m_UnitCondition;

	[CanBeNull]
	public BlueprintScriptableObject Blueprint => Fact?.Blueprint ?? m_Blueprint;

	[CanBeNull]
	public BlueprintScriptableObject PathFeatureSource => m_PathFeatureSource;

	[CanBeNull]
	public BlueprintScriptableObject BlueprintRaw => m_Blueprint;

	[CanBeNull]
	public BlueprintCutscene Cutscene => m_Blueprint as BlueprintCutscene;

	[CanBeNull]
	public BlueprintPath Path => m_Blueprint as BlueprintPath;

	[CanBeNull]
	public int? PathRank => m_PathRank;

	[CanBeNull]
	public int? FeatureRank => m_FeatureRank;

	public bool IsMissing
	{
		get
		{
			if (Cutscene == null)
			{
				if (m_EntityRef.IsEmpty || m_EntityRef.Entity != null)
				{
					if (!m_FactRef.IsEmpty)
					{
						return m_FactRef.Fact == null;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	[JsonConstructor]
	private EntityFactSource([CanBeNull] Entity _entity = null, [CanBeNull] EntityFact _fact = null, [CanBeNull] BlueprintComponent _component = null, [CanBeNull] UnitCondition? _unitCondition = null, [CanBeNull] BlueprintScriptableObject _blueprint = null, [CanBeNull] BlueprintScriptableObject _pathFeatureSource = null, [CanBeNull] int? _pathRank = null, [CanBeNull] int? _featureRank = null)
	{
		m_FactRef = _fact;
		m_ComponentId = _component?.name;
		m_EntityRef = _entity?.Ref ?? default(EntityRef);
		m_UnitCondition = _unitCondition;
		m_Blueprint = _blueprint;
		m_PathFeatureSource = _pathFeatureSource;
		m_PathRank = _pathRank;
		m_FeatureRank = _featureRank;
	}

	public EntityFactSource([NotNull] EntityFact fact, [CanBeNull] BlueprintComponent component)
		: this(null, fact, component)
	{
	}

	public EntityFactSource([NotNull] Entity entity)
		: this(entity, null, null, null, null, null, null, null)
	{
	}

	public EntityFactSource([NotNull] CutscenePlayerData cutscene)
		: this(cutscene.Cutscene)
	{
	}

	public EntityFactSource(UnitCondition unitCondition)
		: this(null, null, null, unitCondition)
	{
	}

	public EntityFactSource([NotNull] Etude etude)
		: this(etude, null)
	{
	}

	public EntityFactSource([NotNull] BlueprintScriptableObject blueprint, int? pathRank = null)
		: this(null, null, null, null, blueprint, null, pathRank)
	{
	}

	public EntityFactSource([NotNull] BlueprintPath path, [CanBeNull] BlueprintScriptableObject pathFeatureSource, int pathRank, int featureRank)
		: this(null, null, null, null, path, pathFeatureSource, pathRank, featureRank)
	{
	}

	protected EntityFactSource()
	{
	}

	public bool IsFrom([NotNull] EntityFact fact, [CanBeNull] BlueprintComponent component = null)
	{
		if (m_FactRef.Entity?.UniqueId == fact.Owner?.UniqueId && m_FactRef.FactId == fact.UniqueId)
		{
			if (component != null)
			{
				return component.name == m_ComponentId;
			}
			return true;
		}
		return false;
	}

	public bool IsFrom([NotNull] Entity entity)
	{
		if (!(m_EntityRef.Entity?.UniqueId == entity.UniqueId))
		{
			return m_FactRef.Entity?.UniqueId == entity.UniqueId;
		}
		return true;
	}

	public bool IsFrom(UnitCondition unitCondition)
	{
		return m_UnitCondition == unitCondition;
	}

	public bool IsFrom(Etude etude)
	{
		return Etude == etude;
	}

	public bool Equals(EntityFactSource other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && m_EntityRef.Equals(other.m_EntityRef) && m_FactRef.Equals(other.m_FactRef) && m_ComponentId == other.m_ComponentId && m_UnitCondition == other.m_UnitCondition && m_Blueprint == other.m_Blueprint)
			{
				return m_PathRank == other.m_PathRank;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityFactSource other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_EntityRef, m_FactRef, m_ComponentId, m_UnitCondition, m_Blueprint, m_PathFeatureSource, m_PathRank);
	}

	public static bool operator ==(EntityFactSource o1, EntityFactSource o2)
	{
		return o1?.Equals(o2) ?? ((object)o2 == null);
	}

	public static bool operator !=(EntityFactSource o1, EntityFactSource o2)
	{
		if ((object)o1 == null)
		{
			return (object)o2 != null;
		}
		return !o1.Equals(o2);
	}

	public override string ToString()
	{
		if (!m_FactRef.IsEmpty)
		{
			if (!m_ComponentId.IsNullOrEmpty())
			{
				return $"FactSource[{m_FactRef.Fact}]({m_ComponentId})";
			}
			return $"FactSource[{m_FactRef.Fact}]";
		}
		if (!m_EntityRef.IsEmpty)
		{
			return $"FactSource[{m_EntityRef.Entity}]";
		}
		if (m_UnitCondition.HasValue)
		{
			return $"FactSource[{m_UnitCondition.Value}]";
		}
		if (m_Blueprint != null)
		{
			if (m_PathRank.HasValue)
			{
				return $"FactSource[{m_Blueprint}]({m_PathRank.Value})";
			}
			return $"FactSource[{m_Blueprint}]";
		}
		return "FactSource[???]";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef obj = m_EntityRef;
		Hash128 val = EntityRefHasher.GetHash128(ref obj);
		result.Append(ref val);
		EntityFactRef obj2 = m_FactRef;
		Hash128 val2 = StructHasher<EntityFactRef>.GetHash128(ref obj2);
		result.Append(ref val2);
		result.Append(m_ComponentId);
		if (m_UnitCondition.HasValue)
		{
			UnitCondition val3 = m_UnitCondition.Value;
			result.Append(ref val3);
		}
		Hash128 val4 = SimpleBlueprintHasher.GetHash128(m_Blueprint);
		result.Append(ref val4);
		Hash128 val5 = SimpleBlueprintHasher.GetHash128(m_PathFeatureSource);
		result.Append(ref val5);
		if (m_PathRank.HasValue)
		{
			int val6 = m_PathRank.Value;
			result.Append(ref val6);
		}
		if (m_FeatureRank.HasValue)
		{
			int val7 = m_FeatureRank.Value;
			result.Append(ref val7);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityFactSource source = new EntityFactSource();
		result = Unsafe.As<EntityFactSource, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityFactSource>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef value = m_EntityRef;
		formatter.Field(0, "m_EntityRef", ref value, state);
		EntityFactRef value2 = m_FactRef;
		formatter.Field(1, "m_FactRef", ref value2, state);
		string value3 = m_ComponentId;
		formatter.StringField(2, "m_ComponentId", ref value3, state);
		UnitCondition? value4 = m_UnitCondition;
		formatter.EnumNullableField(3, "m_UnitCondition", ref value4, state);
		BlueprintScriptableObject value5 = m_Blueprint;
		formatter.Field(4, "m_Blueprint", ref value5, state);
		BlueprintScriptableObject value6 = m_PathFeatureSource;
		formatter.Field(5, "m_PathFeatureSource", ref value6, state);
		int? value7 = m_PathRank;
		formatter.UnmanagedNullableField(6, "m_PathRank", ref value7, state);
		int? value8 = m_FeatureRank;
		formatter.UnmanagedNullableField(7, "m_FeatureRank", ref value8, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityFactSource>();
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
				m_EntityRef = formatter.ReadPackable<EntityRef>(state);
				break;
			case 1:
				m_FactRef = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 2:
				m_ComponentId = formatter.ReadString(state);
				break;
			case 3:
				m_UnitCondition = formatter.ReadNullableEnum<UnitCondition>(state);
				break;
			case 4:
				m_Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 5:
				m_PathFeatureSource = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 6:
				m_PathRank = formatter.ReadNullableUnmanaged<int>(state);
				break;
			case 7:
				m_FeatureRank = formatter.ReadNullableUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
