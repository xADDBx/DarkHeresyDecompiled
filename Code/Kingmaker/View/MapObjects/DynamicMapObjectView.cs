using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[KnowledgeDatabaseID("6672130d750674146b2fe0549b847142")]
public class DynamicMapObjectView : MapObjectView
{
	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintDynamicMapObjectReference m_Blueprint;

	public override bool CreatesDataOnLoad => false;

	public BlueprintDynamicMapObject Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintDynamicMapObjectReference>();
		}
	}

	public override BlueprintMechanicEntityFact MechanicFactBlueprint => Blueprint ?? base.MechanicFactBlueprint;

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new DynamicMapObjectEntity(this));
	}
}
