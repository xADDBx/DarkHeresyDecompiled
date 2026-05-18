using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Owlcat.Runtime.Core.Math;

namespace Kingmaker.Visual.Sound;

public static class UnitAsksHelper
{
	public static float LowHealthBarkHPPercent => ConfigRoot.Instance.Sound.LowHealthBarkHPPercent;

	public static float LowShieldBarkPercent => ConfigRoot.Instance.Sound.LowShieldBarkPercent;

	public static float AggroBarkRadius => ConfigRoot.Instance.Sound.AggroBarkRadius;

	public static int EnemyMassDeathKillsCount => ConfigRoot.Instance.Sound.EnemyMassDeathKillsCount;

	public static int TilesToBarkMoveOrderSpaceCombat => ConfigRoot.Instance.Sound.TilesToBarkMoveOrderSpaceCombat;

	public static Size[] EnemyShipSizesToBarkEnemyDeathSC => ConfigRoot.Instance.Sound.EnemyShipSizesToBarkEnemyDeathSC;

	public static Size[] EnemyShipSizesToBarkShieldIsDownSC => ConfigRoot.Instance.Sound.EnemyShipSizesToBarkShieldIsDownSC;

	public static float AggroBarkRadiusScr => AggroBarkRadius * AggroBarkRadius;

	public static bool IsSpaceCombat => Game.Instance.CurrentModeType == GameModeType.SpaceCombat;

	public static bool Schedule(this AskWrapper askWrapper, bool is2D = false, AskCallback callback = null, AsksContext context = null)
	{
		if (context == null)
		{
			context = UnitAsksManager.CreateAsksContext();
		}
		if (askWrapper == null || !askWrapper.HasBarks)
		{
			PFLog.VO.Error("Trying Scheduling Empty Ask " + (askWrapper.Type ?? "unknown") + " Caster: " + context.Caster?.Name + " Target: " + context.Target?.Entity?.Name);
			callback?.Invoke(context);
			return false;
		}
		AskSchedulingEntry schedulingEntry = new AskSchedulingEntry(askWrapper, is2D, callback, context);
		string reason;
		bool flag = askWrapper.UnitAsksManager.TrySchedule(schedulingEntry, out reason);
		if (flag)
		{
			PFLog.VO.Log("[VO] Scheduling Ask " + (askWrapper.Type ?? "unknown") + " \n Caster: " + context.Caster?.Name + " Target: " + context.Target?.Entity?.Name);
			askWrapper.UnitAsksManager.TryPlayNextAsk();
		}
		else
		{
			PFLog.VO.Log("[VO] Ask " + (askWrapper.Type ?? "unknown") + " was NOT scheduled because of: " + reason + " \n Caster: " + context.Caster?.Name + " Target: " + context.Target?.Entity?.Name);
		}
		return flag;
	}

	public static BaseUnitEntity GetRandomPartyEntity(Func<BaseUnitEntity, bool> predicate)
	{
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.Player.PartyAndPets.Where(predicate);
		return enumerable.Where(UnitIsCloseToCamera).Random(PFStatefulRandom.Visuals.Sounds) ?? enumerable.Random(PFStatefulRandom.Visuals.Sounds);
	}

	public static BaseUnitEntity GetRandomEntity(Func<BaseUnitEntity, bool> predicate)
	{
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.EntityPools.AllBaseAwakeUnits.Where(predicate);
		return enumerable.Where(UnitIsCloseToCamera).Random(PFStatefulRandom.Visuals.Sounds) ?? enumerable.Random(PFStatefulRandom.Visuals.Sounds);
	}

	public static bool UnitIsCloseToCamera(this BaseUnitEntity entity)
	{
		return VectorMath.SqrDistanceXZ(entity.Position, CameraRig.Instance.transform.position) < AggroBarkRadiusScr;
	}

	public static bool CanSpeakAsks(this MechanicEntity entity)
	{
		if (entity.View == null || entity.View.Asks == null || entity.IsDisposed || entity.IsDisposingNow)
		{
			return false;
		}
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return true;
		}
		if (!baseUnitEntity.LifeState.IsDead)
		{
			return true;
		}
		if (baseUnitEntity.View.DismembermentManager != null && baseUnitEntity.View.DismembermentManager.Dismembered)
		{
			return false;
		}
		if (entity.GetOptional<PartDropLootAndDestroyAfterDelay>() != null)
		{
			return false;
		}
		return true;
	}
}
