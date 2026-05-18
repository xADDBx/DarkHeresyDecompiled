using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.View.MapObjects.SriptZones;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IScriptZoneConfig : IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	string name { get; }

	BlueprintScriptZone Blueprint { get; }

	bool DisableSameMultipleTriggers { get; }

	bool OnceOnly { get; }

	bool PlayersOnly { get; }

	bool UseDeads { get; }

	bool UseGlobalCooldown { get; }

	List<IScriptZoneShape> Shapes { get; }

	ScriptZoneEvent OnUnitEntered { get; }

	ScriptZoneEvent OnUnitExited { get; }
}
