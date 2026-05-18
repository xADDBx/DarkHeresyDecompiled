using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.HitSystem;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class DestructibleEntity : MapObjectEntity, PartHealth.IOwner, IEntityPartOwner<PartHealth>, IEntityPartOwner, PartDestructionStagesManager.IOwner, IEntityPartOwner<PartDestructionStagesManager>, IHashable, IOwlPackable<DestructibleEntity>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DestructibleEntity",
		OldNames = null,
		Fields = new FieldInfo[17]
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
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("WasHighlightedOnRevealAndNoticed", typeof(bool)),
			new FieldInfo("ViewSettings", typeof(MapObjectViewSettings)),
			new FieldInfo("IsNewInGame", typeof(bool))
		}
	};

	public bool IsWall { get; private set; }

	public bool ZAligned { get; private set; }

	public new BlueprintDestructibleObject Blueprint => (BlueprintDestructibleObject)base.Blueprint;

	public new IAbstractDestructibleEntityView View => (IAbstractDestructibleEntityView)base.View;

	public new IDestructibleEntityConfig Config => (IDestructibleEntityConfig)base.Config;

	public override ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy => ViewHandlingOnDisposePolicyType.Deactivate;

	public override bool CanBeAttackedDirectly => Config.CanBeAttackedDirectly;

	public PartHealth Health => GetRequired<PartHealth>();

	public PartDestructionStagesManager DestructionStages => GetRequired<PartDestructionStagesManager>();

	public SurfaceType SurfaceType => Blueprint.SurfaceType;

	public GridObstacle[] CurrentStageGridObstacles => View?.CurrentStageGridObstacles;

	public GridObstacle[] WholeStageGridObstacles => View?.WholeStageGridObstacles;

	public GridObstacle[] AllGridObstacles => View?.AllGridObstacles;

	public override IntRect SizeRect => BoundsToSizeRect(Config?.Bounds ?? default(Rect));

	public bool AutoHit => !Config.DisableAutoHit;

	public int HitChanceModifier => Config.HitChanceModifier;

	public override bool SetViewOrientation => false;

	public DestructibleEntity(IDestructibleEntityConfig config)
		: base(config)
	{
	}

	protected DestructibleEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartHealth>();
		GetOrCreate<PartDestructionStagesManager>();
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		AddFact(ConfigRoot.Instance.SystemMechanics.CommonDestructibleEntityFact);
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		CalculateIsWall();
		base.Orientation = (ZAligned ? 90 : 0);
	}

	public override StatBaseValue GetStatBaseValue(StatType type)
	{
		return type switch
		{
			StatType.MaxHitPoints => Blueprint.HitPoints, 
			StatType.Toughness => Blueprint.Toughness, 
			_ => base.GetStatBaseValue(type), 
		};
	}

	public static IntRect BoundsToSizeRect(Rect bounds)
	{
		Vector2 size = bounds.size;
		int num = Math.Max(0, Mathf.RoundToInt(size.x / GraphParamsMechanicsCache.GridCellSize) - 1);
		int num2 = Math.Max(0, Mathf.RoundToInt(size.y / GraphParamsMechanicsCache.GridCellSize) - 1);
		int num3 = ((num >= 2) ? (-(num / 2)) : 0);
		int xmax = ((num < 2) ? num : (num + num3));
		int num4 = ((num2 >= 2) ? (-(num2 / 2)) : 0);
		int ymax = ((num2 < 2) ? num2 : (num2 + num4));
		return new IntRect(num3, num4, xmax, ymax);
	}

	private void CalculateIsWall()
	{
		if (WholeStageGridObstacles.Length == 1)
		{
			IsWall = true;
			ZAligned = WholeStageGridObstacles[0].ZAligned;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		Vector3? vector = null;
		GridObstacle[] wholeStageGridObstacles = WholeStageGridObstacles;
		for (int i = 0; i < wholeStageGridObstacles.Length; i++)
		{
			Vector3 position = wholeStageGridObstacles[i].transform.position;
			if (!vector.HasValue)
			{
				vector = position;
				continue;
			}
			flag |= Math.Abs(vector.Value.x - position.x) > 0.1f;
			flag2 |= Math.Abs(vector.Value.z - position.z) > 0.1f;
		}
		IsWall = !flag || !flag2;
		ZAligned = flag2 && !flag;
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
		DestructibleEntity source = new DestructibleEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<DestructibleEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DestructibleEntity>(OwlPackTypeInfo);
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
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		bool value3 = base.WasHighlightedOnRevealAndNoticed;
		formatter.UnmanagedField(14, "WasHighlightedOnRevealAndNoticed", ref value3, state);
		MapObjectViewSettings value4 = base.ViewSettings;
		formatter.Field(15, "ViewSettings", ref value4, state);
		bool value5 = base.IsNewInGame;
		formatter.UnmanagedField(16, "IsNewInGame", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DestructibleEntity>();
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
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				base.WasHighlightedOnRevealAndNoticed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				base.ViewSettings = formatter.ReadPackable<MapObjectViewSettings>(state);
				break;
			case 16:
				base.IsNewInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
