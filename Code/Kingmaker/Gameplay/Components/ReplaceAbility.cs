using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("28d4f985ff1f47bf815c1bc8356067b2")]
public class ReplaceAbility : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public sealed class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public EntityFactRef<Ability> ReplacementAbility;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("ReplacementAbility", typeof(EntityFactRef<Ability>))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			EntityFactRef<Ability> obj = ReplacementAbility;
			Hash128 val2 = StructHasher<EntityFactRef<Ability>>.GetHash128(ref obj);
			result.Append(ref val2);
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
			formatter.Field(0, "ReplacementAbility", ref ReplacementAbility, state);
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
					ReplacementAbility = formatter.ReadPackable<EntityFactRef<Ability>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[ValidateNotNull]
	public BpRef<BlueprintAbility> TargetAbility;

	[ValidateNotNull]
	public BpRef<BlueprintAbility> ReplacementAbility;

	protected override void OnActivate()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		Ability ability = base.Owner.Abilities.Add(ReplacementAbility);
		ability.Hide(base.Fact);
		componentData.ReplacementAbility = ability;
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityReplacements>().Add(base.Fact, TargetAbility, RequestSavableData<ComponentData>().ReplacementAbility);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityReplacements>()?.Remove(base.Fact, TargetAbility);
		ComponentData componentData = RequestSavableData<ComponentData>();
		base.Owner.Abilities.Remove((Ability)componentData.ReplacementAbility);
	}
}
