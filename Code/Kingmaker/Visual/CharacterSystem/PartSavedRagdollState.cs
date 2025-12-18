using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.Visual.CharactersRigidbody;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[OwlPackable(OwlPackableMode.Generate)]
public class PartSavedRagdollState : EntityPart, IHashable, IOwlPackable<PartSavedRagdollState>
{
	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private List<RigidbodyCreatureController.BoneData> m_BoneData = new List<RigidbodyCreatureController.BoneData>();

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Active;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartSavedRagdollState",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_BoneData", typeof(List<RigidbodyCreatureController.BoneData>)),
			new FieldInfo("m_Active", typeof(bool))
		}
	};

	public bool Active => m_Active;

	public void SaveRagdollState(RigidbodyCreatureController controller)
	{
		m_Active = controller.IsActive;
		if (m_Active)
		{
			controller.SaveBonesPosition(m_BoneData);
		}
		else
		{
			m_BoneData.Clear();
		}
	}

	public void RestoreRagdollState(RigidbodyCreatureController controller)
	{
		if (m_Active)
		{
			controller.RagdollCurrentPositions = m_BoneData;
			controller.RestoreRagdollPositions();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Active);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartSavedRagdollState source = new PartSavedRagdollState();
		result = Unsafe.As<PartSavedRagdollState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartSavedRagdollState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_BoneData", ref m_BoneData, state);
		formatter.UnmanagedField(1, "m_Active", ref m_Active, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartSavedRagdollState>();
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
				m_BoneData = formatter.ReadPackable<List<RigidbodyCreatureController.BoneData>>(state);
				break;
			case 1:
				m_Active = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
