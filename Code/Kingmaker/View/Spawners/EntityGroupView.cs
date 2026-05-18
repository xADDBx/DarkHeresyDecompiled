using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("9e19d847eaec460999dc26bf3ff2020d")]
public class EntityGroupView : AbstractEntityGroupView
{
	public Color GizmosColor = Color.red;

	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new EntityGroupEntity(this));
	}
}
