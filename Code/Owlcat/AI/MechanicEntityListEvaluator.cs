using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using OwlPack.Runtime;

namespace Owlcat.AI;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntityListEvaluator : EntityListEvaluator, IEvaluator<List<MechanicEntity>>, IOwlPackable, IOwlPackable<MechanicEntityListEvaluator>
{
	public new List<MechanicEntity> GetValue()
	{
		return (from e in base.GetValue()
			select (MechanicEntity)e).ToList();
	}

	public bool TryGetValue(out List<MechanicEntity> value)
	{
		if (TryGetValue(out List<Entity> value2))
		{
			value = value2?.Select((Entity e) => (MechanicEntity)e).ToList();
			if (value != null)
			{
				return value.All((MechanicEntity me) => me != null);
			}
			return false;
		}
		value = null;
		return false;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
