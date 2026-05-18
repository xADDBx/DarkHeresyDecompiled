using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartWarhammerMovementApPerCellThreateningArea : UnitPart, IHashable, IOwlPackable<PartWarhammerMovementApPerCellThreateningArea>
{
	private readonly List<(EntityFactComponent Runtime, OverrideWarhammerMovementApPerCellThreateningArea Component)> m_ThreateningAreaEntries = new List<(EntityFactComponent, OverrideWarhammerMovementApPerCellThreateningArea)>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartWarhammerMovementApPerCellThreateningArea",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public static float GetThreateningArea(BaseUnitEntity unit)
	{
		PartWarhammerMovementApPerCellThreateningArea warhammerMovementApPerCellThreateningAreaOptional = unit.GetWarhammerMovementApPerCellThreateningAreaOptional();
		if (warhammerMovementApPerCellThreateningAreaOptional != null)
		{
			foreach (var threateningAreaEntry in warhammerMovementApPerCellThreateningAreaOptional.m_ThreateningAreaEntries)
			{
				using (threateningAreaEntry.Runtime.SetScope())
				{
					float? warhammerMovementApPerCellThreateningArea = threateningAreaEntry.Component.GetWarhammerMovementApPerCellThreateningArea();
					if (warhammerMovementApPerCellThreateningArea.HasValue)
					{
						return warhammerMovementApPerCellThreateningArea.GetValueOrDefault();
					}
				}
			}
		}
		return unit.Blueprint.WarhammerMovementApPerCellThreateningArea;
	}

	public void Add(OverrideWarhammerMovementApPerCellThreateningArea component)
	{
		m_ThreateningAreaEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideWarhammerMovementApPerCellThreateningArea component)
	{
		m_ThreateningAreaEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_ThreateningAreaEntries.Empty())
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
		PartWarhammerMovementApPerCellThreateningArea source = new PartWarhammerMovementApPerCellThreateningArea();
		result = Unsafe.As<PartWarhammerMovementApPerCellThreateningArea, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartWarhammerMovementApPerCellThreateningArea>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartWarhammerMovementApPerCellThreateningArea>();
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
