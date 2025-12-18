using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("a5bdd1358d4c415cb7f6990e1abe6060")]
public class SpawnerGuardSettings : EntityPartComponent<SpawnerGuardSettings.Part>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Part : ViewBasedPart, IUnitInitializer, IHashable, IOwlPackable<Part>
	{
		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Part",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceType", typeof(string))
			}
		};

		public new SpawnerGuardSettings Source => (SpawnerGuardSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			unit.GetOrCreate<UnitPartGuard>().Init(this);
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			if ((bool)Source.ExtendedVisionArea)
			{
				PartVision visionOptional = unit.GetVisionOptional();
				if (visionOptional != null)
				{
					visionOptional.ExtendedVisionArea = Source.ExtendedVisionArea;
				}
			}
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

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
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

	public float Range = 5f;

	public int CoercionBonus;

	public ScriptZone ExtendedVisionArea;

	public bool UseLosInsteadOfVisibility;

	public ActionsReference OnDetect;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		DebugDraw.DrawCircle(base.transform.position, Vector3.up, Range);
	}
}
