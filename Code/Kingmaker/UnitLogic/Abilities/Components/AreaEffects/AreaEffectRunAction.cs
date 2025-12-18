using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[TypeId("24be9d7901731604fb3e9dcc6c21fbb6")]
public class AreaEffectRunAction : AreaEffectLogic
{
	public ActionList UnitEnter = new ActionList();

	public ActionList UnitExit = new ActionList();

	public ActionList UnitMove = new ActionList();

	public ActionList Round = new ActionList();

	public ActionList EffectEnd = new ActionList();

	public ActionList EffectEndForEachUnit = new ActionList();

	public ActionList OnUnitTurnStart = new ActionList();

	public ActionList OnUnitTurnEnd = new ActionList();

	protected override void OnEntityEnter(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		UnitEnter.Run();
	}

	protected override void OnEntityExit(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		UnitExit.Run();
	}

	protected override void OnEntityMove(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		UnitMove.Run();
	}

	protected override void OnEntityTurnStart(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnUnitTurnStart.Run();
	}

	protected override void OnEntityTurnEnd(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnUnitTurnEnd.Run();
	}

	protected override void OnRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (!Round.HasActions)
		{
			return;
		}
		foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
		{
			using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(item))
			{
				Round.Run();
			}
		}
	}

	protected override void OnEndForEachEntity(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
		{
			using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(item))
			{
				EffectEndForEachUnit.Run();
			}
		}
	}

	protected override void OnEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(areaEffect))
		{
			EffectEnd.Run();
		}
	}
}
