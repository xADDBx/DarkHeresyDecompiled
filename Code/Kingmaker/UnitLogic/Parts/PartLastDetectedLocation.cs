using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartLastDetectedLocation : EntityPart, IHashable, IOwlPackable<PartLastDetectedLocation>
{
	[JsonProperty]
	[OwlPackInclude]
	private BlueprintAreaReference m_Area;

	[JsonProperty]
	[OwlPackInclude]
	private BlueprintAreaPartReference m_AreaPart;

	[JsonProperty]
	[OwlPackInclude]
	private int m_Chapter;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartLastDetectedLocation",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_Area", typeof(BlueprintAreaReference)),
			new FieldInfo("m_AreaPart", typeof(BlueprintAreaPartReference)),
			new FieldInfo("m_Chapter", typeof(int))
		}
	};

	public BlueprintArea Area => m_Area;

	public BlueprintAreaPart AreaPart => m_AreaPart;

	public int Chapter => m_Chapter;

	public void DetectLocation([NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaPart areaPart, int chapter)
	{
		m_Area = area.ToReference<BlueprintAreaReference>();
		m_AreaPart = areaPart.ToReference<BlueprintAreaPartReference>();
		m_Chapter = chapter;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = BlueprintReferenceHasher.GetHash128(m_Area);
		result.Append(ref val2);
		Hash128 val3 = BlueprintReferenceHasher.GetHash128(m_AreaPart);
		result.Append(ref val3);
		result.Append(ref m_Chapter);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartLastDetectedLocation source = new PartLastDetectedLocation();
		result = Unsafe.As<PartLastDetectedLocation, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartLastDetectedLocation>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Area", ref m_Area, state);
		formatter.Field(1, "m_AreaPart", ref m_AreaPart, state);
		formatter.UnmanagedField(2, "m_Chapter", ref m_Chapter, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartLastDetectedLocation>();
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
				m_Area = formatter.ReadPackable<BlueprintAreaReference>(state);
				break;
			case 1:
				m_AreaPart = formatter.ReadPackable<BlueprintAreaPartReference>(state);
				break;
			case 2:
				m_Chapter = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
