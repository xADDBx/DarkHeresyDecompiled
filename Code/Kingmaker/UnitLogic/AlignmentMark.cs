using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[Obsolete]
[OwlPackable(OwlPackableMode.Generate)]
public class AlignmentMark : Feature, IHashable, IOwlPackable<AlignmentMark>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AlignmentMark",
		OldNames = null,
		Fields = new FieldInfo[15]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_ParentContext", typeof(MechanicsContext)),
			new FieldInfo("m_Context", typeof(MechanicsContext)),
			new FieldInfo("Rank", typeof(int)),
			new FieldInfo("IsTemporary", typeof(bool)),
			new FieldInfo("Param", typeof(FeatureParam)),
			new FieldInfo("IgnorePrerequisites", typeof(bool)),
			new FieldInfo("DisabledBecauseOfPrerequisites", typeof(bool))
		}
	};

	public AlignmentMark(BlueprintFeature blueprint, IEvalContext context)
		: base(blueprint, context)
	{
	}

	public AlignmentMark(JsonConstructorMark _)
		: base(_)
	{
	}

	protected AlignmentMark()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AlignmentMark source = new AlignmentMark();
		result = Unsafe.As<AlignmentMark, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AlignmentMark>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ComponentsData", ref m_ComponentsData, state);
		List<EntityFactComponent> value = base.m_Components;
		formatter.Field(1, "m_Components", ref value, state);
		formatter.Field(2, "m_Sources", ref m_Sources, state);
		formatter.Field(3, "m_ChildrenFacts", ref m_ChildrenFacts, state);
		string value2 = base.UniqueId;
		formatter.StringField(4, "UniqueId", ref value2, state);
		formatter.Field(5, "m_Blueprint", ref m_Blueprint, state);
		bool value3 = base.IsActive;
		formatter.UnmanagedField(6, "IsActive", ref value3, state);
		EntityFactRef value4 = base.ChildOf;
		formatter.Field(7, "ChildOf", ref value4, state);
		formatter.Field(8, "m_ParentContext", ref m_ParentContext, state);
		formatter.Field(9, "m_Context", ref m_Context, state);
		int value5 = base.Rank;
		formatter.UnmanagedField(10, "Rank", ref value5, state);
		bool value6 = base.IsTemporary;
		formatter.UnmanagedField(11, "IsTemporary", ref value6, state);
		FeatureParam value7 = base.Param;
		formatter.Field(12, "Param", ref value7, state);
		bool value8 = base.IgnorePrerequisites;
		formatter.UnmanagedField(13, "IgnorePrerequisites", ref value8, state);
		bool value9 = base.DisabledBecauseOfPrerequisites;
		formatter.UnmanagedField(14, "DisabledBecauseOfPrerequisites", ref value9, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AlignmentMark>();
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
				m_ComponentsData = formatter.ReadPackable<Dictionary<string, List<IEntityFactComponentSavableData>>>(state);
				break;
			case 1:
				base.m_Components = formatter.ReadPackable<List<EntityFactComponent>>(state);
				break;
			case 2:
				m_Sources = formatter.ReadPackable<List<EntityFactSource>>(state);
				break;
			case 3:
				m_ChildrenFacts = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			case 4:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 5:
				m_Blueprint = formatter.ReadPackable<BlueprintFact>(state);
				break;
			case 6:
				base.IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				base.ChildOf = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 8:
				m_ParentContext = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 9:
				m_Context = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 10:
				base.Rank = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				base.IsTemporary = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				base.Param = formatter.ReadPackable<FeatureParam>(state);
				break;
			case 13:
				base.IgnorePrerequisites = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				base.DisabledBecauseOfPrerequisites = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
