using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[Obsolete]
[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartSizeModifier : BaseUnitPart, IHashable, IOwlPackable<UnitPartSizeModifier>
{
	private readonly List<EntityFact> m_SizeChangeFacts = new List<EntityFact>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartSizeModifier",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Add(EntityFact fact)
	{
		m_SizeChangeFacts.Add(fact);
		UpdateSize();
	}

	public void Remove(EntityFact fact)
	{
		m_SizeChangeFacts.Remove(fact);
		UpdateSize();
	}

	private void UpdateSize()
	{
		EntityFact entityFact = m_SizeChangeFacts.LastItem();
		if (entityFact == null)
		{
			base.Owner.Remove<UnitPartSizeModifier>();
			return;
		}
		Size? size = null;
		foreach (EntityFactComponent component in entityFact.Components)
		{
			size = ((component.SourceBlueprintComponent is Polymorph polymorph) ? new Size?(polymorph.GetUnitSize(component)) : ((component.SourceBlueprintComponent is ChangeUnitSize changeUnitSize) ? new Size?(changeUnitSize.GetUnitSize(component)) : null));
			if (size.HasValue)
			{
				break;
			}
		}
		if (!size.HasValue)
		{
			PFLog.Default.Error(entityFact.Blueprint, $"Invalid fact (has no ChangeUnitSize component): {entityFact.Blueprint}");
			m_SizeChangeFacts.RemoveAt(m_SizeChangeFacts.Count - 1);
			UpdateSize();
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		m_SizeChangeFacts.RemoveAll((EntityFact f) => !f.Active);
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
		UnitPartSizeModifier source = new UnitPartSizeModifier();
		result = Unsafe.As<UnitPartSizeModifier, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartSizeModifier>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartSizeModifier>();
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
