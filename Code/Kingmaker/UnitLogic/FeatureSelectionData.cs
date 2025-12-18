using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public readonly struct FeatureSelectionData : IHashable, IOwlPackable, IOwlPackable<FeatureSelectionData>
{
	[JsonProperty]
	[NotNull]
	[OwlPackInclude]
	public readonly BlueprintPath Path;

	[JsonProperty]
	[OwlPackInclude]
	public readonly int Level;

	[JsonProperty]
	[NotNull]
	[OwlPackInclude]
	public readonly BlueprintSelectionFeature Selection;

	[JsonProperty]
	[NotNull]
	[OwlPackInclude]
	public readonly BlueprintFeature Feature;

	[JsonProperty]
	[OwlPackInclude]
	public readonly int Rank;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FeatureSelectionData",
		Fields = new FieldInfo[5]
		{
			new FieldInfo("Path", typeof(BlueprintPath)),
			new FieldInfo("Level", typeof(int)),
			new FieldInfo("Selection", typeof(BlueprintSelectionFeature)),
			new FieldInfo("Feature", typeof(BlueprintFeature)),
			new FieldInfo("Rank", typeof(int))
		}
	};

	public FeatureSelectionData(BlueprintPath path, int level, BlueprintSelectionFeature selection, BlueprintFeature feature, int rank)
	{
		Path = path;
		Level = level;
		Selection = selection;
		Feature = feature;
		Rank = rank;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Path);
		result.Append(ref val);
		int val2 = Level;
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(Selection);
		result.Append(ref val3);
		Hash128 val4 = SimpleBlueprintHasher.GetHash128(Feature);
		result.Append(ref val4);
		int val5 = Rank;
		result.Append(ref val5);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FeatureSelectionData source = default(FeatureSelectionData);
		result = Unsafe.As<FeatureSelectionData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FeatureSelectionData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintPath value = Path;
		formatter.Field(0, "Path", ref value, state);
		int value2 = Level;
		formatter.UnmanagedField(1, "Level", ref value2, state);
		BlueprintSelectionFeature value3 = Selection;
		formatter.Field(2, "Selection", ref value3, state);
		BlueprintFeature value4 = Feature;
		formatter.Field(3, "Feature", ref value4, state);
		int value5 = Rank;
		formatter.UnmanagedField(4, "Rank", ref value5, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FeatureSelectionData>();
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
				Unsafe.AsRef(in Path) = formatter.ReadPackable<BlueprintPath>(state);
				break;
			case 1:
				Unsafe.AsRef(in Level) = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Unsafe.AsRef(in Selection) = formatter.ReadPackable<BlueprintSelectionFeature>(state);
				break;
			case 3:
				Unsafe.AsRef(in Feature) = formatter.ReadPackable<BlueprintFeature>(state);
				break;
			case 4:
				Unsafe.AsRef(in Rank) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
