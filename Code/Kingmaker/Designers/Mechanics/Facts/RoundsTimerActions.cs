using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Combat/RoundsTimerActions")]
[TypeId("904cdbeb5bf84eaf89247b99658a8b3b")]
public class RoundsTimerActions : EntityFactComponentDelegate, IRoundStartHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public int RoundsPassed;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("RoundsPassed", typeof(int))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref RoundsPassed);
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
			formatter.UnmanagedField(0, "RoundsPassed", ref RoundsPassed, state);
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
					RoundsPassed = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private enum TickPolicy
	{
		[UsedImplicitly]
		Default,
		TurnBasedOnly,
		RealtimeOnly
	}

	[SerializeField]
	private TickPolicy m_TickPolicy;

	[InfoBox("Wait Delay rounds before run Actions (0 means trigger on first New Round event)")]
	public int Delay;

	[InfoBox("Run Actions every Loop rounds after Delay rounds (0 means never repeat)")]
	public int Loop;

	public ActionList Actions;

	public bool CanTickInTurnBased => m_TickPolicy != TickPolicy.RealtimeOnly;

	public bool CanTickRealtime => m_TickPolicy != TickPolicy.TurnBasedOnly;

	private bool CanTick(bool isTurnBased)
	{
		if (!isTurnBased || !CanTickInTurnBased)
		{
			if (!isTurnBased)
			{
				return CanTickRealtime;
			}
			return false;
		}
		return true;
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		if (CanTick(isTurnBased) && (!(base.Owner is MechanicEntity mechanicEntity) || mechanicEntity.Initiative.Empty))
		{
			NextRound();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (CanTick(isTurnBased) && base.Owner is MechanicEntity mechanicEntity && !mechanicEntity.Initiative.Empty && base.Owner == EventInvokerExtensions.MechanicEntity)
		{
			NextRound();
		}
	}

	private void NextRound()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		savableData.RoundsPassed++;
		if (savableData.RoundsPassed == Delay + 1 || (savableData.RoundsPassed > Delay && Loop > 0 && (savableData.RoundsPassed - Delay - 1) % Loop == 0))
		{
			using (EvalContext.PushContextMaybe(base.Fact?.MaybeContext))
			{
				Actions.Run();
			}
		}
	}
}
