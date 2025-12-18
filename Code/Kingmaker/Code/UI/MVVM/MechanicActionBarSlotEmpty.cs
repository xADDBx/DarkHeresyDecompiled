using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Mechanics.Facts;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[OwlPackable(OwlPackableMode.Generate)]
public class MechanicActionBarSlotEmpty : MechanicActionBarSlot, IHashable, IOwlPackable<MechanicActionBarSlotEmpty>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicActionBarSlotEmpty",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>))
		}
	};

	public override MechanicEntityFact AbilityFact => null;

	public override string KeyName => null;

	public override bool IsPossibleActive => false;

	public override void OnClick()
	{
		UISounds.Instance.Sounds.Combat.ActionBarCanNotSlotClick.Play();
	}

	public override int GetResource()
	{
		return -1;
	}

	public override int GetResourceCost()
	{
		return -1;
	}

	public override int GetResourceAmount()
	{
		return -1;
	}

	public override Sprite GetIcon()
	{
		return null;
	}

	public override bool NeedUpdate()
	{
		return false;
	}

	public override string GetTitle()
	{
		return string.Empty;
	}

	public override string GetDescription()
	{
		return string.Empty;
	}

	public override bool IsCasting()
	{
		return false;
	}

	public override object GetContentData()
	{
		return null;
	}

	protected override bool CanUseIfTurnBased()
	{
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
		MechanicActionBarSlotEmpty source = new MechanicActionBarSlotEmpty();
		result = Unsafe.As<MechanicActionBarSlotEmpty, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicActionBarSlotEmpty>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicActionBarSlotEmpty>();
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
				m_UnitRef = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
