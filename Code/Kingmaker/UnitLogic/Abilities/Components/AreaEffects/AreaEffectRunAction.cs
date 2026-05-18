using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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

	public bool SkipUnitEnterOnInitialPlacement;

	protected override void OnEntityEnter(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		if (!SkipUnitEnterOnInitialPlacement || !areaEffect.IsInInitialEntityScan)
		{
			UnitEnter.Run();
		}
	}

	protected override void OnEntityExit(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		UnitExit.Run();
	}

	protected override void OnEntityMove(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		UnitMove.Run();
	}

	protected override void OnEntityTurnStart(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnUnitTurnStart.Run();
	}

	protected override void OnEntityTurnEnd(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnUnitTurnEnd.Run();
	}

	protected override void OnRound(IEvalContext context, AreaEffectEntity areaEffect)
	{
		if (!Round.HasActions)
		{
			return;
		}
		foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
		{
			using (EvalContext.Current.PushTarget(item))
			{
				Round.Run();
			}
		}
	}

	protected override void OnEndForEachEntity(IEvalContext context, AreaEffectEntity areaEffect)
	{
		foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
		{
			using (EvalContext.Current.PushTarget(item))
			{
				EffectEndForEachUnit.Run();
			}
		}
	}

	protected override void OnEnd(IEvalContext context, AreaEffectEntity areaEffect)
	{
		using (EvalContext.Current.PushTarget(areaEffect))
		{
			EffectEnd.Run();
		}
	}
}
