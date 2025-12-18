using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.Spawners.Components;

[RequireComponent(typeof(UnitSpawnerBase))]
[KnowledgeDatabaseID("4cc3ab944f8c67e49bb75ef42129855a")]
public class SpawnerCorpseInteraction : EntityPartComponent<SpawnerCorpseInteraction.Part>
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

		public void OnSpawn(AbstractUnitEntity unit)
		{
			unit.GetOrCreate<CorpseInteractionPart>().SetSource(base.ConcreteOwner);
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

	[OwlPackable(OwlPackableMode.Generate)]
	public class CorpseInteractionPart : BaseUnitPart, IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IAreaHandler, IHashable, IOwlPackable<CorpseInteractionPart>
	{
		[JsonProperty]
		[OwlPackInclude]
		private EntityRef m_SourceRef;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "CorpseInteractionPart",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_SourceRef", typeof(EntityRef)),
				new FieldInfo("InteractionObjectRef", typeof(EntityRef<DynamicMapObjectView.EntityData>))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public EntityRef<DynamicMapObjectView.EntityData> InteractionObjectRef { get; private set; }

		public void SetSource(Entity source)
		{
			m_SourceRef = source?.Ref ?? default(EntityRef);
		}

		void IUnitDieHandler.OnUnitDie()
		{
			if (InteractionObjectRef == null || InteractionObjectRef.IsNull)
			{
				AddInteraction();
			}
		}

		void IAreaHandler.OnAreaBeginUnloading()
		{
		}

		void IAreaHandler.OnAreaDidLoad()
		{
			if (InteractionObjectRef != null && !InteractionObjectRef.IsNull)
			{
				AddInteraction();
			}
		}

		protected override void OnAttach()
		{
			base.Owner.Features.SuppressedDismember.Retain();
			base.Owner.Features.SuppressedDecomposition.Retain();
		}

		protected override void OnDetach()
		{
			base.Owner.Features.SuppressedDismember.Release();
			base.Owner.Features.SuppressedDecomposition.Release();
		}

		protected override void OnPostLoad()
		{
			OnAttach();
		}

		private void AddInteraction()
		{
			EntityViewBase entityViewBase = (EntityViewBase)(m_SourceRef.Entity?.View);
			if (entityViewBase == null)
			{
				return;
			}
			SpawnerCorpseInteraction component = entityViewBase.GetComponent<SpawnerCorpseInteraction>();
			if (!(component == null))
			{
				DynamicMapObjectView.EntityData entityData = EnsureObject(component);
				InteractionSkillCheck component2 = entityData.View.GetComponent<InteractionSkillCheck>();
				if (!(component2 == null))
				{
					component2.EnsurePart().SetSettings(component.m_Settings);
					InteractionObjectRef = entityData;
				}
			}
		}

		private DynamicMapObjectView.EntityData EnsureObject(SpawnerCorpseInteraction source)
		{
			if (InteractionObjectRef != null)
			{
				return InteractionObjectRef.Entity;
			}
			BlueprintDynamicMapObject blueprint = source.m_ObjectReference.Get();
			SceneEntitiesState mainState = Game.Instance.LoadedAreaState.MainState;
			DynamicMapObjectView.EntityData entityData = Game.Instance.Controllers.EntitySpawner.SpawnMapObject(blueprint, base.Owner.Position, Quaternion.identity, mainState);
			entityData.IsInGame = source.m_EnableInteractionOnDeath;
			return entityData;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			EntityRef obj = m_SourceRef;
			Hash128 val2 = EntityRefHasher.GetHash128(ref obj);
			result.Append(ref val2);
			EntityRef<DynamicMapObjectView.EntityData> obj2 = InteractionObjectRef;
			Hash128 val3 = StructHasher<EntityRef<DynamicMapObjectView.EntityData>>.GetHash128(ref obj2);
			result.Append(ref val3);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			CorpseInteractionPart source = new CorpseInteractionPart();
			result = Unsafe.As<CorpseInteractionPart, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<CorpseInteractionPart>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "m_SourceRef", ref m_SourceRef, state);
			EntityRef<DynamicMapObjectView.EntityData> value = InteractionObjectRef;
			formatter.Field(1, "InteractionObjectRef", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CorpseInteractionPart>();
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
					m_SourceRef = formatter.ReadPackable<EntityRef>(state);
					break;
				case 1:
					InteractionObjectRef = formatter.ReadPackable<EntityRef<DynamicMapObjectView.EntityData>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	private InteractionSkillCheckSettings m_Settings;

	[SerializeField]
	private BlueprintDynamicMapObjectReference m_ObjectReference;

	[SerializeField]
	private bool m_EnableInteractionOnDeath = true;
}
