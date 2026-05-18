using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Squads;
using Owlcat.AI;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Combat/RemoveFromSquad")]
[TypeId("2d4adc7b66ddd0942b595260aeba039c")]
public sealed class RemoveFromSquad : UnitFactComponentDelegate, IParameterizedBehaviourTreeProvider, IBehaviourTreeProvider
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[OwlPackInclude]
		public string SquadId;

		[OwlPackInclude]
		public bool WasLeader;

		[OwlPackInclude]
		public float InitiativeValue;

		[OwlPackInclude]
		public float InitiativeRoll;

		[OwlPackInclude]
		public int InitiativeOrder;

		[OwlPackInclude]
		public int InitiativeLastTurn;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[6]
			{
				new FieldInfo("SquadId", typeof(string)),
				new FieldInfo("WasLeader", typeof(bool)),
				new FieldInfo("InitiativeValue", typeof(float)),
				new FieldInfo("InitiativeRoll", typeof(float)),
				new FieldInfo("InitiativeOrder", typeof(int)),
				new FieldInfo("InitiativeLastTurn", typeof(int))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentData source = new ComponentData();
			result = Unsafe.As<ComponentData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.StringField(0, "SquadId", ref SquadId, state);
			formatter.UnmanagedField(1, "WasLeader", ref WasLeader, state);
			formatter.UnmanagedField(2, "InitiativeValue", ref InitiativeValue, state);
			formatter.UnmanagedField(3, "InitiativeRoll", ref InitiativeRoll, state);
			formatter.UnmanagedField(4, "InitiativeOrder", ref InitiativeOrder, state);
			formatter.UnmanagedField(5, "InitiativeLastTurn", ref InitiativeLastTurn, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentData>();
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
					SquadId = formatter.ReadString(state);
					break;
				case 1:
					WasLeader = formatter.ReadUnmanaged<bool>(state);
					break;
				case 2:
					InitiativeValue = formatter.ReadUnmanaged<float>(state);
					break;
				case 3:
					InitiativeRoll = formatter.ReadUnmanaged<float>(state);
					break;
				case 4:
					InitiativeOrder = formatter.ReadUnmanaged<int>(state);
					break;
				case 5:
					InitiativeLastTurn = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	private ParameterizedBehaviourTree m_ParameterizedBehaviourTree;

	public ParameterizedBehaviourTree ParameterizedBehaviourTree => m_ParameterizedBehaviourTree;

	public BehaviourTreeSerializableData BehaviourTree => m_ParameterizedBehaviourTree.BehaviourTree?.Get();

	protected override void OnActivate()
	{
		PartSquad squadOptional = base.Owner.GetSquadOptional();
		if (squadOptional != null && squadOptional.IsInSquad)
		{
			ComponentData componentData = RequestSavableData<ComponentData>();
			UnitSquad squad = squadOptional.Squad;
			componentData.SquadId = squadOptional.Id;
			componentData.WasLeader = squadOptional.IsLeader;
			componentData.InitiativeValue = squad.Initiative.Value;
			componentData.InitiativeRoll = squad.Initiative.Roll;
			componentData.InitiativeOrder = squad.Initiative.Order;
			componentData.InitiativeLastTurn = squad.Initiative.LastTurn;
			base.Owner.Remove<PartSquad>();
			EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
			{
				h.HandleInitiativeChanged();
			});
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		if (BehaviourTree != null)
		{
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.Register(base.Owner, this);
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.RegisterViewForDebug(base.Owner, this);
		}
	}

	protected override void OnViewDidAttach()
	{
		if (BehaviourTree != null)
		{
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.RegisterViewForDebug(base.Owner, this);
		}
	}

	protected override void OnDeactivate()
	{
		Game.Instance.Controllers.BehaviourTreeTickController.Storage.UnRegister(base.Owner, this);
		ComponentData componentData = RequestSavableData<ComponentData>();
		if (componentData.SquadId != null)
		{
			PartSquad orCreate = base.Owner.GetOrCreate<PartSquad>();
			orCreate.Id = componentData.SquadId;
			if (componentData.WasLeader)
			{
				orCreate.Squad.Leader = base.Owner;
			}
			UnitSquad squad = orCreate.Squad;
			if (squad.Initiative.Empty)
			{
				squad.Initiative.Value = componentData.InitiativeValue;
				squad.Initiative.Roll = componentData.InitiativeRoll;
				squad.Initiative.Order = componentData.InitiativeOrder;
				squad.Initiative.LastTurn = componentData.InitiativeLastTurn;
			}
			base.Owner.Initiative.CopyFrom(squad.Initiative);
			base.Owner.Initiative.LastTurn = squad.Initiative.LastTurn;
			EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
			{
				h.HandleInitiativeChanged();
			});
		}
	}
}
