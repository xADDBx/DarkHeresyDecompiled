using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class FeatureParam : IHashable, IOwlPackable, IOwlPackable<FeatureParam>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FeatureParam",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("WeaponCategory", typeof(WeaponCategory?)),
			new FieldInfo("StatType", typeof(StatType?))
		}
	};

	public FeatureParam Value => this;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintScriptableObject Blueprint { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public WeaponCategory? WeaponCategory { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public StatType? StatType { get; private set; }

	public FeatureParam(BlueprintScriptableObject blueprint)
		: this()
	{
		Blueprint = blueprint;
	}

	public FeatureParam(WeaponCategory? weaponCategory)
		: this()
	{
		WeaponCategory = weaponCategory;
	}

	public FeatureParam(StatType? statType)
		: this()
	{
		StatType = statType;
	}

	public FeatureParam()
	{
	}

	public static implicit operator FeatureParam(BlueprintScriptableObject blueprint)
	{
		return new FeatureParam(blueprint);
	}

	public static implicit operator FeatureParam(WeaponCategory weaponCategory)
	{
		return new FeatureParam(weaponCategory);
	}

	public static implicit operator FeatureParam(StatType statType)
	{
		return new FeatureParam(statType);
	}

	public bool Equals(FeatureParam other)
	{
		if (object.Equals(Blueprint, other?.Blueprint) && WeaponCategory == other?.WeaponCategory)
		{
			return StatType == other?.StatType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is FeatureParam)
		{
			return Equals((FeatureParam)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((Blueprint != null) ? Blueprint.GetHashCode() : 0) * 397) ^ WeaponCategory.GetHashCode()) * 397) ^ StatType.GetHashCode();
	}

	public static bool operator ==(FeatureParam f1, FeatureParam f2)
	{
		return f1?.Equals(f2) ?? ((object)f2 == null);
	}

	public static bool operator !=(FeatureParam f1, FeatureParam f2)
	{
		return !(f1?.Equals(f2) ?? ((object)f2 == null));
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		if (WeaponCategory.HasValue)
		{
			WeaponCategory val2 = WeaponCategory.Value;
			result.Append(ref val2);
		}
		if (StatType.HasValue)
		{
			StatType val3 = StatType.Value;
			result.Append(ref val3);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FeatureParam source = new FeatureParam();
		result = Unsafe.As<FeatureParam, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FeatureParam>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintScriptableObject value = Blueprint;
		formatter.Field(0, "Blueprint", ref value, state);
		WeaponCategory? value2 = WeaponCategory;
		formatter.EnumNullableField(1, "WeaponCategory", ref value2, state);
		StatType? value3 = StatType;
		formatter.EnumNullableField(2, "StatType", ref value3, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FeatureParam>();
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
				Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 1:
				WeaponCategory = formatter.ReadNullableEnum<WeaponCategory>(state);
				break;
			case 2:
				StatType = formatter.ReadNullableEnum<StatType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
