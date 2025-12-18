using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartMimic : BaseUnitPart, IHashable, IOwlPackable<UnitPartMimic>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_AmbushObjectEntityId;

	private MapObjectEntity m_AmbushObjectEntity;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartMimic",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_AmbushObjectEntityId", typeof(string))
		}
	};

	[CanBeNull]
	public MapObjectEntity AmbushObject
	{
		get
		{
			if (string.IsNullOrEmpty(m_AmbushObjectEntityId))
			{
				return null;
			}
			if (m_AmbushObjectEntity == null || !m_AmbushObjectEntity.IsInState)
			{
				foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
				{
					if (mapObject.UniqueId == m_AmbushObjectEntityId)
					{
						m_AmbushObjectEntity = mapObject;
					}
				}
			}
			return m_AmbushObjectEntity;
		}
	}

	public bool AmbushObjectAttached => !string.IsNullOrEmpty(m_AmbushObjectEntityId);

	public void AttachAmbushObject(MapObjectEntity ambushObject)
	{
		if (AmbushObjectAttached)
		{
			PFLog.Default.Error("Ambush object is already attached to mimic " + base.Owner.ToString());
			return;
		}
		m_AmbushObjectEntityId = ambushObject.UniqueId;
		m_AmbushObjectEntity = ambushObject;
	}

	public void HideAmbushObject()
	{
		if (AmbushObject != null)
		{
			AmbushObject.IsInGame = false;
		}
		m_AmbushObjectEntityId = null;
		m_AmbushObjectEntity = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_AmbushObjectEntityId);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartMimic source = new UnitPartMimic();
		result = Unsafe.As<UnitPartMimic, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartMimic>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_AmbushObjectEntityId", ref m_AmbushObjectEntityId, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartMimic>();
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
				m_AmbushObjectEntityId = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
