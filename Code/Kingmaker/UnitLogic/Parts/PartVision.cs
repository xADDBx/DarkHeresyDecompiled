using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartVision : BaseUnitPart, IHashable, IOwlPackable<PartVision>
{
	public interface IOwner : IEntityPartOwner<PartVision>, IEntityPartOwner
	{
		PartVision Vision { get; }
	}

	public readonly SortedList<string, BaseUnitEntity> CanBeInRange = new SortedList<string, BaseUnitEntity>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartVision",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("VisionRangeMetersOverride", typeof(float?)),
			new FieldInfo("CombatVisionRangeMetersOverride", typeof(float?))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public float? VisionRangeMetersOverride { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float? CombatVisionRangeMetersOverride { get; set; }

	public UnitSightCache SightCache { get; private set; }

	public ScriptZoneEntity ExtendedVisionArea { get; set; }

	public float RangeMeters
	{
		get
		{
			if (base.Owner.Faction.IsPlayer)
			{
				return 22f;
			}
			float num = VisionRangeMetersOverride ?? GetDefaultVisionRange();
			if (!base.Owner.IsInCombat)
			{
				return num;
			}
			return CombatVisionRangeMetersOverride ?? Math.Max(22f, num);
		}
	}

	private float GetDefaultVisionRange()
	{
		return 8.5f;
	}

	protected override void OnAttachOrPostLoad()
	{
		SightCache = new UnitSightCache(base.Owner);
	}

	public bool HasLOS(MechanicEntity other)
	{
		using (ProfileScope.New("PartVision.HasLOS(MechanicEntity)"))
		{
			Vector3 eyePosition = base.Owner.EyePosition;
			float rangeMeters = RangeMeters;
			if (ExtendedVisionArea != null && ExtendedVisionArea.ContainsPosition(other.Position))
			{
				return true;
			}
			if (other is BaseUnitEntity baseUnitEntity && !CanBeInRange.ContainsKey(baseUnitEntity.UniqueId))
			{
				return false;
			}
			if (GeometryUtils.SqrDistance2D(eyePosition, other.EyePosition) > rangeMeters * rangeMeters)
			{
				return false;
			}
			return GameHelper.CheckLOS(base.Owner, other);
		}
	}

	public bool HasLOS(GraphNode targetNode, Vector3 eyePosition)
	{
		return HasLOS((Vector3)targetNode.position, eyePosition, 0f);
	}

	private bool HasLOS(Vector3 point, Vector3 eyePosition, float fudgeRadius)
	{
		using (ProfileScope.New("PartVision.HasLos(Vector3, float)"))
		{
			float rangeMeters = RangeMeters;
			return (ExtendedVisionArea != null && ExtendedVisionArea.ContainsPosition(point)) || (GeometryUtils.SqrDistance2D(eyePosition, point) <= rangeMeters * rangeMeters && !LineOfSightGeometry.Instance.HasObstacle(eyePosition, point, fudgeRadius));
		}
	}

	public bool HasLOS(EntityViewBase view)
	{
		return HasLOS(view.ViewTransform.position, base.Owner.EyePosition, (view as MapObjectView)?.FogOfWarFudgeRadius ?? 0f);
	}

	private bool HasLOSBetweenPoints(Vector3 p1, Vector3 p2, Transform t = null)
	{
		float rangeMeters = RangeMeters;
		bool num = GeometryUtils.SqrDistance2D(p1, p2) <= rangeMeters * rangeMeters;
		bool flag = LineOfSightGeometry.Instance.HasObstacle(p1, p2, t?.GetInstanceID() ?? 0);
		bool flag2 = ExtendedVisionArea != null && ExtendedVisionArea.ContainsPosition(p2);
		return (num && !flag) || flag2;
	}

	public bool HasLOS(GraphNode targetNode, GraphNode overridePositionNode)
	{
		return HasLOS((Vector3)targetNode.position, (Vector3)overridePositionNode.position);
	}

	public bool HasLOS(Vector3 point, Vector3 overridePosition)
	{
		try
		{
			Vector3 p = overridePosition + LosCalculations.EyeShift;
			return HasLOSBetweenPoints(p, point);
		}
		finally
		{
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (VisionRangeMetersOverride.HasValue)
		{
			float val2 = VisionRangeMetersOverride.Value;
			result.Append(ref val2);
		}
		if (CombatVisionRangeMetersOverride.HasValue)
		{
			float val3 = CombatVisionRangeMetersOverride.Value;
			result.Append(ref val3);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartVision source = new PartVision();
		result = Unsafe.As<PartVision, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartVision>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		float? value = VisionRangeMetersOverride;
		formatter.UnmanagedNullableField(0, "VisionRangeMetersOverride", ref value, state);
		float? value2 = CombatVisionRangeMetersOverride;
		formatter.UnmanagedNullableField(1, "CombatVisionRangeMetersOverride", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartVision>();
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
				VisionRangeMetersOverride = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 1:
				CombatVisionRangeMetersOverride = formatter.ReadNullableUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
