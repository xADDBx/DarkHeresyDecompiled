using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Weakpoints;

[Serializable]
[TypeId("f1b05c3405e34617a93206a1b4af8938")]
public sealed class Weakpoint : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public WeakpointSide? ChosenSide;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("ChosenSide", typeof(WeakpointSide?))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			if (ChosenSide.HasValue)
			{
				WeakpointSide val2 = ChosenSide.Value;
				result.Append(ref val2);
			}
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
			formatter.EnumNullableField(0, "ChosenSide", ref ChosenSide, state);
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
					ChosenSide = formatter.ReadNullableEnum<WeakpointSide>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public WeakpointSideSelector Side;

	protected override void OnActivateOrPostLoad()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		WeakpointSide valueOrDefault = componentData.ChosenSide.GetValueOrDefault();
		WeakpointSide num;
		if (!componentData.ChosenSide.HasValue)
		{
			valueOrDefault = Side.Select(base.Context.MaybeCaster, base.Owner);
			componentData.ChosenSide = valueOrDefault;
			num = valueOrDefault;
		}
		else
		{
			num = valueOrDefault;
		}
		WeakpointSide side = num;
		base.Owner.GetOrCreate<PartWeakpoints>().Add(side, base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartWeakpoints>()?.Remove(base.Fact, this);
	}
}
