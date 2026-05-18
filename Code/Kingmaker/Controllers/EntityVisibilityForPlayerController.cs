using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.Controllers;

public class EntityVisibilityForPlayerController : IControllerTick, IController
{
	private int m_VisibleEntityRevealingDisabledCounter;

	public void EnableVisibleEntityRevealing()
	{
		m_VisibleEntityRevealingDisabledCounter--;
	}

	public void DisableVisibleEntityRevealing()
	{
		m_VisibleEntityRevealingDisabledCounter++;
	}

	public bool IsVisibleEntityRevealingEnabled()
	{
		return m_VisibleEntityRevealingDisabledCounter <= 0;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		bool revealVisible = IsVisibleEntityRevealingEnabled();
		foreach (MechanicEntity mechanicEntity in Game.Instance.EntityPools.MechanicEntities)
		{
			Update(mechanicEntity, revealVisible);
		}
	}

	private static void Update(MechanicEntity entity, bool revealVisible)
	{
		BaseUnitEntity baseUnitEntity = entity as BaseUnitEntity;
		if (entity.View != null && (baseUnitEntity == null || !baseUnitEntity.IsSleepingWithouTimers || (baseUnitEntity.LifeState.IsFinallyDead && !baseUnitEntity.LifeState.IsDeathRevealed)))
		{
			bool visible = ((baseUnitEntity != null) ? IsVisible(baseUnitEntity) : IsVisible(entity));
			entity.View.SetVisible(visible, force: false, revealVisible);
		}
	}

	private static bool IsVisible(BaseUnitEntity unit)
	{
		if ((bool)unit.Features.Hidden)
		{
			return false;
		}
		if (unit.LifeState.IsHiddenBecauseDead)
		{
			return false;
		}
		UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.InParty)
		{
			return true;
		}
		if (unit.IsInvisible)
		{
			return false;
		}
		if (unit.IsInFogOfWar)
		{
			return false;
		}
		if (unit.Stealth.Active)
		{
			foreach (BaseUnitEntity item in unit.Stealth.SpottedBy)
			{
				if (item.Faction.IsPlayer)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private static bool IsVisible(MechanicEntity entity)
	{
		PartDetectiveServoSkull optional = entity.GetOptional<PartDetectiveServoSkull>();
		if (optional != null && !optional.Enabled)
		{
			return false;
		}
		bool flag = ((entity is AbstractUnitEntity) ? (!entity.IsInFogOfWar) : (entity.IsRevealed || !entity.IsInFogOfWar));
		if (!((!(entity is MapObjectEntity mapObjectEntity) || mapObjectEntity.IsAwarenessCheckPassed) && flag))
		{
			return false;
		}
		UnitPartFamiliar optional2 = entity.GetOptional<UnitPartFamiliar>();
		if (optional2 != null)
		{
			return optional2.IsVisible;
		}
		if ((entity is DetectiveTraceEntity || entity is DetectiveClueEntity) && (Game.Instance.Player.IsInCombat || Game.Instance.CurrentGameMode?.Type == GameModeType.Cutscene))
		{
			return false;
		}
		if (entity is TrapObjectData trapObjectData)
		{
			return trapObjectData.TrapActive;
		}
		return true;
	}
}
