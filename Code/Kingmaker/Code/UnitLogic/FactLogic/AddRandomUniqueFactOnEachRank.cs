using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("e40a52710a2d44ddab5083812e6d458d")]
public class AddRandomUniqueFactOnEachRank : MechanicEntityFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("AddedFacts", typeof(List<EntityFactRef<MechanicEntityFact>>))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public List<EntityFactRef<MechanicEntityFact>> AddedFacts { get; private set; } = new List<EntityFactRef<MechanicEntityFact>>();


		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<EntityFactRef<MechanicEntityFact>> addedFacts = AddedFacts;
			if (addedFacts != null)
			{
				for (int i = 0; i < addedFacts.Count; i++)
				{
					EntityFactRef<MechanicEntityFact> obj = addedFacts[i];
					Hash128 val2 = StructHasher<EntityFactRef<MechanicEntityFact>>.GetHash128(ref obj);
					result.Append(ref val2);
				}
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
			List<EntityFactRef<MechanicEntityFact>> value = AddedFacts;
			formatter.Field(0, "AddedFacts", ref value, state);
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
					AddedFacts = formatter.ReadPackable<List<EntityFactRef<MechanicEntityFact>>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_Facts;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	private int Rank => base.Fact.GetRank();

	protected override void OnActivateOrPostLoad()
	{
		Update();
	}

	protected override void OnFactDetached()
	{
		RequestSavableData<ComponentData>().AddedFacts.RemoveAll(delegate(EntityFactRef<MechanicEntityFact> i)
		{
			base.Owner.Facts.Remove(i.Fact);
			return true;
		});
	}

	private void Update()
	{
		MechanicEntity concreteOwner = base.Owner;
		ComponentData componentData = RequestSavableData<ComponentData>();
		componentData.AddedFacts.RemoveAll(delegate(EntityFactRef<MechanicEntityFact> i)
		{
			int num;
			if (i.Fact != null)
			{
				num = ((!Facts.HasReference(i.Fact.Blueprint)) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0048;
				}
			}
			else
			{
				num = 1;
			}
			concreteOwner.Facts.Remove(i.Fact);
			goto IL_0048;
			IL_0048:
			return (byte)num != 0;
		});
		while (componentData.AddedFacts.Count > 0 && componentData.AddedFacts.Count > Rank)
		{
			List<EntityFactRef<MechanicEntityFact>> addedFacts = componentData.AddedFacts;
			EntityFactRef<MechanicEntityFact> entityFactRef = addedFacts[addedFacts.Count - 1];
			concreteOwner.Facts.Remove((MechanicEntityFact)entityFactRef);
			componentData.AddedFacts.Remove(entityFactRef);
		}
		while (componentData.AddedFacts.Count < Rank)
		{
			BlueprintMechanicEntityFact blueprintMechanicEntityFact = Facts.Where((BlueprintMechanicEntityFact i) => !concreteOwner.Facts.Contains(i)).Random(PFStatefulRandom.Mechanics);
			if (blueprintMechanicEntityFact != null)
			{
				MechanicEntityFact mechanicEntityFact = concreteOwner.Facts.Add(blueprintMechanicEntityFact.CreateFact(base.Context, default(BuffDuration)));
				if (mechanicEntityFact != null)
				{
					mechanicEntityFact.AddSource(base.Fact, this);
					componentData.AddedFacts.Add(mechanicEntityFact);
					continue;
				}
				break;
			}
			break;
		}
	}
}
