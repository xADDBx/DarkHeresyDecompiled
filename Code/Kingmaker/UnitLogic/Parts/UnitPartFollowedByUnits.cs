using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartFollowedByUnits : BaseUnitPart, IHashable, IOwlPackable<UnitPartFollowedByUnits>
{
	public readonly HashSet<AbstractUnitEntity> Followers = new HashSet<AbstractUnitEntity>();

	public readonly Dictionary<AbstractUnitEntity, FollowerAction> FollowerDesiredActions = new Dictionary<AbstractUnitEntity, FollowerAction>();

	public readonly Dictionary<AbstractUnitEntity, FollowerActionType> FollowersActionTypesTemp = new Dictionary<AbstractUnitEntity, FollowerActionType>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartFollowedByUnits",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool ForceRefresh { get; set; }

	public double LastRefreshTime { get; set; }

	public Vector3 LastKnownDestination { get; set; }

	public bool PositionChanged { get; set; } = true;


	public FollowerAction? GetFollowerAction(AbstractUnitEntity unit)
	{
		if (!FollowerDesiredActions.TryGetValue(unit, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<AbstractUnitEntity> GetActiveFollowers()
	{
		foreach (AbstractUnitEntity follower in Followers)
		{
			if (!follower.IsSleeping && !follower.LifeState.IsDead)
			{
				yield return follower;
			}
		}
	}

	public void AddFollower(AbstractUnitEntity follower)
	{
		Followers.Add(follower);
		ForceRefresh = true;
	}

	public void RemoveFollower(AbstractUnitEntity follower)
	{
		Followers.Remove(follower);
		FollowerDesiredActions.Remove(follower);
		if (Followers.Count < 1)
		{
			base.Owner.Remove<UnitPartFollowedByUnits>();
		}
	}

	public void ClearCache()
	{
		ForceRefresh = true;
		FollowerDesiredActions.Clear();
		LastKnownDestination = Vector3.zero;
	}

	public void Cleanup()
	{
		Followers.RemoveWhere((AbstractUnitEntity u) => u?.GetOptional<UnitPartFollowUnit>() == null);
		if (Followers.Count < 1)
		{
			base.Owner.Remove<UnitPartFollowedByUnits>();
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
		UnitPartFollowedByUnits source = new UnitPartFollowedByUnits();
		result = Unsafe.As<UnitPartFollowedByUnits, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartFollowedByUnits>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartFollowedByUnits>();
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
