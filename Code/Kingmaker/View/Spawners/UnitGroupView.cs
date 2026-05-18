using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("b94416fff5c65f44e9ec143a7ceb887e")]
public class UnitGroupView : AbstractEntityGroupView
{
	public Color GizmosColor = Color.red;

	public bool DisableOnSimplified;

	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new UnitGroupEntity(this));
	}
}
