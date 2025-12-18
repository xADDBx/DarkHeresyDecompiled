using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartVisualChanges : BaseUnitPart, IHashable, IOwlPackable<UnitPartVisualChanges>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<string> SourceBone = new List<string>();

	[JsonProperty]
	[OwlPackInclude]
	public bool BoneReplaced;

	[JsonProperty]
	[OwlPackInclude]
	public bool BoneDefault;

	[JsonProperty]
	[OwlPackInclude]
	public CastSource CastSource;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartVisualChanges",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("SourceBone", typeof(List<string>)),
			new FieldInfo("BoneReplaced", typeof(bool)),
			new FieldInfo("BoneDefault", typeof(bool)),
			new FieldInfo("CastSource", typeof(CastSource))
		}
	};

	public void ReplaceCastSource(CastSource castSource)
	{
		CastSource = castSource;
	}

	public void AddReplacementBone(string newBone)
	{
		if (!BoneDefault)
		{
			BoneReplaced = true;
		}
		SourceBone.Add(newBone);
	}

	public void RemoveReplacementBone(string oldBone)
	{
		SourceBone.Remove(oldBone);
		if (SourceBone.Count <= 0)
		{
			BoneReplaced = false;
		}
	}

	public string GetReplacementBone()
	{
		if (SourceBone.Count < 0)
		{
			return "Locator_HeadCenterFX_00";
		}
		return SourceBone[SourceBone.Count - 1];
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<string> sourceBone = SourceBone;
		if (sourceBone != null)
		{
			for (int i = 0; i < sourceBone.Count; i++)
			{
				Hash128 val2 = StringHasher.GetHash128(sourceBone[i]);
				result.Append(ref val2);
			}
		}
		result.Append(ref BoneReplaced);
		result.Append(ref BoneDefault);
		result.Append(ref CastSource);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartVisualChanges source = new UnitPartVisualChanges();
		result = Unsafe.As<UnitPartVisualChanges, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartVisualChanges>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "SourceBone", ref SourceBone, state);
		formatter.UnmanagedField(1, "BoneReplaced", ref BoneReplaced, state);
		formatter.UnmanagedField(2, "BoneDefault", ref BoneDefault, state);
		formatter.EnumField(3, "CastSource", ref CastSource, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartVisualChanges>();
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
				SourceBone = formatter.ReadPackable<List<string>>(state);
				break;
			case 1:
				BoneReplaced = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				BoneDefault = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				CastSource = formatter.ReadEnum<CastSource>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
