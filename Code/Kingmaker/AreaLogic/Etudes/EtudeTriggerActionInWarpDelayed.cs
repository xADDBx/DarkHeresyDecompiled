using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("d7c3d80beb354d44ab46ace48fc5bf75")]
public class EtudeTriggerActionInWarpDelayed : BlueprintComponent
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public int WarpTravelStartCount;

		[JsonProperty]
		[OwlPackInclude]
		public bool IsCompleted;

		[JsonProperty]
		[OwlPackInclude]
		public bool IsReadyToTrigger;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("WarpTravelStartCount", typeof(int)),
				new FieldInfo("IsCompleted", typeof(bool)),
				new FieldInfo("IsReadyToTrigger", typeof(bool))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref WarpTravelStartCount);
			result.Append(ref IsCompleted);
			result.Append(ref IsReadyToTrigger);
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
			formatter.UnmanagedField(0, "WarpTravelStartCount", ref WarpTravelStartCount, state);
			formatter.UnmanagedField(1, "IsCompleted", ref IsCompleted, state);
			formatter.UnmanagedField(2, "IsReadyToTrigger", ref IsReadyToTrigger, state);
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
					WarpTravelStartCount = formatter.ReadUnmanaged<int>(state);
					break;
				case 1:
					IsCompleted = formatter.ReadUnmanaged<bool>(state);
					break;
				case 2:
					IsReadyToTrigger = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public enum TimeToStartAction
	{
		AfterTravelStart,
		AfterTravelFinished
	}

	public enum EventType
	{
		OncePerTravel,
		MechanicEvent,
		SimpleEvent
	}

	[Tooltip("How much warp travels should pass for ActionList to be invoked")]
	public int WarpTravelTriggerCount;

	[Tooltip("Actions to invoke after required amount of warp travel has passed")]
	public ActionList ActionList;

	[SerializeField]
	public EventType TriggerType;

	[Tooltip("When to invoke actions relatively to last warp travel")]
	[ShowIf("IsOncePerTravel")]
	public TimeToStartAction TimeToStart = TimeToStartAction.AfterTravelFinished;

	[SerializeField]
	[ShowIf("IsOncePerTravel")]
	[Tooltip("The greater number - the higher priority. Priority of deadly encounters is 20")]
	public int Priority = 1;

	private bool IsOncePerTravel => TriggerType == EventType.OncePerTravel;
}
