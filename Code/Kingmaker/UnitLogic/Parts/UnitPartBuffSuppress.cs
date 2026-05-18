using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartBuffSuppress : BaseUnitPart, IHashable, IOwlPackable<UnitPartBuffSuppress>
{
	private readonly MultiSet<BlueprintBuff> m_Buffs = new MultiSet<BlueprintBuff>();

	private readonly MultiSet<BlueprintAbilityGroup> m_Groups = new MultiSet<BlueprintAbilityGroup>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartBuffSuppress",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Suppress(BlueprintBuff buff)
	{
		m_Buffs.Add(buff);
		Update();
	}

	public void Release(BlueprintBuff buff)
	{
		m_Buffs.Remove(buff);
		Update();
		TryRemovePart();
	}

	public void Suppress(BlueprintAbilityGroup group)
	{
		m_Groups.Add(group);
		Update();
	}

	public void Release(BlueprintAbilityGroup group)
	{
		m_Groups.Remove(group);
		Update();
		TryRemovePart();
	}

	private void TryRemovePart()
	{
		if (!m_Buffs.Any())
		{
			base.Owner.Remove<UnitPartBuffSuppress>();
		}
	}

	public bool IsSuppressed(Buff buff)
	{
		if (!m_Buffs.Contains(buff.Blueprint))
		{
			return buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup p) => m_Groups.Contains(p));
		}
		return true;
	}

	private void Update()
	{
		foreach (Buff item in base.Owner.Buffs.RawFacts.ToList())
		{
			bool flag = IsSuppressed(item);
			if (item.IsSuppressed != flag)
			{
				if (flag && item.Active)
				{
					item.Deactivate();
				}
				item.IsSuppressed = flag;
				if (!flag && !item.Active)
				{
					item.Activate();
				}
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
		UnitPartBuffSuppress source = new UnitPartBuffSuppress();
		result = Unsafe.As<UnitPartBuffSuppress, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartBuffSuppress>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartBuffSuppress>();
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
