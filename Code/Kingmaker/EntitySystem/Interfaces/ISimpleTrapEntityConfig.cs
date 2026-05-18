using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.View.MapObjects.Traps.Simple;

namespace Kingmaker.EntitySystem.Interfaces;

public interface ISimpleTrapEntityConfig : ITrapEntityConfig, IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	[CanBeNull]
	ActionList TriggerActions { get; }

	[CanBeNull]
	ActionList DisarmActions { get; }

	SimpleTrapObjectInfo Info { get; }
}
