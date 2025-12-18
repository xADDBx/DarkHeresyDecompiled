using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitViewSettings : MechanicEntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<PartUnitViewSettings>
{
	public interface IOwner : IEntityPartOwner<PartUnitViewSettings>, IEntityPartOwner
	{
		PartUnitViewSettings ViewSettings { get; }
	}

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	private string m_CustomPrefabGuid;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitViewSettings",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_CustomPrefabGuid", typeof(string)),
			new FieldInfo("Doll", typeof(DollData))
		}
	};

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public DollData Doll { get; private set; }

	[CanBeNull]
	public string PrefabGuid
	{
		get
		{
			if (Doll?.RacePreset != null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(m_CustomPrefabGuid))
			{
				return m_CustomPrefabGuid;
			}
			return base.Owner.Blueprint.Prefab.AssetId;
		}
	}

	protected override void OnAttach()
	{
		SpawningData current = ContextData<SpawningData>.Current;
		if (current != null)
		{
			m_CustomPrefabGuid = current.PrefabGuid;
		}
	}

	public void SetDoll(DollData doll)
	{
		Doll = doll;
	}

	public void SetCustomPrefabGuid(string guid)
	{
		m_CustomPrefabGuid = guid;
	}

	public UnitEntityView Instantiate(bool ignorePolymorph = false)
	{
		if (!ignorePolymorph)
		{
			PartPolymorphed optional = base.Owner.GetOptional<PartPolymorphed>();
			UnitEntityView unitEntityView = optional?.Component.GetPrefab(base.Owner).Load();
			if (unitEntityView != null)
			{
				Quaternion rotation = (unitEntityView.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
				UnitEntityView component = Object.Instantiate(unitEntityView, base.Owner.Position, rotation).GetComponent<UnitEntityView>();
				optional.ViewReplacement = component.gameObject;
				component.DisableSizeScaling = true;
				return component;
			}
		}
		if (Doll?.RacePreset != null)
		{
			UnitEntityView unitEntityView2 = Doll.CreateUnitView();
			unitEntityView2.ViewTransform.position = base.Owner.Position;
			unitEntityView2.ViewTransform.rotation = Quaternion.Euler(0f, base.Owner.Orientation, 0f);
			return unitEntityView2;
		}
		UnitEntityView unitEntityView3 = ResourcesLibrary.TryGetResource<UnitEntityView>(PrefabGuid);
		if (unitEntityView3 == null)
		{
			PFLog.Default.Error(base.Owner.Blueprint, "Cannot find prefab for unit: " + PrefabGuid);
			return null;
		}
		Quaternion rotation2 = (unitEntityView3.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
		return Object.Instantiate(unitEntityView3, base.Owner.Position, rotation2);
	}

	public void PreloadResources()
	{
		if (Doll != null)
		{
			Doll.PreloadEquipmentEntities();
		}
		else if (!string.IsNullOrEmpty(PrefabGuid))
		{
			ResourcesLibrary.PreloadResource<GameObject>(PrefabGuid);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_CustomPrefabGuid);
		Hash128 val2 = ClassHasher<DollData>.GetHash128(Doll);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitViewSettings source = new PartUnitViewSettings();
		result = Unsafe.As<PartUnitViewSettings, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitViewSettings>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_CustomPrefabGuid", ref m_CustomPrefabGuid, state);
		DollData value = Doll;
		formatter.Field(1, "Doll", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitViewSettings>();
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
				m_CustomPrefabGuid = formatter.ReadString(state);
				break;
			case 1:
				Doll = formatter.ReadPackable<DollData>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
