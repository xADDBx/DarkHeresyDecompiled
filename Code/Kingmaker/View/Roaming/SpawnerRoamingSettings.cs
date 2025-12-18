using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Spawners;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("35323d72d20e37f4ab636e75ceeadda3")]
public class SpawnerRoamingSettings : EntityPartComponent<SpawnerRoamingSettings.Part>
{
	public enum ModeType
	{
		Patrol,
		Radius,
		Cutscene
	}

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

		public new SpawnerRoamingSettings Source => (SpawnerRoamingSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			UnitPartRoaming orCreate = unit.GetOrCreate<UnitPartRoaming>();
			if (Source.Mode == ModeType.Patrol && Source.FirstWaypoint?.FindData() is RoamingWaypointData nextPoint)
			{
				orCreate.NextPoint = nextPoint;
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			UnitPartRoaming orCreate = unit.GetOrCreate<UnitPartRoaming>();
			UnitPartRoaming unitPartRoaming = orCreate;
			if (unitPartRoaming.Settings == null)
			{
				RoamingUnitSettings roamingUnitSettings2 = (unitPartRoaming.Settings = new RoamingUnitSettings());
			}
			orCreate.Settings.MovementType = Source.MovementType;
			orCreate.Settings.MovementSpeed = Source.MovementSpeed;
			orCreate.Settings.Sleepless = Source.Sleepless;
			orCreate.OriginalPoint = Source.transform.position;
			switch (Source.Mode)
			{
			case ModeType.Radius:
				orCreate.Settings.Radius = Source.Radius;
				orCreate.Settings.MinIdleTime = Source.MinIdleTime;
				orCreate.Settings.MaxIdleTime = Source.MaxIdleTime;
				break;
			case ModeType.Cutscene:
				orCreate.Settings.SetCutscenes(Source.IdleCutscenes);
				orCreate.Settings.MinIdleTime = Source.MinIdleTime;
				orCreate.Settings.MaxIdleTime = Source.MaxIdleTime;
				break;
			}
			if (Source.Sleepless)
			{
				unit.Sleepless.Retain();
			}
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
			unit.GetOptional<UnitPartRoaming>()?.IdleCutscene?.Stop();
			unit.Remove<UnitPartRoaming>();
			if (Source.Sleepless)
			{
				unit.Sleepless.Release();
			}
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

	public ModeType Mode;

	[AllowedEntityType(typeof(RoamingWaypointView))]
	[ShowIf("IsPatrolMode")]
	public EntityReference FirstWaypoint;

	[ShowIf("IsRadiusMode")]
	public float Radius = 1f;

	[ShowIf("IsCutsceneMode")]
	public CutsceneReference[] IdleCutscenes = new CutsceneReference[0];

	[ShowIf("IsRadiusOrCutsceneMode")]
	public float MinIdleTime;

	[ShowIf("IsRadiusOrCutsceneMode")]
	public float MaxIdleTime;

	public WalkSpeedType MovementType = WalkSpeedType.Walk;

	public float MovementSpeed;

	public bool Sleepless;

	private bool IsPatrolMode => Mode == ModeType.Patrol;

	private bool IsRadiusMode => Mode == ModeType.Radius;

	private bool IsCutsceneMode => Mode == ModeType.Cutscene;

	private bool IsRadiusOrCutsceneMode
	{
		get
		{
			ModeType mode = Mode;
			return mode == ModeType.Radius || mode == ModeType.Cutscene;
		}
	}
}
