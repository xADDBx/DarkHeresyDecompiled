using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[ComponentName("Combat/Turn/OutOfCasterTurnFactActivator")]
[TypeId("c7894d09190e4f30b6455a035db59a28")]
public class OutOfCasterTurnFactActivator : MechanicEntityFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public string FactId;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("FactId", typeof(string))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(FactId);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			SavableData source = new SavableData();
			result = Unsafe.As<SavableData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<SavableData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.StringField(0, "FactId", ref FactId, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SavableData>();
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
					FactId = formatter.ReadString(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_FactBlueprint;

	public BlueprintMechanicEntityFact FactBlueprint => m_FactBlueprint;

	protected override void OnActivate()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (turnController != null && turnController.TurnBasedModeActive && turnController.CurrentUnit != base.Context.MaybeCaster)
		{
			UpdateFact(add: true);
		}
	}

	protected override void OnDeactivate()
	{
		UpdateFact(add: false);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			UpdateFact(EventInvokerExtensions.MechanicEntity != base.Context.MaybeCaster);
		}
	}

	private void UpdateFact(bool add)
	{
		MechanicEntity owner = base.Owner;
		SavableData savableData = RequestSavableData<SavableData>();
		if (add && savableData.FactId == null)
		{
			MechanicEntityFact mechanicEntityFact = FactBlueprint.CreateFact(base.Context, default(BuffDuration));
			owner.Facts.Add(mechanicEntityFact);
			savableData.FactId = mechanicEntityFact.UniqueId;
		}
		else if (!add && savableData.FactId != null)
		{
			owner.Facts.RemoveById(savableData.FactId);
			savableData.FactId = null;
		}
	}
}
