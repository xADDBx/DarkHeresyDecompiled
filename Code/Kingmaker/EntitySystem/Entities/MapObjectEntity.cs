using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Parts.ViewBased;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.Scene.Mechanics.Entities;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public class MapObjectEntity : MechanicEntity<BlueprintMechanicEntityFact>, IMapObjectEntity, IMechanicEntity, IEntity, IDisposable, IHashable, IOwlPackable<MapObjectEntity>
{
	public bool SuppressedByFlashlight;

	public bool FlashlightOwnerNear;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MapObjectEntity",
		OldNames = null,
		Fields = new FieldInfo[17]
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
			new FieldInfo("IsNewInGame", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool WasHighlightedOnRevealAndNoticed { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	public MapObjectViewSettings ViewSettings { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsNewInGame { get; protected set; }

	public EntityPartsManager.PartsByTypeEnumerable<AbstractInteractionPart> Interactions => GetAll<AbstractInteractionPart>();

	public AbstractInteractionPart FirstInteraction => Interactions.FirstOrDefault();

	[CanBeNull]
	public PartAwarenessCheck AwarenessCheck => GetOptional<PartAwarenessCheck>();

	[CanBeNull]
	public PartDetectiveObject DetectiveObject => GetOptional<PartDetectiveObject>();

	public bool IsAwarenessCheckPassed => AwarenessCheck.IsPassed();

	public override bool IsSuppressible => true;

	public override bool IsAffectedByFogOfWar => true;

	public override bool AddToGrid => true;

	public new MapObjectView View => (MapObjectView)base.View;

	public bool VisibilitySuppressedByFlashlight()
	{
		UnitPartFlashlight flashlight = Game.Instance.Player.Flashlight;
		if (flashlight != null && !flashlight.FlashlightInUse)
		{
			return false;
		}
		if (FlashlightOwnerNear)
		{
			return false;
		}
		if (SuppressedByFlashlight)
		{
			return true;
		}
		return false;
	}

	public MapObjectEntity(string uniqueId, bool isInGame, BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public MapObjectEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame, ConfigRoot.Instance.SystemMechanics.DefaultMapObjectBlueprint)
	{
	}

	public MapObjectEntity(MapObjectView view)
		: this(view.UniqueId, view.IsInGameBySettings, ConfigRoot.Instance.SystemMechanics.DefaultMapObjectBlueprint)
	{
	}

	protected MapObjectEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected MapObjectEntity()
	{
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new MechanicEntityFactBlueprinted(blueprint, null);
	}

	protected override IEntityViewBase CreateViewForData()
	{
		if (ViewSettings?.Blueprint != null)
		{
			GameObject gameObject = ViewSettings.Blueprint.Prefab.Load();
			if ((bool)gameObject && !gameObject.GetComponent<MapObjectView>())
			{
				PFLog.Default.Error($"Resource with id '{ViewSettings.Blueprint}' is not a MapObjectView");
				return null;
			}
			MapObjectView mapObjectView = UnityEngine.Object.Instantiate(gameObject, ViewSettings.Position, ViewSettings.Rotation).Or(null)?.GetComponent<MapObjectView>();
			if ((bool)mapObjectView)
			{
				mapObjectView.UniqueId = base.UniqueId;
				ViewSettings.Blueprint.InitializeObjectView(mapObjectView);
			}
			return mapObjectView;
		}
		return null;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		WasHighlightedOnRevealAndNoticed = View.InteractionComponent == null && !(View is AbstractDestructibleEntityView);
		FactHolder.Fact fact = Facts.GetAll<FactHolder.Fact>().FirstItem();
		BlueprintLogicConnector blueprintLogicConnector = (View.FactHolder ? View.FactHolder.Blueprint : null);
		if (fact?.Blueprint != blueprintLogicConnector)
		{
			if (fact != null)
			{
				Facts.Remove(fact);
			}
			if ((bool)blueprintLogicConnector)
			{
				Facts.Add(new FactHolder.Fact(blueprintLogicConnector));
			}
		}
	}

	protected override void OnIsInGameChanged()
	{
		IsNewInGame = base.IsInGame;
		base.OnIsInGameChanged();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		EventBus.RaiseEvent((IMapObjectEntity)this, (Action<IMapObjectHandler>)delegate(IMapObjectHandler h)
		{
			h.HandleMapObjectSpawned();
		}, isCheckRuntime: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		EventBus.RaiseEvent((IMapObjectEntity)this, (Action<IMapObjectHandler>)delegate(IMapObjectHandler h)
		{
			h.HandleMapObjectDestroyed();
		}, isCheckRuntime: true);
	}

	public bool IsAwarenessRollAllowed([NotNull] BaseUnitEntity unit)
	{
		return AwarenessCheck?.IsCheckAllowedFor(unit) ?? false;
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<EntityBoundsPart>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = WasHighlightedOnRevealAndNoticed;
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<MapObjectViewSettings>.GetHash128(ViewSettings);
		result.Append(ref val3);
		bool val4 = IsNewInGame;
		result.Append(ref val4);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MapObjectEntity source = new MapObjectEntity();
		result = Unsafe.As<MapObjectEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MapObjectEntity>(OwlPackTypeInfo);
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
		bool value3 = WasHighlightedOnRevealAndNoticed;
		formatter.UnmanagedField(14, "WasHighlightedOnRevealAndNoticed", ref value3, state);
		MapObjectViewSettings value4 = ViewSettings;
		formatter.Field(15, "ViewSettings", ref value4, state);
		bool value5 = IsNewInGame;
		formatter.UnmanagedField(16, "IsNewInGame", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MapObjectEntity>();
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
				WasHighlightedOnRevealAndNoticed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				ViewSettings = formatter.ReadPackable<MapObjectViewSettings>(state);
				break;
			case 16:
				IsNewInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MapObjectEntity<TBlueprint> : MapObjectEntity, IHashable, IOwlPackable<MapObjectEntity<TBlueprint>> where TBlueprint : BlueprintMechanicEntityFact
{
	public new TBlueprint OriginalBlueprint => (TBlueprint)base.OriginalBlueprint;

	public new TBlueprint Blueprint => (TBlueprint)base.Blueprint;

	public override Type RequiredBlueprintType => typeof(TBlueprint);

	protected MapObjectEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected MapObjectEntity(string uniqueId, bool isInGame, TBlueprint blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected MapObjectEntity()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public abstract override void Serialize<TFormatter>(TFormatter formatter, SerializerState state);

	public abstract override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state);
}
