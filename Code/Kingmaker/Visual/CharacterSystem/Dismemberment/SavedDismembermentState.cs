using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Networking.Serialization;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

[OwlPackable(OwlPackableMode.Generate)]
public class SavedDismembermentState : BaseUnitPart, IHashable, IOwlPackable<SavedDismembermentState>
{
	[JsonProperty]
	[CanBeNull]
	[GameStateIgnore]
	[OwlPackInclude]
	private List<BoneTransformData> m_Bones;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public int SetIndex = -1;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public int DestroyedPieceIndex = -1;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SavedDismembermentState",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_Bones", typeof(List<BoneTransformData>)),
			new FieldInfo("SetIndex", typeof(int)),
			new FieldInfo("DestroyedPieceIndex", typeof(int))
		}
	};

	public bool Active => SetIndex >= 0;

	private static void ForEachBone(UnitDismembermentManager manager, Action<Transform> action)
	{
		ForEachBoneInternal(manager.GetComponentInChildren<DismembermentSetBehaviour>().transform, action);
		static void ForEachBoneInternal(Transform t, Action<Transform> a)
		{
			a(t);
			for (int i = 0; i < t.childCount; i++)
			{
				ForEachBoneInternal(t.GetChild(i), a);
			}
		}
	}

	public void SaveDismembermentState(UnitDismembermentManager dismembermentManager)
	{
		if (dismembermentManager.Dismembered)
		{
			SetIndex = dismembermentManager.SetIndex;
			DestroyedPieceIndex = dismembermentManager.DestroyedPieceIndex;
			m_Bones = new List<BoneTransformData>();
			ForEachBone(dismembermentManager, delegate(Transform i)
			{
				m_Bones.Add(new BoneTransformData(i.localPosition, i.localRotation));
			});
		}
		else
		{
			SetIndex = -1;
			DestroyedPieceIndex = -1;
			m_Bones = null;
		}
	}

	public void RestoreDismembermentState(UnitDismembermentManager dismembermentManager)
	{
		if (SetIndex < 0 || m_Bones == null)
		{
			return;
		}
		dismembermentManager.SetIndex = SetIndex;
		dismembermentManager.DestroyedPieceIndex = DestroyedPieceIndex;
		dismembermentManager.RestoreState();
		int index = 0;
		ForEachBone(dismembermentManager, delegate(Transform i)
		{
			if (index < m_Bones.Count)
			{
				i.localPosition = m_Bones[index].Position;
				i.localRotation = m_Bones[index].Rotation;
				index++;
			}
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SavedDismembermentState source = new SavedDismembermentState();
		result = Unsafe.As<SavedDismembermentState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SavedDismembermentState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Bones", ref m_Bones, state);
		formatter.UnmanagedField(1, "SetIndex", ref SetIndex, state);
		formatter.UnmanagedField(2, "DestroyedPieceIndex", ref DestroyedPieceIndex, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SavedDismembermentState>();
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
				m_Bones = formatter.ReadPackable<List<BoneTransformData>>(state);
				break;
			case 1:
				SetIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				DestroyedPieceIndex = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
