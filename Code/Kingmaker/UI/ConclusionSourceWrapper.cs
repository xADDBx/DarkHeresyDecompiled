using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Framework.DetectiveSystem;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[Serializable]
[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class ConclusionSourceWrapper : IHashable, IOwlPackable, IOwlPackable<ConclusionSourceWrapper>
{
	public BlueprintCaseItem Item1;

	public BlueprintCaseItem Item2;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ConclusionSourceWrapper",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public ConclusionSourceWrapper()
	{
	}

	public ConclusionSourceWrapper(BlueprintCaseItem item1, BlueprintCaseItem item2)
	{
		Item1 = item1;
		Item2 = item2;
	}

	public bool Is(BlueprintConclusion.Source source)
	{
		if (source.Item1.Blueprint == Item1)
		{
			return source.Item2.Blueprint == Item2;
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ConclusionSourceWrapper source = new ConclusionSourceWrapper();
		result = Unsafe.As<ConclusionSourceWrapper, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConclusionSourceWrapper>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConclusionSourceWrapper>();
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
