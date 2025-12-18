using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Code.Visual.Animation;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.Interaction;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[OwlPackable(OwlPackableMode.Generate)]
public class PartDetectiveServoSkull : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<PartDetectiveServoSkull>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartDetectiveServoSkull",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public float FlyHeightDeviationProgress { get; set; }

	public float FlyHeightDifference { get; set; }

	public IDetectiveServoskullDelegate? Delegate { get; set; }

	public bool IsBusy => base.Owner.GetOptional<InteractionProcessPart>()?.HasActiveInteraction ?? false;

	public BaseUnitEntity Leader => base.Owner.GetRequired<UnitPartFamiliar>().Leader;

	public Vector3 IdlePosition
	{
		get
		{
			BaseUnitEntity leader = Leader;
			Vector3 vector = (Vector3.back + Vector3.left) * 0.5f;
			Vector3 vector2 = Quaternion.Euler(0f, leader.Orientation, 0f) * vector;
			return leader.Position + vector2;
		}
	}

	public bool Enabled
	{
		get
		{
			if (!Game.Instance.Controllers.TurnController.InCombat)
			{
				return !Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull;
			}
			return false;
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		base.Owner.Sleepless.Retain();
	}

	protected override void OnDetach()
	{
		base.Owner.Sleepless.Release();
	}

	public async Task Scan(MapObjectEntity? target)
	{
		if (!Enabled)
		{
			throw new InvalidOperationException();
		}
		if (target != null)
		{
			Delegate?.SetScanTargetEntity(target);
			await MoveToScanPosition(target);
			await (Delegate?.PlayScanAnimation(target) ?? Task.CompletedTask);
			Delegate?.SetScanTargetEntity(null);
		}
	}

	private async Task MoveToScanPosition(MapObjectEntity target)
	{
		Vector3 normalized = (base.Owner.Position - target.Position).normalized;
		float scanToTargetDistance = ConfigRoot.Instance.DetectiveServoskull.ScanToTargetDistance;
		Vector3 vector = target.Position + normalized * scanToTargetDistance;
		ForcedPath path = PathfindingService.Instance.FindPathRT_Blocking(base.Owner.View.MovementAgent, vector, 0.1f);
		await base.Owner.Commands.Run(new UnitMoveToParams(path, vector, 0.1f)
		{
			MovementType = WalkSpeedType.Run
		});
		while (true)
		{
			IDetectiveServoskullDelegate? @delegate = Delegate;
			if (@delegate != null && @delegate.IsVisualSyncedToAgent)
			{
				break;
			}
			await Task.Delay(100);
		}
		base.Owner.TurnTo(target.Position);
	}

	public static PartDetectiveServoSkull? Find()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			PartDetectiveServoSkull fromOwner = GetFromOwner(item);
			if (fromOwner != null)
			{
				return fromOwner;
			}
		}
		return null;
	}

	public static bool IsOwner(BaseUnitEntity unit)
	{
		return GetFromOwner(unit) != null;
	}

	public static PartDetectiveServoSkull? GetFromOwner(BaseUnitEntity unit)
	{
		IEnumerable<AbstractUnitEntity> enumerable = unit.GetOptional<UnitPartFamiliarLeader>()?.EquippedFamiliars;
		if (enumerable == null)
		{
			return null;
		}
		foreach (AbstractUnitEntity item in enumerable)
		{
			PartDetectiveServoSkull optional = item.GetOptional<PartDetectiveServoSkull>();
			if (optional != null)
			{
				return optional;
			}
		}
		return null;
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
		PartDetectiveServoSkull source = new PartDetectiveServoSkull();
		result = Unsafe.As<PartDetectiveServoSkull, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartDetectiveServoSkull>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartDetectiveServoSkull>();
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
