using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d841152a8173c9e42ad26cacbed8a5f8")]
public class SetChargeWeapon : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Runtime : EntityFactComponent<BaseUnitEntity, SetChargeWeapon>, IHashable, IOwlPackable<Runtime>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Runtime",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceBlueprintComponentName", typeof(string))
			}
		};

		protected override void OnActivateOrPostLoad()
		{
			base.Owner.GetOrCreate<UnitPartChargeWeapon>().Set(base.SourceBlueprintComponent.Weapon);
		}

		protected override void OnDeactivate()
		{
			base.Owner.Remove<UnitPartChargeWeapon>();
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Runtime source = new Runtime();
			result = Unsafe.As<Runtime, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Runtime>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceBlueprintComponentName;
			formatter.StringField(0, "SourceBlueprintComponentName", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Runtime>();
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
					base.SourceBlueprintComponentName = formatter.ReadString(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	private BlueprintItemWeaponReference m_Weapon;

	public BlueprintItemWeapon Weapon => m_Weapon?.Get();

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
