using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[ExecuteInEditMode]
[RequireComponent(typeof(EntityViewBase))]
[KnowledgeDatabaseID("8c4e829910cc0d84f8a2ac26a1332cf2")]
public abstract class AbstractEntityPartComponent : MonoBehaviour, IEntityPartConfig
{
	public abstract Type EntityPartType { get; }

	public EntityViewBase EntityView { get; private set; }

	public string EntityId => EntityView.UniqueId;

	public abstract object GetSettings();

	protected virtual void Awake()
	{
		EntityView = GetComponentInParent<EntityViewBase>();
	}
}
