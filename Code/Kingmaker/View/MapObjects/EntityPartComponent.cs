using System;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public abstract class EntityPartComponent<TPart> : AbstractEntityPartComponent where TPart : EntityPartWithConfig, new()
{
	public override Type EntityPartType => typeof(TPart);

	public override object GetSettings()
	{
		return null;
	}
}
public abstract class EntityPartComponent<TPart, TSettings> : AbstractEntityPartComponent where TPart : EntityPartWithConfig, new() where TSettings : class
{
	[SerializeField]
	public TSettings Settings;

	public override Type EntityPartType => typeof(TPart);

	public override object GetSettings()
	{
		return Settings;
	}
}
