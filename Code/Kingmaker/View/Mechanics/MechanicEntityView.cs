using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Mechanics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("cfe1163b47d64ccb87bda1355e624620")]
public abstract class MechanicEntityView : EntityViewBase, IMechanicEntityView, IEntityView, IMechanicEntityConfig, IEntityConfig
{
	public new MechanicEntity Data => (MechanicEntity)base.Data;

	public MechanicEntity EntityData => Data;

	[CanBeNull]
	public virtual ParticlesSnapMap ParticlesSnapMap => null;

	[CanBeNull]
	public virtual UnitAsksManager Asks => null;

	public virtual BlueprintMechanicEntityFact MechanicFactBlueprint => ConfigRoot.Instance.SystemMechanics.DefaultMapObjectBlueprint;

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.color = Color.magenta;
		GizmoHelper.ShowCellsCoveredByMechanicEntity(Data, this);
		MechanicEntity data = Data;
		if (data != null)
		{
			Vector3 position = data.Position;
			Vector3 forward = data.Forward;
			Gizmos.DrawSphere(position, 0.1f);
			Gizmos.DrawLine(position, position + forward);
		}
	}

	GameObject IMechanicEntityView.get_gameObject()
	{
		return base.gameObject;
	}

	T[] IMechanicEntityView.GetComponentsInChildren<T>()
	{
		return GetComponentsInChildren<T>();
	}

	string IEntityView.get_name()
	{
		return base.name;
	}
}
