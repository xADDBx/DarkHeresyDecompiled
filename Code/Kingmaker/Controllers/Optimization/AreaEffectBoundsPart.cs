using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Controllers.Optimization;

[OwlPackable(OwlPackableMode.Generate)]
public class AreaEffectBoundsPart : EntityPart<AreaEffectEntity>, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, EntityBoundsController.IHasTick, AreaEffectEntity.IEntityWithinBoundsHandler, IAreaActivationHandler, IHashable, IOwlPackable<AreaEffectBoundsPart>
{
	private static readonly HashSet<EntityRef<MechanicEntity>> Empty = new HashSet<EntityRef<MechanicEntity>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaEffectBoundsPart",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[CanBeNull]
	private CircleCollider2D SphereVisionCollider { get; set; }

	[CanBeNull]
	private AreaEffectTrigger Trigger { get; set; }

	public HashSet<EntityRef<MechanicEntity>> Inside
	{
		get
		{
			if (!Trigger)
			{
				return Empty;
			}
			return Trigger.Inside;
		}
	}

	public HashSet<EntityRef<MechanicEntity>> Entered
	{
		get
		{
			if (!Trigger)
			{
				return Empty;
			}
			return Trigger.Entered;
		}
	}

	public HashSet<EntityRef<MechanicEntity>> Exited
	{
		get
		{
			if (!Trigger)
			{
				return Empty;
			}
			return Trigger.Exited;
		}
	}

	public void ClearDelta()
	{
		if ((bool)Trigger)
		{
			Trigger.ClearDelta();
		}
	}

	protected override void OnAttach()
	{
		SetupObjectCollision();
	}

	protected override void OnDetach()
	{
		DestroyColliders();
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		SetupObjectCollision();
	}

	private void DestroyColliders()
	{
		if ((bool)SphereVisionCollider)
		{
			UnityEngine.Object.Destroy(SphereVisionCollider.gameObject);
			SphereVisionCollider = null;
			Trigger = null;
		}
	}

	public void Tick()
	{
		using (ProfileScope.NewScope("Tick"))
		{
			if ((bool)SphereVisionCollider)
			{
				Bounds bounds = base.Owner.Shape.GetBounds();
				Transform transform = SphereVisionCollider.transform;
				Vector3 vector = bounds.center.To2D();
				if (transform.position != vector)
				{
					transform.position = vector;
				}
				float magnitude = bounds.extents.magnitude;
				magnitude = ScaleRadius(magnitude, Vector3.one);
				if (Math.Abs(SphereVisionCollider.radius - magnitude) > 0.05f)
				{
					SphereVisionCollider.radius = magnitude;
				}
			}
		}
	}

	private void SetupObjectCollision()
	{
		float magnitude = base.Owner.Shape.GetBounds().extents.magnitude;
		magnitude = ScaleRadius(magnitude, Vector3.one);
		SetupSphereVisionCollider(base.Owner, magnitude);
		if (!base.Owner.IsInGame)
		{
			HandleObjectInGameChanged();
		}
	}

	private static float ScaleRadius(float radius, Vector3 lossyScale)
	{
		float num = Math.Max(Math.Abs(lossyScale.x), Math.Abs(lossyScale.z));
		return Math.Max(0.05f, radius * num);
	}

	private void SetupSphereVisionCollider(AreaEffectEntity entity, float radius)
	{
		GameObject obj = new GameObject(entity.Blueprint.name + "_SphereVision")
		{
			layer = 23
		};
		SceneManager.MoveGameObjectToScene(obj, Game.Instance.Controllers.EntityBoundsController.Scene);
		CircleCollider2D circleCollider2D = obj.AddComponent<CircleCollider2D>();
		circleCollider2D.radius = radius;
		circleCollider2D.transform.position = entity.Position.To2D();
		circleCollider2D.isTrigger = true;
		SphereVisionCollider = circleCollider2D;
		AreaEffectTrigger areaEffectTrigger = circleCollider2D.gameObject.AddComponent<AreaEffectTrigger>();
		areaEffectTrigger.Entity = base.Owner;
		Trigger = areaEffectTrigger;
	}

	public void HandleObjectInGameChanged()
	{
		if ((bool)SphereVisionCollider)
		{
			SphereVisionCollider.gameObject.SetActive(base.Owner.IsInGame);
		}
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
		AreaEffectBoundsPart source = new AreaEffectBoundsPart();
		result = Unsafe.As<AreaEffectBoundsPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaEffectBoundsPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaEffectBoundsPart>();
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
