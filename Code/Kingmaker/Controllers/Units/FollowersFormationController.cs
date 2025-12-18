using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Formations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class FollowersFormationController : IControllerTick, IController, IControllerEnable, IControllerDisable, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber
{
	private const float DestinationDiffTolerance = 1f;

	private const int FollowersFormationCapacity = 20;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		foreach (AbstractUnitEntity allAwakeUnit in Game.Instance.EntityPools.AllAwakeUnits)
		{
			UnitPartFollowedByUnits optional = allAwakeUnit.GetOptional<UnitPartFollowedByUnits>();
			if (optional == null || optional.Followers.Count < 1)
			{
				continue;
			}
			try
			{
				TickOnUnit(optional);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			finally
			{
				optional.PositionChanged = false;
			}
		}
	}

	void IEntityPositionChangedHandler.HandleEntityPositionChanged()
	{
		UnitPartFollowedByUnits unitPartFollowedByUnits = EventInvokerExtensions.BaseUnitEntity?.GetOptional<UnitPartFollowedByUnits>();
		if (unitPartFollowedByUnits != null)
		{
			unitPartFollowedByUnits.PositionChanged = true;
		}
	}

	private void TickOnUnit([NotNull] UnitPartFollowedByUnits leader)
	{
		BaseUnitEntity owner = leader.Owner;
		foreach (AbstractUnitEntity follower in leader.Followers)
		{
			UnitPartFollowUnit optional = follower.GetOptional<UnitPartFollowUnit>();
			if (optional != null)
			{
				optional.Skip = ShouldSkipProcessing(follower);
				if (optional.Skip)
				{
					leader.FollowersActionTypesTemp[follower] = FollowerActionType.DoNothing;
					leader.FollowerDesiredActions[follower] = new FollowerAction(Vector3.zero, 0f, FollowerActionType.DoNothing);
				}
			}
		}
		if (leader != null && !leader.PositionChanged && !leader.ForceRefresh)
		{
			return;
		}
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		float repathCooldownSec = ConfigRoot.Instance.Formations.FollowersFormation.RepathCooldownSec;
		if (!leader.ForceRefresh && gameTime.TotalSeconds < leader.LastRefreshTime + (double)repathCooldownSec)
		{
			return;
		}
		leader.LastRefreshTime = Game.Instance.Controllers.TimeController.GameTime.TotalSeconds;
		Vector3 unitDestination = GetUnitDestination(owner);
		float num = GeometryUtils.SqrMechanicsDistance(unitDestination, leader.LastKnownDestination);
		if (!leader.ForceRefresh && num < 1f)
		{
			return;
		}
		leader.ForceRefresh = false;
		leader.LastKnownDestination = unitDestination;
		leader.FollowersActionTypesTemp.Clear();
		List<AbstractUnitEntity> list = TempList.Get<AbstractUnitEntity>();
		uint area = ObstacleAnalyzer.GetArea(owner.Position);
		foreach (AbstractUnitEntity follower2 in leader.Followers)
		{
			UnitPartFollowUnit optional2 = follower2.GetOptional<UnitPartFollowUnit>();
			if (optional2 == null || !optional2.Skip)
			{
				FollowerAction? followerAction = leader.GetFollowerAction(follower2);
				if (!followerAction.HasValue || followerAction.Value.Type != FollowerActionType.Teleport || !(followerAction.Value.Position != follower2.Position))
				{
					uint area2 = ObstacleAnalyzer.GetArea(follower2.Position);
					list.Add(follower2);
					leader.FollowersActionTypesTemp[follower2] = ((area == area2) ? FollowerActionType.Move : FollowerActionType.Teleport);
				}
			}
		}
		Vector3 followersFrontPosition = GetFollowersFrontPosition(owner);
		foreach (List<AbstractUnitEntity> item in list.Slice(20))
		{
			PrepareFormation(leader, item, followersFrontPosition, leader.FollowersActionTypesTemp);
		}
	}

	public Dictionary<AbstractUnitEntity, FollowerAction> CalculateTeleportToLeaderDestinations(UnitPartFollowedByUnits leader)
	{
		leader.FollowersActionTypesTemp.Clear();
		List<AbstractUnitEntity> list = TempList.Get<AbstractUnitEntity>();
		foreach (AbstractUnitEntity follower in leader.Followers)
		{
			if (ShouldSkipProcessing(follower))
			{
				leader.FollowersActionTypesTemp[follower] = FollowerActionType.DoNothing;
				leader.FollowerDesiredActions[follower] = new FollowerAction(Vector3.zero, 0f, FollowerActionType.DoNothing);
			}
			else
			{
				list.Add(follower);
				leader.FollowersActionTypesTemp[follower] = FollowerActionType.Teleport;
			}
		}
		foreach (AbstractUnitEntity item in list)
		{
			leader.FollowerDesiredActions.Remove(item);
		}
		Vector3 followersFrontPosition = GetFollowersFrontPosition(leader.Owner);
		foreach (List<AbstractUnitEntity> item2 in list.Slice(20))
		{
			PrepareFormation(leader, item2, followersFrontPosition, leader.FollowersActionTypesTemp);
		}
		Dictionary<AbstractUnitEntity, FollowerAction> dictionary = new Dictionary<AbstractUnitEntity, FollowerAction>();
		foreach (AbstractUnitEntity item3 in list)
		{
			dictionary[item3] = leader.FollowerDesiredActions[item3];
		}
		return dictionary;
	}

	private static bool ShouldSkipProcessing(AbstractUnitEntity follower)
	{
		if (follower.LifeState.State == UnitLifeState.Dead)
		{
			return true;
		}
		UnitPartFollowUnit optional = follower.GetOptional<UnitPartFollowUnit>();
		if (optional == null)
		{
			return false;
		}
		if (optional.IsBusy)
		{
			return true;
		}
		if (!optional.FollowWhileCutscene && follower.CutsceneControlledUnit?.GetCurrentlyActive() != null)
		{
			return true;
		}
		if (!optional.FollowInCombat)
		{
			return optional.Leader.IsInCombat;
		}
		return false;
	}

	private void PrepareFormation(UnitPartFollowedByUnits leader, IList<AbstractUnitEntity> followers, Vector3 position, Dictionary<AbstractUnitEntity, FollowerActionType> desiredActions)
	{
		if (!followers.Empty())
		{
			FollowersFormation followersFormation = ConfigRoot.Instance.Formations.FollowersFormation;
			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			list.Add(leader.Owner);
			Span<Vector3> resultPositions = stackalloc Vector3[followers.Count];
			PartyFormationHelper.FillFormationPositions(position, FormationAnchor.Front, ClickGroundHandler.GetDirection(position, list), followers, followers, followersFormation, resultPositions, 1f, forceRelax: true);
			for (int i = 0; i < followers.Count; i++)
			{
				CreateFollowerAction(followers[i], leader, resultPositions[i], desiredActions[followers[i]]);
			}
		}
	}

	public void OnEnable()
	{
		ClearCache();
	}

	public void OnDisable()
	{
		ClearCache();
	}

	private static void ClearCache()
	{
		foreach (AbstractUnitEntity item in Game.Instance.EntityPools.AllUnits.All)
		{
			item.GetOptional<UnitPartFollowedByUnits>()?.ClearCache();
		}
	}

	private static Vector3 GetFollowersFrontPosition(AbstractUnitEntity leader)
	{
		Vector2 playerOffset = ConfigRoot.Instance.Formations.FollowersFormation.PlayerOffset;
		Vector3 unitDestination = GetUnitDestination(leader);
		Quaternion orientationQuaternion = GetOrientationQuaternion(leader);
		return unitDestination + orientationQuaternion * new Vector3(playerOffset.x, 0f, playerOffset.y);
	}

	private static Quaternion GetOrientationQuaternion(AbstractUnitEntity unit)
	{
		return Quaternion.Euler(0f, unit.Commands.CurrentMoveTo?.Orientation ?? unit.Orientation, 0f);
	}

	private static float GetOrientation(BaseUnitEntity unit)
	{
		return unit.Commands.CurrentMoveTo?.Orientation ?? unit.Orientation;
	}

	private static Vector3 GetUnitDestination(AbstractUnitEntity unit)
	{
		return unit.Commands.Current?.ApproachPoint ?? unit.Commands.CurrentMoveTo?.ApproachPoint ?? unit.Position;
	}

	public static void CreateFollowerAction(AbstractUnitEntity follower, UnitPartFollowedByUnits leader, Vector3 position, FollowerActionType type)
	{
		float num = ConfigRoot.Instance.Formations.FollowersFormation.LookAngleRandomSpread / 2f;
		FollowerAction value = new FollowerAction(position, GetOrientation(leader.Owner) + leader.Owner.Random.Range(0f - num, num), type);
		leader.FollowerDesiredActions[follower] = value;
	}
}
