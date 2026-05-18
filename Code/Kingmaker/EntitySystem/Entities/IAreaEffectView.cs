using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.EntitySystem.Entities;

public interface IAreaEffectView : IEntityConfig
{
	void SyncTransform();
}
