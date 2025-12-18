using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[ComponentName("Events/EtudeClearCutscenesTrigger")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[AllowedOn(typeof(BlueprintComponentList))]
[TypeId("a2541639abc3992409b7b041092cccb8")]
public class EtudeClearCutscenesTrigger : EtudeBracketTrigger, IEtudesUpdateHandler, ISubscriber, IAreaActivationHandler
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public bool AlreadyProcessedActivation;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("AlreadyProcessedActivation", typeof(bool))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref AlreadyProcessedActivation);
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
			formatter.UnmanagedField(0, "AlreadyProcessedActivation", ref AlreadyProcessedActivation, state);
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
					AlreadyProcessedActivation = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	protected override void OnActivate()
	{
		RequestSavableData<SavableData>().AlreadyProcessedActivation = false;
	}

	private void MaybeTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (savableData.AlreadyProcessedActivation || Game.Instance.EtudesSystem.GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).Any((BlueprintAreaMechanics mechanics) => !mechanics.IsSceneLoadedNow()))
		{
			return;
		}
		savableData.AlreadyProcessedActivation = true;
		foreach (CutscenePlayerData cutscene in Game.Instance.EntityPools.Cutscenes)
		{
			if (!cutscene.HasActiveLockControl && HasPartyUnitControlled(cutscene))
			{
				cutscene.Stop();
			}
		}
	}

	private static bool HasPartyUnitControlled(CutscenePlayerData cutscene)
	{
		foreach (AbstractUnitEntity currentControlledUnit in cutscene.GetCurrentControlledUnits())
		{
			if (currentControlledUnit is BaseUnitEntity baseUnitEntity && baseUnitEntity.CombatGroup.IsPlayerParty)
			{
				return true;
			}
		}
		return false;
	}

	public void OnEtudesUpdate()
	{
		MaybeTrigger();
	}

	public void OnAreaActivated()
	{
		MaybeTrigger();
	}
}
