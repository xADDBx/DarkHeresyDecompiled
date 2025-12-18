using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartInventory : PartItemsCollection, IUnitFactionHandler<EntitySubscriber>, IUnitFactionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitFactionHandler, EntitySubscriber>, ILevelUpCompleteUIHandler<EntitySubscriber>, ILevelUpCompleteUIHandler, IEventTag<ILevelUpCompleteUIHandler, EntitySubscriber>, ICompanionChangeHandler<EntitySubscriber>, ICompanionChangeHandler, IEventTag<ICompanionChangeHandler, EntitySubscriber>, IPartyHandler<EntitySubscriber>, IPartyHandler, IEventTag<IPartyHandler, EntitySubscriber>, IHashable, IOwlPackable<PartInventory>
{
	public interface IOwner : IEntityPartOwner<PartInventory>, IEntityPartOwner
	{
		PartInventory Inventory { get; }
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class DroppedLootData : IHashable, IOwlPackable, IOwlPackable<DroppedLootData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "DroppedLootData",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Position", typeof(Vector3)),
				new FieldInfo("Rotation", typeof(Vector3))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public Vector3 Position { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public Vector3 Rotation { get; private set; }

		public DroppedLootData(Vector3 position, Vector3 rotation)
		{
			Position = position;
			Rotation = rotation;
		}

		[JsonConstructor]
		private DroppedLootData()
		{
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Vector3 val = Position;
			result.Append(ref val);
			Vector3 val2 = Rotation;
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			DroppedLootData source = new DroppedLootData();
			result = Unsafe.As<DroppedLootData, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<DroppedLootData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			Vector3 value = Position;
			formatter.Field(0, "Position", ref value, state);
			Vector3 value2 = Rotation;
			formatter.Field(1, "Rotation", ref value2, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DroppedLootData>();
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
					Position = formatter.ReadPackable<Vector3>(state);
					break;
				case 1:
					Rotation = formatter.ReadPackable<Vector3>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private EntityRef<DroppedLoot.EntityData> m_DroppedLootEntityDataRef;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private DroppedLootData m_AttachedDroppedLootData;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartInventory",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("CollectionConverter", typeof(ItemsCollection)),
			new FieldInfo("HasOwnInventory", typeof(bool)),
			new FieldInfo("m_DroppedLootEntityDataRef", typeof(EntityRef<DroppedLoot.EntityData>)),
			new FieldInfo("m_AttachedDroppedLootData", typeof(DroppedLootData))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool HasOwnInventory { get; private set; }

	public DroppedLootData AttachedDroppedLootData => m_AttachedDroppedLootData;

	public bool IsLootDroppedAsEntity => m_DroppedLootEntityDataRef != null;

	public bool IsLootDroppedAsAttached => m_AttachedDroppedLootData != null;

	public bool IsLootDropped
	{
		get
		{
			if (!IsLootDroppedAsEntity)
			{
				return IsLootDroppedAsAttached;
			}
			return true;
		}
	}

	public override bool HasLoot
	{
		get
		{
			if ((base.OwnerUnit == null || !base.OwnerUnit.Faction.IsPlayer) && !base.ConcreteOwner.GetOptional<UnitPartUnlootable>() && base.HasLoot)
			{
				return !IsLootDroppedAsEntity;
			}
			return false;
		}
	}

	protected override ItemsCollection SetupInternal(ItemsCollection currentCollection)
	{
		if (base.OwnerUnit != null && !base.OwnerUnit.Faction.IsPlayer)
		{
			HasOwnInventory = true;
		}
		ItemsCollection result = currentCollection;
		BaseUnitEntity ownerUnit = base.OwnerUnit;
		if (ownerUnit != null && ownerUnit.Faction.IsPlayer && !HasOwnInventory)
		{
			result = Game.Instance.PartySharedInventory.Collection;
		}
		else if (currentCollection?.Owner != base.Owner)
		{
			result = new ItemsCollection(base.ConcreteOwner);
		}
		return result;
	}

	protected override void OnCollectionChanged()
	{
		if (base.OwnerUnit != null)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.OwnerUnit, (Action<IUnitInventoryChanged>)delegate(IUnitInventoryChanged h)
			{
				h.HandleInventoryChanged();
			}, isCheckRuntime: true);
		}
	}

	public void EnsureOwn()
	{
		if (base.Collection.Owner != base.Owner)
		{
			HasOwnInventory = true;
			Setup();
		}
	}

	protected override void OnHoldingStateChanged()
	{
		if (base.ConcreteOwner.HoldingState != Game.Instance.Player.CrossSceneState && base.ConcreteOwner.HoldingState != null && !HasOwnInventory)
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				EnsureOwn();
			}
		}
	}

	public void DropLootToGround(bool dismember = false, Vector3? overridePos = null, bool dropAttached = false)
	{
		if (!HasLoot || IsLootDroppedAsAttached)
		{
			return;
		}
		if (dropAttached && base.OwnerUnit != null && !dismember)
		{
			m_AttachedDroppedLootData = new DroppedLootData(base.Owner.Position, Vector3.up * PFStatefulRandom.Visuals.AttachedDroppedLoot.Range(0f, 360f));
			EventBus.RaiseEvent((IBaseUnitEntity)base.OwnerUnit, (Action<ILootDroppedAsAttachedHandler>)delegate(ILootDroppedAsAttachedHandler h)
			{
				h.HandleLootDroppedAsAttached();
			}, isCheckRuntime: true);
			return;
		}
		DroppedLoot unityObject = ((dismember && base.OwnerUnit != null) ? ConfigRoot.Instance.HitSystemRoot.GetDismemberLoot(base.OwnerUnit.SurfaceType) : null);
		unityObject = unityObject.Or(null) ?? ConfigRoot.Instance.Prefabs.DroppedLootBag;
		DroppedLoot droppedLoot = Game.Instance.Controllers.EntitySpawner.SpawnEntityWithView(unityObject, overridePos ?? base.Owner.Position, base.Owner.View.Or(null)?.ViewTransform.rotation ?? Quaternion.identity, base.ConcreteOwner.HoldingState);
		StatefulRandom statefulRandom = ((base.OwnerUnit != null) ? base.OwnerUnit.Random : PFStatefulRandom.UnitLogic.Parts);
		Vector3 rotation = Vector3.up * statefulRandom.Range(0f, 360f);
		UnitHelper.UpdateDropTransform(base.OwnerUnit, droppedLoot.ViewTransform, rotation);
		if (base.OwnerUnit != null)
		{
			droppedLoot.Data.GetOptional<InteractionLootPart>()?.SetUnit(base.OwnerUnit.Blueprint);
		}
		if (dismember)
		{
			DroppedLoot.EntityData data = droppedLoot.Data;
			data.IsDismember = true;
			data.BloodType = base.OwnerUnit?.BloodType ?? BloodType.Dust;
			data.SurfaceType = base.OwnerUnit?.SurfaceType ?? SurfaceType.Ground;
		}
		droppedLoot.Loot = base.Collection;
		droppedLoot.DroppedBy = base.ConcreteOwner;
		m_DroppedLootEntityDataRef = droppedLoot.Data;
		LocalMapMarkerPart orCreate = ((MapObjectView)droppedLoot).Data.GetOrCreate<LocalMapMarkerPart>();
		orCreate.IsRuntimeCreated = true;
		orCreate.Settings.Type = LocalMapMarkType.Loot;
		orCreate.NonLocalizedDescription = base.ConcreteOwner.GetOptional<PartUnitDescription>()?.Name ?? "";
	}

	public void TransferInventoryToDroppedLoot()
	{
		if (m_DroppedLootEntityDataRef == null)
		{
			return;
		}
		DroppedLoot.EntityData entityData = m_DroppedLootEntityDataRef.Entity;
		if (entityData == null)
		{
			PFLog.Default.Warning($"Invalid DroppedLoot reference {m_DroppedLootEntityDataRef.Id} from Unit {base.Owner}");
			return;
		}
		entityData.Loot = new ItemsCollection(entityData);
		using (ContextData<ItemSlot.IgnoreLock>.Request())
		{
			base.Items.Where((ItemEntity i) => i.IsLootable).ToArray().ForEach(delegate(ItemEntity i)
			{
				Transfer(i, entityData.Loot);
			});
		}
		m_DroppedLootEntityDataRef = null;
		entityData.DroppedBy = null;
	}

	void IUnitFactionHandler.HandleFactionChanged()
	{
		Setup();
	}

	void ILevelUpCompleteUIHandler.HandleLevelUpComplete()
	{
		foreach (ItemEntity item in base.Collection.Items)
		{
			item.TryIdentify(base.OwnerUnit.FromBaseUnitEntity(), 0);
		}
	}

	void ICompanionChangeHandler.HandleRecruit()
	{
		foreach (ItemEntity item in base.Collection.Items)
		{
			item.TryIdentify(base.OwnerUnit.FromBaseUnitEntity(), 0);
		}
	}

	void ICompanionChangeHandler.HandleUnrecruit()
	{
	}

	void IPartyHandler.HandleAddCompanion()
	{
	}

	void IPartyHandler.HandleCompanionActivated()
	{
		if (base.OwnerUnit != null && base.OwnerUnit.Faction.IsPlayer)
		{
			HasOwnInventory = false;
		}
		Setup();
	}

	void IPartyHandler.HandleCompanionRemoved(bool stayInGame)
	{
	}

	void IPartyHandler.HandleCapitalModeChanged()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = HasOwnInventory;
		result.Append(ref val2);
		EntityRef<DroppedLoot.EntityData> obj = m_DroppedLootEntityDataRef;
		Hash128 val3 = StructHasher<EntityRef<DroppedLoot.EntityData>>.GetHash128(ref obj);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartInventory source = new PartInventory();
		result = Unsafe.As<PartInventory, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartInventory>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		ItemsCollection value = base.CollectionConverter;
		formatter.Field(0, "CollectionConverter", ref value, state);
		bool value2 = HasOwnInventory;
		formatter.UnmanagedField(1, "HasOwnInventory", ref value2, state);
		formatter.Field(2, "m_DroppedLootEntityDataRef", ref m_DroppedLootEntityDataRef, state);
		formatter.Field(3, "m_AttachedDroppedLootData", ref m_AttachedDroppedLootData, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartInventory>();
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
				base.CollectionConverter = formatter.ReadPackable<ItemsCollection>(state);
				break;
			case 1:
				HasOwnInventory = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_DroppedLootEntityDataRef = formatter.ReadPackable<EntityRef<DroppedLoot.EntityData>>(state);
				break;
			case 3:
				m_AttachedDroppedLootData = formatter.ReadPackable<DroppedLootData>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
