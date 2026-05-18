using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View.MapObjects.SriptZones;

namespace Kingmaker.Framework.EntitySystem.Interfaces.Config;

public interface IAreaEffectConfig : IEntityConfig
{
	IScriptZoneShape Shape { get; }
}
