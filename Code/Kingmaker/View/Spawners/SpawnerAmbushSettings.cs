using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.View.MapObjects;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("37e99300b8a24a429ee0076686fe7e50")]
public class SpawnerAmbushSettings : EntityPartComponent<SpawnerAmbushSettings.Part>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Part : EntityPartWithConfig, IUnitInitializer, IHashable, IOwlPackable<Part>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Part",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceType", typeof(string))
			}
		};

		public new SpawnerAmbushSettings Source => (SpawnerAmbushSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			PartUnitStealth optional = unit.GetOptional<PartUnitStealth>();
			if (optional != null)
			{
				optional.InAmbush = true;
				optional.AmbushJoinCombatDistance = Source.JoinCombatDistance;
				optional.AmbushTake20 = Source.StealthTake20;
				optional.ForceEnterStealth = true;
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
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
			Part source = new Part();
			result = Unsafe.As<Part, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Part>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceType;
			formatter.StringField(0, "SourceType", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Part>();
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
				}
			}
			formatter.LeaveObject();
		}
	}

	public float JoinCombatDistance = 5f;

	public bool StealthTake20;
}
