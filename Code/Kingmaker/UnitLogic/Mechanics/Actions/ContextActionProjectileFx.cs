using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("eb117305cabfd9b4c9d44512730470bf")]
public class ContextActionProjectileFx : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Projectile")]
	private BlueprintProjectileReference m_Projectile;

	public new BlueprintProjectile Projectile => m_Projectile?.Get();

	public override string GetCaption()
	{
		return "Spawn Projectile FX: " + ((Projectile != null) ? Projectile.name : "unspecified");
	}

	protected override void RunAction()
	{
		if (!base.Context.DisableFx)
		{
			if (base.Context.Caster == null)
			{
				Element.LogError(this, "Caster is missing");
			}
			else
			{
				new ProjectileLauncher(Projectile, base.Context.Caster, base.Target).Ability(base.Context.Ability).Launch();
			}
		}
	}
}
