using System;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using OwlPack.Runtime;

namespace Kingmaker.ElementsSystem;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntityEvaluator : EntityEvaluator, IEvaluator<MechanicEntity>, IOwlPackable, IOwlPackable<MechanicEntityEvaluator>
{
	public new MechanicEntity GetValue()
	{
		return (MechanicEntity)base.GetValue();
	}

	public bool TryGetValue(out MechanicEntity value)
	{
		if (TryGetValue(out Entity value2))
		{
			value = value2 as MechanicEntity;
			return value != null;
		}
		value = null;
		return false;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
