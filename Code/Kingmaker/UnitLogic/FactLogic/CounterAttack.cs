using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("21dbbb0b719a4ce38c2152bf9ea6bbce")]
public class CounterAttack : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public int? UsageLimit;

		[JsonProperty]
		[OwlPackInclude]
		public int UsageCount;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("UsageLimit", typeof(int?)),
				new FieldInfo("UsageCount", typeof(int))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			if (UsageLimit.HasValue)
			{
				int val2 = UsageLimit.Value;
				result.Append(ref val2);
			}
			result.Append(ref UsageCount);
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
			formatter.UnmanagedNullableField(0, "UsageLimit", ref UsageLimit, state);
			formatter.UnmanagedField(1, "UsageCount", ref UsageCount, state);
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
					UsageLimit = formatter.ReadNullableUnmanaged<int>(state);
					break;
				case 1:
					UsageCount = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public enum TriggerType
	{
		AfterParryAttack,
		AfterAnyAttack
	}

	public TriggerType Trigger;

	public bool GuardAllies;

	[ShowIf("GuardAllies")]
	public ContextValue GuardAlliesRange;

	public bool Limited;

	[ShowIf("Limited")]
	public ContextValue UsageLimit;

	public bool CanUseInRange;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartCounterAttack>().Add(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartCounterAttack>()?.Remove(base.Fact, this);
	}
}
