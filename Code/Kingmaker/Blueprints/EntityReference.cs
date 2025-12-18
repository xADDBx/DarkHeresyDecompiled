using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class EntityReference : IHashable, IOwlPackable, IOwlPackable<EntityReference>
{
	private IEntity m_CachedData;

	[HideInInspector]
	public string EntityNameInEditor;

	[HideInInspector]
	[JsonProperty("_entity_id")]
	[OwlPackInclude]
	public string UniqueId;

	[HideInInspector]
	[JsonProperty("SceneAssetGuid")]
	[OwlPackInclude]
	public string SceneAssetGuid;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityReference",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("SceneAssetGuid", typeof(string))
		}
	};

	[CanBeNull]
	public IEntityViewBase FindView()
	{
		return FindData()?.View;
	}

	[CanBeNull]
	public IEntity FindData()
	{
		if (m_CachedData == null || !m_CachedData.IsInState)
		{
			IPersistentState service = InterfaceServiceLocator.GetService<IPersistentState>();
			m_CachedData = service.GetEntityDataFromLoadedAreaState(UniqueId);
			EntityReferenceTracker.Register(this);
		}
		return m_CachedData;
	}

	internal void DropCached()
	{
		m_CachedData = null;
	}

	public override string ToString()
	{
		return EntityNameInEditor ?? UniqueId ?? "";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(UniqueId);
		result.Append(SceneAssetGuid);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityReference source = new EntityReference();
		result = Unsafe.As<EntityReference, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityReference>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "UniqueId", ref UniqueId, state);
		formatter.StringField(1, "SceneAssetGuid", ref SceneAssetGuid, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityReference>();
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
				UniqueId = formatter.ReadString(state);
				break;
			case 1:
				SceneAssetGuid = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
