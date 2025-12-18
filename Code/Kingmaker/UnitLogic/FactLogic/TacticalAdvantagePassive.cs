using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("77f8a0667de149a5bd6c94dd58b0c81a")]
public class TacticalAdvantagePassive : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Data : IEntityFactComponentSavableData, IHashable, IOwlPackable<Data>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Data",
			OldNames = null,
			Fields = new FieldInfo[0]
		};

		public int MomentumThisCombat { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Data source = new Data();
			result = Unsafe.As<Data, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Data>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Data>();
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

	public int Percent;

	[SerializeField]
	private BlueprintBuffReference m_LinchpinBuff;

	[SerializeField]
	private BlueprintBuffReference m_TacticalAdvantageBuff;

	public StatType LinchpinStat;

	public int LinchpinStatBonus;

	public int LinchpinBaseBonus;

	[SerializeField]
	private BlueprintUnitFactReference m_ComfortInConformityFact;

	public int ComfortInConformityHeal;

	public BlueprintBuff LinchpinBuff => m_LinchpinBuff?.Get();

	public BlueprintBuff TacticalAdvantageBuff => m_TacticalAdvantageBuff?.Get();

	public BlueprintUnitFact ComfortInConformityFact => m_ComfortInConformityFact?.Get();

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RequestSavableData<Data>().MomentumThisCombat = 0;
		}
	}
}
