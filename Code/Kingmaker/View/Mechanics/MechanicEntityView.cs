using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Mechanics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("cfe1163b47d64ccb87bda1355e624620")]
public abstract class MechanicEntityView : EntityViewBase
{
	public new MechanicEntity Data => (MechanicEntity)base.Data;

	public MechanicEntity EntityData => Data;

	[CanBeNull]
	public virtual ParticlesSnapMap ParticlesSnapMap => null;

	[CanBeNull]
	public virtual UnitAsksManager Asks => null;

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
}
