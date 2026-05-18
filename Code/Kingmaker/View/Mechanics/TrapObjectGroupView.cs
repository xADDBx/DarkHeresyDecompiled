using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("fbd3990a28d24e6188cb835ef85b43d3")]
public sealed class TrapObjectGroupView : MechanicGroupView<TrapObjectView>
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new TrapObjectGroupEntity(this));
	}
}
