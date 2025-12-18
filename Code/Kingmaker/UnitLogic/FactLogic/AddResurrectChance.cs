using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("47f44a33d13b4aab8e2a36070bc24f5f")]
public class AddResurrectChance : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ResurrectChanceUnitPart : BaseUnitPart, IHashable, IOwlPackable<ResurrectChanceUnitPart>
	{
		public int ResurrectChance;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ResurrectChanceUnitPart",
			OldNames = null,
			Fields = new FieldInfo[0]
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
			ResurrectChanceUnitPart source = new ResurrectChanceUnitPart();
			result = Unsafe.As<ResurrectChanceUnitPart, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ResurrectChanceUnitPart>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ResurrectChanceUnitPart>();
			List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
			formatter.EnterObject();
			for (int i = 0; i < typeInfo.Fields.Length; i++)
			{
				formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
				if (mappingForType[fieldID] == byte.MaxValue)
				{
					formatter.SkipField(size);
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	private ContextValue m_ResurrectChance;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<ResurrectChanceUnitPart>().ResurrectChance = m_ResurrectChance.Calculate(base.Context);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<ResurrectChanceUnitPart>();
	}
}
