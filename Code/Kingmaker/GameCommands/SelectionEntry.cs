using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SelectionEntry : IOwlPackable, IOwlPackable<SelectionEntry>
{
	[JsonProperty]
	public readonly BlueprintSelection Selection;

	[JsonProperty]
	public readonly int PathRank;

	[JsonProperty]
	public readonly BlueprintFeature Feature;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SelectionEntry",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public SelectionEntry([NotNull] BlueprintSelection selection, int pathRank, [NotNull] BlueprintFeature feature)
	{
		Selection = selection;
		PathRank = pathRank;
		Feature = feature;
	}

	public SelectionEntry(OwlPackConstructorParameter _)
	{
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SelectionEntry source = new SelectionEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<SelectionEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SelectionEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SelectionEntry>();
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
