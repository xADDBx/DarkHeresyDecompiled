using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("4bde3787d4f544c88b707e8d0d8c9f9e")]
public sealed class MapObjectGroupView : MechanicGroupView<MapObjectView>
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MapObjectGroupEntity(this));
	}
}
