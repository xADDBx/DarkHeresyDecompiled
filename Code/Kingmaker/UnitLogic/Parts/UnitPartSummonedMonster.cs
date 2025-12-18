using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartSummonedMonster : BaseUnitPart, IHashable, IOwlPackable<UnitPartSummonedMonster>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_Summoner;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartSummonedMonster",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Summoner", typeof(EntityRef<MechanicEntity>))
		}
	};

	[CanBeNull]
	public MechanicEntity Summoner => m_Summoner;

	[CanBeNull]
	public UnitCommandHandle MoveTo { get; set; }

	public bool IsLinkedToSummoner => m_Summoner.Id != base.Owner.UniqueId;

	public void Init([NotNull] MechanicEntity summoner)
	{
		if (Summoner != null)
		{
			PFLog.Default.Error("Double initialization of UnitPartSummonedMonster");
		}
		m_Summoner = summoner;
	}

	protected override void OnPostLoad()
	{
		if (base.Owner.LifeState.IsDead)
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(base.Owner);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<MechanicEntity> obj = m_Summoner;
		Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartSummonedMonster source = new UnitPartSummonedMonster();
		result = Unsafe.As<UnitPartSummonedMonster, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartSummonedMonster>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Summoner", ref m_Summoner, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartSummonedMonster>();
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
				m_Summoner = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
