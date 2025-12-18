using System;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using OwlPack.Runtime;

namespace Kingmaker.ElementsSystem;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class BaseUnitEvaluator : AbstractUnitEvaluator, IEvaluator<BaseUnitEntity>, IOwlPackable<BaseUnitEvaluator>
{
	public new BaseUnitEntity GetValue()
	{
		return (BaseUnitEntity)base.GetValue();
	}

	public IBaseUnitEntity GetInterfaceValue()
	{
		return GetValue();
	}

	public bool TryGetValue(out BaseUnitEntity value)
	{
		if (TryGetValue(out AbstractUnitEntity value2))
		{
			value = value2 as BaseUnitEntity;
			return value != null;
		}
		value = null;
		return false;
	}
}
