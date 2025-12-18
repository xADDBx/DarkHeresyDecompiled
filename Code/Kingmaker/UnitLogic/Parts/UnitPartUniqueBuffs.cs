using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartUniqueBuffs : BaseUnitPart, IAreaHandler, ISubscriber, IHashable, IOwlPackable<UnitPartUniqueBuffs>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<Buff> Buffs = new List<Buff>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartUniqueBuffs",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Buffs", typeof(List<Buff>))
		}
	};

	public void NewBuff(Buff newBuff)
	{
		Buff buff = Buffs.Find((Buff p) => p.Blueprint == newBuff.Blueprint);
		if (buff != null)
		{
			Buffs.Remove(buff);
			buff.Remove();
		}
		Buffs.Add(newBuff);
	}

	public void RemoveBuff(Buff buff)
	{
		Buff buff2 = Buffs.Find((Buff p) => p.Blueprint == buff.Blueprint);
		if (buff2 != null)
		{
			Buffs.Remove(buff2);
		}
	}

	public void OnAreaBeginUnloading()
	{
		Buffs.RemoveAll(delegate(Buff b)
		{
			if (b.Owner.HoldingState != base.Owner.HoldingState)
			{
				b.Remove();
				return true;
			}
			return false;
		});
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Buff> buffs = Buffs;
		if (buffs != null)
		{
			for (int i = 0; i < buffs.Count; i++)
			{
				Hash128 val2 = ClassHasher<Buff>.GetHash128(buffs[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartUniqueBuffs source = new UnitPartUniqueBuffs();
		result = Unsafe.As<UnitPartUniqueBuffs, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartUniqueBuffs>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Buffs", ref Buffs, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartUniqueBuffs>();
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
				Buffs = formatter.ReadPackable<List<Buff>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
