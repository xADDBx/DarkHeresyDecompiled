using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ResourceManagement;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartHoldPrefabBundle : AbstractUnitPart, IHashable, IOwlPackable<PartHoldPrefabBundle>
{
	private BundledResourceHandle<UnitEntityView> m_Handle;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartHoldPrefabBundle",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (m_Handle == null)
		{
			m_Handle = BundledResourceHandle<UnitEntityView>.Request(base.Owner.Blueprint.Prefab.AssetId, hold: true);
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		if (m_Handle == null)
		{
			m_Handle = BundledResourceHandle<UnitEntityView>.Request(base.Owner.Blueprint.Prefab.AssetId, hold: true);
		}
	}

	protected override void OnViewWillDetach()
	{
		m_Handle?.Dispose();
		m_Handle = null;
		base.OnViewWillDetach();
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
		PartHoldPrefabBundle source = new PartHoldPrefabBundle();
		result = Unsafe.As<PartHoldPrefabBundle, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartHoldPrefabBundle>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartHoldPrefabBundle>();
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
