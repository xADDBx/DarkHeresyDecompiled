using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("2fe2aff34d6e42d58b7e4374890b22a0")]
public sealed class AreaEffectGroupView : MechanicGroupView<AreaEffectView>
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new AreaEffectGroupEntity(this));
	}
}
