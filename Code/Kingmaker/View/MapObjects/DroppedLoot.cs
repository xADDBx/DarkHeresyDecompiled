using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(InteractionLoot))]
[KnowledgeDatabaseID("e543a12d94400d9448819b9e7206cf65")]
public class DroppedLoot : MapObjectView, IResource
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class EntityPartBreathOfMoney : EntityPart<EntityData>, IAreaHandler, ISubscriber, IHashable, IOwlPackable<EntityPartBreathOfMoney>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "EntityPartBreathOfMoney",
			OldNames = null,
			Fields = new FieldInfo[0]
		};

		public void OnAreaBeginUnloading()
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(base.Owner);
		}

		public void OnAreaDidLoad()
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
			EntityPartBreathOfMoney source = new EntityPartBreathOfMoney();
			result = Unsafe.As<EntityPartBreathOfMoney, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<EntityPartBreathOfMoney>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityPartBreathOfMoney>();
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

	[OwlPackable(OwlPackableMode.Generate)]
	public new class EntityData : MapObjectEntity, IHashable, IOwlPackable<EntityData>
	{
		[JsonProperty]
		[OwlPackInclude]
		private Vector3 m_SavedPosition;

		[JsonProperty]
		[OwlPackInclude]
		private EntityRef<Entity> m_DroppedBy;

		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "EntityData",
			OldNames = null,
			Fields = new FieldInfo[23]
			{
				new FieldInfo("UniqueId", typeof(string)),
				new FieldInfo("m_IsInGame", typeof(bool)),
				new FieldInfo("m_Position", typeof(Vector3)),
				new FieldInfo("m_Orientation", typeof(float)),
				new FieldInfo("m_InitialPosition", typeof(Vector3?)),
				new FieldInfo("m_InitialOrientation", typeof(float?)),
				new FieldInfo("Facts", typeof(EntityFactsManager)),
				new FieldInfo("Parts", typeof(EntityPartsManager)),
				new FieldInfo("m_IsRevealed", typeof(bool)),
				new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
				new FieldInfo("m_Initiative", typeof(Initiative)),
				new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
				new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
				new FieldInfo("MainFact", typeof(MechanicEntityFact)),
				new FieldInfo("WasHighlightedOnRevealAndNoticed", typeof(bool)),
				new FieldInfo("ViewSettings", typeof(MapObjectViewSettings)),
				new FieldInfo("IsNewInGame", typeof(bool)),
				new FieldInfo("m_SavedPosition", typeof(Vector3)),
				new FieldInfo("m_DroppedBy", typeof(EntityRef<Entity>)),
				new FieldInfo("BloodType", typeof(BloodType)),
				new FieldInfo("SurfaceType", typeof(SurfaceType)),
				new FieldInfo("IsDismember", typeof(bool)),
				new FieldInfo("IsDroppedByPlayer", typeof(bool))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public BloodType BloodType { get; set; }

		[JsonProperty]
		[OwlPackInclude]
		public SurfaceType SurfaceType { get; set; }

		[JsonProperty]
		[OwlPackInclude]
		public bool IsDismember { get; set; }

		[JsonProperty]
		[OwlPackInclude]
		public bool IsDroppedByPlayer { get; set; }

		public override bool SetTransformFromConfigOnLoad => false;

		public ItemsCollection Loot
		{
			get
			{
				return GetOrCreate<InteractionLootPart>()?.Loot;
			}
			set
			{
				GetOrCreate<InteractionLootPart>().Loot = value;
			}
		}

		public EntityRef<Entity> DroppedBy
		{
			get
			{
				return m_DroppedBy;
			}
			set
			{
				IsDroppedByPlayer = (value.Entity?.GetOptional<PartFaction>()?.IsPlayer).GetValueOrDefault();
				m_DroppedBy = value;
			}
		}

		public override ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy => ViewHandlingOnDisposePolicyType.FadeOutAndDestroy;

		public EntityData(MapObjectView mapObjectView)
			: base(mapObjectView)
		{
		}

		public EntityData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected EntityData()
		{
		}

		protected override void OnPreSave()
		{
			base.OnPreSave();
			m_SavedPosition = base.View.ViewTransform.position;
		}

		protected override IEntityViewBase CreateViewForData()
		{
			return Object.Instantiate(GetPrefab(), m_SavedPosition, Quaternion.identity);
		}

		private EntityViewBase GetPrefab()
		{
			EntityViewBase entityViewBase = null;
			if (IsDismember)
			{
				entityViewBase = ConfigRoot.Instance.HitSystemRoot.GetDismemberLoot(SurfaceType);
			}
			if ((bool)GetOptional<EntityPartBreathOfMoney>())
			{
				entityViewBase = ConfigRoot.Instance.Prefabs.BreathOfMoneyLootBag;
			}
			if (entityViewBase == null)
			{
				entityViewBase = ConfigRoot.Instance.Prefabs.DroppedLootBag;
			}
			return entityViewBase;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref m_SavedPosition);
			EntityRef<Entity> obj = m_DroppedBy;
			Hash128 val2 = StructHasher<EntityRef<Entity>>.GetHash128(ref obj);
			result.Append(ref val2);
			BloodType val3 = BloodType;
			result.Append(ref val3);
			SurfaceType val4 = SurfaceType;
			result.Append(ref val4);
			bool val5 = IsDismember;
			result.Append(ref val5);
			bool val6 = IsDroppedByPlayer;
			result.Append(ref val6);
			return result;
		}

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			EntityData source = new EntityData();
			result = Unsafe.As<EntityData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<EntityData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.UniqueId;
			formatter.StringField(0, "UniqueId", ref value, state);
			formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
			formatter.Field(2, "m_Position", ref m_Position, state);
			formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
			formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
			formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
			formatter.Field(6, "Facts", ref Facts, state);
			formatter.Field(7, "Parts", ref Parts, state);
			formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
			formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
			formatter.Field(10, "m_Initiative", ref m_Initiative, state);
			formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
			formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
			MechanicEntityFact value2 = base.MainFact;
			formatter.Field(13, "MainFact", ref value2, state);
			bool value3 = base.WasHighlightedOnRevealAndNoticed;
			formatter.UnmanagedField(14, "WasHighlightedOnRevealAndNoticed", ref value3, state);
			MapObjectViewSettings value4 = base.ViewSettings;
			formatter.Field(15, "ViewSettings", ref value4, state);
			bool value5 = base.IsNewInGame;
			formatter.UnmanagedField(16, "IsNewInGame", ref value5, state);
			formatter.Field(17, "m_SavedPosition", ref m_SavedPosition, state);
			formatter.Field(18, "m_DroppedBy", ref m_DroppedBy, state);
			BloodType value6 = BloodType;
			formatter.EnumField(19, "BloodType", ref value6, state);
			SurfaceType value7 = SurfaceType;
			formatter.EnumField(20, "SurfaceType", ref value7, state);
			bool value8 = IsDismember;
			formatter.UnmanagedField(21, "IsDismember", ref value8, state);
			bool value9 = IsDroppedByPlayer;
			formatter.UnmanagedField(22, "IsDroppedByPlayer", ref value9, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityData>();
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
					base.UniqueId = formatter.ReadString(state);
					break;
				case 1:
					m_IsInGame = formatter.ReadUnmanaged<bool>(state);
					break;
				case 2:
					m_Position = formatter.ReadPackable<Vector3>(state);
					break;
				case 3:
					m_Orientation = formatter.ReadUnmanaged<float>(state);
					break;
				case 4:
					m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
					break;
				case 5:
					m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
					break;
				case 6:
					Facts = formatter.ReadPackable<EntityFactsManager>(state);
					break;
				case 7:
					Parts = formatter.ReadPackable<EntityPartsManager>(state);
					break;
				case 8:
					m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
					break;
				case 9:
					m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
					break;
				case 10:
					m_Initiative = formatter.ReadPackable<Initiative>(state);
					break;
				case 11:
					m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
					break;
				case 12:
					m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
					break;
				case 13:
					base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
					break;
				case 14:
					base.WasHighlightedOnRevealAndNoticed = formatter.ReadUnmanaged<bool>(state);
					break;
				case 15:
					base.ViewSettings = formatter.ReadPackable<MapObjectViewSettings>(state);
					break;
				case 16:
					base.IsNewInGame = formatter.ReadUnmanaged<bool>(state);
					break;
				case 17:
					m_SavedPosition = formatter.ReadPackable<Vector3>(state);
					break;
				case 18:
					m_DroppedBy = formatter.ReadPackable<EntityRef<Entity>>(state);
					break;
				case 19:
					BloodType = formatter.ReadEnum<BloodType>(state);
					break;
				case 20:
					SurfaceType = formatter.ReadEnum<SurfaceType>(state);
					break;
				case 21:
					IsDismember = formatter.ReadUnmanaged<bool>(state);
					break;
				case 22:
					IsDroppedByPlayer = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public ItemsCollection Loot
	{
		get
		{
			return Data.Loot;
		}
		set
		{
			Data.Loot = value;
		}
	}

	public bool IsSkinningDisabled => Data.DroppedBy.Entity == null;

	public bool IsDroppedByPlayer => Data.IsDroppedByPlayer;

	public EntityRef<Entity> DroppedBy
	{
		get
		{
			return Data.DroppedBy;
		}
		set
		{
			Data.DroppedBy = value;
		}
	}

	public new EntityData Data => (EntityData)base.Data;

	public override void HandleHoverChange(bool isHover)
	{
		base.HandleHoverChange(isHover);
		MassLootHelper.HighlightLoot(this, isHover);
	}

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new EntityData(this));
	}
}
