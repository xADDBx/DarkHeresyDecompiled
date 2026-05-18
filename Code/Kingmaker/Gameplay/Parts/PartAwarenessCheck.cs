using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.View.MapObjects.Traps;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAwarenessCheck : EntityPartWithConfig<AwarenessCheckSettings>, IHashable, IOwlPackable<PartAwarenessCheck>
{
	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<UnitReference, int> LastAwarenessValue = new Dictionary<UnitReference, int>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAwarenessCheck",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsPassed", typeof(bool)),
			new FieldInfo("IsRevealedByFlashlight", typeof(bool)),
			new FieldInfo("LastAwarenessValue", typeof(Dictionary<UnitReference, int>))
		}
	};

	public new MapObjectEntity Owner => (MapObjectEntity)base.Owner;

	[JsonProperty]
	[OwlPackInclude]
	private bool IsPassed { get; set; }

	public bool GetIsPassed => IsPassed;

	[JsonProperty]
	[OwlPackInclude]
	public bool IsRevealedByFlashlight { get; set; }

	public bool IsCheckAllowedFor([NotNull] BaseUnitEntity unit)
	{
		if ((Owner is TrapObjectData { ScriptZone: not null, TrappedObject: { } trappedObject } && (!trappedObject.IsInGame || !trappedObject.IsAwarenessCheckPassed)) ? true : false)
		{
			return false;
		}
		if (LastAwarenessValue.TryGetValue(unit.FromBaseUnitEntity(), out var value))
		{
			return value < (int)unit.Actor.GetStat(StatType.SkillAwareness, null, default(StatContext), "IsCheckAllowedFor");
		}
		return true;
	}

	public void Reset()
	{
		IsPassed = false;
		IsRevealedByFlashlight = false;
		LastAwarenessValue.Clear();
		Owner.IsRevealed = false;
		base.EventBus.RaiseEvent((IMapObjectEntity)Owner, (Action<IResetAwarenessHandler>)delegate(IResetAwarenessHandler h)
		{
			h.HandleAwarenessCheckReset();
		}, isCheckRuntime: true);
	}

	public void SetIsPassed(bool isPassed)
	{
		IsPassed = isPassed;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsPassed;
		result.Append(ref val2);
		bool val3 = IsRevealedByFlashlight;
		result.Append(ref val3);
		Dictionary<UnitReference, int> lastAwarenessValue = LastAwarenessValue;
		if (lastAwarenessValue != null)
		{
			int val4 = 0;
			foreach (KeyValuePair<UnitReference, int> item in lastAwarenessValue)
			{
				Hash128 hash = default(Hash128);
				UnitReference obj = item.Key;
				Hash128 val5 = UnitReferenceHasher.GetHash128(ref obj);
				hash.Append(ref val5);
				int obj2 = item.Value;
				Hash128 val6 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash.Append(ref val6);
				val4 ^= hash.GetHashCode();
			}
			result.Append(ref val4);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartAwarenessCheck source = new PartAwarenessCheck();
		result = Unsafe.As<PartAwarenessCheck, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAwarenessCheck>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = IsPassed;
		formatter.UnmanagedField(1, "IsPassed", ref value2, state);
		bool value3 = IsRevealedByFlashlight;
		formatter.UnmanagedField(2, "IsRevealedByFlashlight", ref value3, state);
		formatter.Field(3, "LastAwarenessValue", ref LastAwarenessValue, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAwarenessCheck>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				IsPassed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				IsRevealedByFlashlight = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				LastAwarenessValue = formatter.ReadPackable<Dictionary<UnitReference, int>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
