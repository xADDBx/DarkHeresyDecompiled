using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartBlindsense : BaseUnitPart, IHashable, IOwlPackable<UnitPartBlindsense>
{
	private readonly List<Feet> m_Ranges = new List<Feet>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartBlindsense",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public Feet Range { get; private set; }

	public void Add(Feet range)
	{
		m_Ranges.Add(range);
		UpdateRange();
	}

	public void Remove(Feet range)
	{
		m_Ranges.Remove(range);
		if (m_Ranges.Count == 0)
		{
			base.Owner.Remove<UnitPartBlindsense>();
		}
		else
		{
			UpdateRange();
		}
	}

	private void UpdateRange()
	{
		Range = 0.Feet();
		foreach (Feet range in m_Ranges)
		{
			Range = Math.Max(Range.Value, range.Value).Feet();
		}
	}

	public bool Reach(BaseUnitEntity unit)
	{
		float num = base.Owner.Corpulence + unit.Corpulence;
		if (base.Owner.DistanceTo(unit) - num <= Range.Meters)
		{
			return base.Owner.Vision.HasLOS(unit);
		}
		return false;
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
		UnitPartBlindsense source = new UnitPartBlindsense();
		result = Unsafe.As<UnitPartBlindsense, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartBlindsense>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartBlindsense>();
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
