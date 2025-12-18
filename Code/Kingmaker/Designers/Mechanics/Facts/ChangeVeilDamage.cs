using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c140f075f8064f4191a3cc3d6e0dc510")]
public class ChangeVeilDamage : UnitFactComponentDelegate, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInitiatorRulebookHandler<RuleCalculateVeilDamage>, IRulebookHandler<RuleCalculateVeilDamage>, IInitiatorRulebookSubscriber
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

		public int UsedThisRound { get; set; }

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

	[HideIf("ReduceToZero")]
	public ContextValue VeilDamageIncrease = 0;

	public bool OnlyFirstPowerEachTurn;

	public bool ReduceToZero;

	public bool HasRandomChance;

	[ShowIf("HasRandomChance")]
	public ContextValue RandomChance;

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		base.Fact.RequestSavableData<Data>(this);
	}

	public int GetChange(AbilityData ability, bool isPrediction = false)
	{
		if (ability.Fact == null || (!Groups.Empty() && !Groups.Any((BlueprintAbilityGroup p) => ability.AbilityGroups.Contains(p))))
		{
			return 0;
		}
		Data data = base.Fact.RequestSavableData<Data>(this);
		if (OnlyFirstPowerEachTurn && data.UsedThisRound > 0)
		{
			return 0;
		}
		if (HasRandomChance)
		{
			if (isPrediction)
			{
				return 0;
			}
			if (Rulebook.Trigger(new RuleRollD100(base.Owner)).Result > RandomChance.Calculate(base.Context))
			{
				return 0;
			}
		}
		if (ReduceToZero)
		{
			return -ability.Blueprint.VeilDamage;
		}
		return VeilDamageIncrease.Calculate(base.Context);
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			RequestSavableData<Data>().UsedThisRound = 0;
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateVeilDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateVeilDamage evt)
	{
		RequestSavableData<Data>().UsedThisRound++;
	}
}
