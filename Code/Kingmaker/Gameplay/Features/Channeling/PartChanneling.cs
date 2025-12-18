using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Channeling;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.Gameplay.Parts.PartChanneling")]
public class PartChanneling : MechanicEntityPart, IUIChanneling, IHashable, IOwlPackable<PartChanneling>
{
	private EntityFactRef<Buff> _buff;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartChanneling",
		OldNames = new string[1] { "Kingmaker.Gameplay.Parts.PartChanneling" },
		Fields = new FieldInfo[0]
	};

	public AbilityData Ability { get; private set; }

	public TargetWrapper Target { get; private set; }

	public bool IsActive
	{
		get
		{
			if (Buff != null)
			{
				return IsTargetReachable();
			}
			return false;
		}
	}

	public Buff Buff => _buff;

	public bool IsTargetReachable()
	{
		if (Target != null)
		{
			IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Target.Entity?.Size ?? Size.Medium);
			IntRect rectForSize2 = SizePathfindingHelper.GetRectForSize(Ability.Caster.Size);
			LosDescription warhammerLos = LosCalculations.GetWarhammerLos(Target.Point, rectForSize, Ability.Caster.Position, rectForSize2);
			if (WarhammerGeometryUtils.DistanceToInCells(Target.Point, rectForSize, Ability.Caster.Position, rectForSize2) <= Ability.RangeCells)
			{
				return (LosCalculations.CoverType)warhammerLos != LosCalculations.CoverType.LosBlocker;
			}
			return false;
		}
		return false;
	}

	public void Set(Buff buff, ChannelingLogic logic)
	{
		Buff?.MarkExpired();
		_buff = buff;
		Ability = new AbilityData(logic.Ability, base.Owner);
		Target = buff.Context.ClickedTarget;
	}

	public void Clear(Buff buff, ChannelingLogic _)
	{
		if (Buff == buff)
		{
			_buff = default(EntityFactRef<Buff>);
			Ability = null;
			Target = null;
			RemoveSelf();
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (Buff == null)
		{
			RemoveSelf();
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
		PartChanneling source = new PartChanneling();
		result = Unsafe.As<PartChanneling, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartChanneling>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartChanneling>();
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
