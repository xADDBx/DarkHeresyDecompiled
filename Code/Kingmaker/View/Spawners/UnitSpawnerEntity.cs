using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.View.Spawners.UnitSpawnerBase+MyData")]
public sealed class UnitSpawnerEntity : AbstractUnitSpawnerEntity, IHashable, IOwlPackable<UnitSpawnerEntity>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitSpawnerEntity",
		OldNames = new string[1] { "Kingmaker.View.Spawners.UnitSpawnerBase+MyData" },
		Fields = new FieldInfo[13]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("HasSpawned", typeof(bool)),
			new FieldInfo("HasDied", typeof(bool)),
			new FieldInfo("m_SpawnedUnit", typeof(EntityRef<AbstractUnitEntity>))
		}
	};

	public new IUnitSpawnerConfig Config => (IUnitSpawnerConfig)base.Config;

	public UnitSpawnerEntity(IUnitSpawnerConfig config)
		: base(config)
	{
	}

	public UnitSpawnerEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		if (Game.Instance.Player.AllCharacters.FirstItem((BaseUnitEntity u) => u.Blueprint.CheckEqualsWithPrototype(base.Blueprint))?.GetCompanionOptional() != null)
		{
			throw new InvalidOperationException(string.Format("Can't spawn {0} because it is companion. Use {1} instead.", base.Blueprint, "CompanionSpawner"));
		}
		using ((Config.Encounter != null) ? ContextData<BaseUnitEntity.EncounterData>.Request().Setup(Config.Encounter, base.UniqueId) : null)
		{
			AbstractUnitEntity abstractUnitEntity = (Config.IsLightweight ? Game.Instance.Controllers.EntitySpawner.SpawnLightweightUnit(base.Blueprint, position, rotation, HoldingState, Config.SelectedCustomizationVariation) : Game.Instance.Controllers.EntitySpawner.SpawnUnit(base.Blueprint, position, rotation, HoldingState, Config.SelectedCustomizationVariation));
			abstractUnitEntity.SelectVoGuid(Config.VoIdIndex.GetVoGuid(base.Blueprint));
			if (!(abstractUnitEntity is BaseUnitEntity baseUnitEntity))
			{
				return abstractUnitEntity;
			}
			if (Config.Encounter == null)
			{
				baseUnitEntity.CombatGroup.Id = "<peaceful-unit>";
			}
			if (Config.BossMusicEnable)
			{
				baseUnitEntity.MusicBossFightType = Config.MusicBossFightType;
			}
			return baseUnitEntity;
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (base.HasSpawned && abstractUnitEntity != null && abstractUnitEntity == base.SpawnedUnit)
		{
			CustomIdleAnimationMonoComponent customIdleAnimation = Config.CustomIdleAnimation;
			if (!(customIdleAnimation == null) && abstractUnitEntity.View.AnimationManager != null)
			{
				abstractUnitEntity.View.AnimationManager.CustomIdleWrappers = customIdleAnimation.IdleClips;
			}
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (base.HasSpawned && abstractUnitEntity == base.SpawnedUnit)
		{
			base.HasDied = true;
		}
	}

	public void HandleUnitDeath()
	{
	}

	protected override void OnSetConfig(IEntityConfig config)
	{
		base.OnSetConfig(config);
		AbstractUnitEntity spawnedUnit = base.SpawnedUnit;
		if (spawnedUnit != null && spawnedUnit is BaseUnitEntity baseUnitEntity)
		{
			if (Config.BossMusicEnable)
			{
				baseUnitEntity.MusicBossFightType = Config.MusicBossFightType;
			}
			else
			{
				baseUnitEntity.MusicBossFightType = null;
			}
		}
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
		UnitSpawnerEntity source = new UnitSpawnerEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<UnitSpawnerEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitSpawnerEntity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		bool value2 = base.HasSpawned;
		formatter.UnmanagedField(10, "HasSpawned", ref value2, state);
		bool value3 = base.HasDied;
		formatter.UnmanagedField(11, "HasDied", ref value3, state);
		formatter.Field(12, "m_SpawnedUnit", ref m_SpawnedUnit, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitSpawnerEntity>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				base.HasSpawned = formatter.ReadUnmanaged<bool>(state);
				break;
			case 11:
				base.HasDied = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_SpawnedUnit = formatter.ReadPackable<EntityRef<AbstractUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
