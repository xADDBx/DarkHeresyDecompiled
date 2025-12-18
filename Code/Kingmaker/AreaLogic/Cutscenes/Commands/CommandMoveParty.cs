using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Formations;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/CommandMoveParty")]
[TypeId("c13acfdc96afb6641b748bc39e972c97")]
public class CommandMoveParty : CommandBase
{
	private class Data
	{
		public List<UnitData> AffectedUnits = new List<UnitData>();
	}

	public class UnitData
	{
		public EntityRef<BaseUnitEntity> Unit;

		public Path Path;

		public UnitCommandHandle Command;

		public Vector3 TargetPosition;

		public Quaternion TargetRotation;

		public bool Interrupt;
	}

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[KDB("Вставляем только LocatorView")]
	[AllowedEntityType(typeof(LocatorView))]
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public EntityReference[] Targets;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ShowIf("OverrideSpeed")]
	public float Speed = 5f;

	public bool DisableAvoidance = true;

	public bool MoveWithFormation;

	[ShowIf("MoveWithFormation")]
	public float FormationSpaceFactor = 1f;

	[SerializeField]
	[Tooltip("If true, dead or unconscious units will be teleported instead of moved")]
	public bool TranslocateDead;

	[SerializeField]
	[Tooltip("Timeout in case something breaks, forces this command to stop after this many seconds")]
	private float m_Timeout = 20f;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		int num = 0;
		Vector3[] positions = GetPositions();
		IEnumerable<BaseUnitEntity> charactersList = Game.Instance.Player.GetCharactersList(m_UnitsList);
		PFLog.Default.Log($"[CommandMoveParty] Starting with {m_UnitsList}. Party size:");
		foreach (BaseUnitEntity item in charactersList)
		{
			PFLog.Default.Log($"  - {item.CharacterName}: IsDead={item.LifeState.IsDead}, IsUnconscious={item.LifeState.IsUnconscious}, LifeState={item.LifeState.State}");
		}
		foreach (BaseUnitEntity item2 in charactersList)
		{
			Vector3 vector = positions[num++ % positions.Length];
			UnitData affectedUnit = new UnitData
			{
				Unit = item2,
				TargetPosition = vector,
				Interrupt = skipping
			};
			Transform transform = Targets[num % positions.Length]?.FindData()?.View?.ViewTransform;
			if (!MoveWithFormation && transform != null)
			{
				affectedUnit.TargetRotation = transform.rotation;
			}
			commandData.AffectedUnits.Add(affectedUnit);
			if (TranslocateDead && (item2.LifeState.IsDead || item2.LifeState.IsUnconscious))
			{
				PFLog.Default.Log($"[CommandMoveParty] Teleporting dead/unconscious unit: {item2.CharacterName} (IsDead={item2.LifeState.IsDead}, IsUnconscious={item2.LifeState.IsUnconscious})");
				item2.Translocate(vector, null);
				if (transform != null)
				{
					item2.SetOrientation(transform.rotation.eulerAngles.y);
				}
				continue;
			}
			PFLog.Default.Log($"[CommandMoveParty] Moving unit: {item2.CharacterName} (IsDead={item2.LifeState.IsDead})");
			if (skipping)
			{
				continue;
			}
			affectedUnit.Path = PathfindingService.Instance.FindPathRT_Delayed(item2.MovementAgent, affectedUnit.TargetPosition, 0.3f, 1, delegate(ForcedPath path)
			{
				affectedUnit.Path = null;
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (!affectedUnit.Interrupt)
				{
					UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, affectedUnit.TargetPosition)
					{
						OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null),
						MovementType = Animation
					};
					unitMoveToParams.MarkFromCutscene();
					BaseUnitEntity baseUnitEntity = affectedUnit.Unit;
					if (baseUnitEntity == null)
					{
						CutscenePlayerData.Logger.Error("Lost unit {0} while executing {1}", affectedUnit.Unit, this);
					}
					else
					{
						UnitCommandHandle command = baseUnitEntity.Commands.Run(unitMoveToParams);
						affectedUnit.Command = command;
						if (DisableAvoidance)
						{
							baseUnitEntity.View.MovementAgent.AvoidanceDisabled = true;
						}
					}
				}
			});
		}
	}

	private Vector3[] GetPositions()
	{
		if (!MoveWithFormation)
		{
			return Targets.Select((EntityReference x) => x.FindData().Position).ToArray();
		}
		return GetFormationPositions();
	}

	private Vector3[] GetFormationPositions()
	{
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable).ToList();
		IPartyFormation currentFormation = Game.Instance.Player.FormationManager.CurrentFormation;
		Span<Vector3> resultPositions = stackalloc Vector3[list.Count];
		Vector3 position = (Targets[0].FindData() ?? throw new Exception("No data for target at " + name)).Position;
		PartyFormationHelper.FillFormationPositions(position, FormationAnchor.Front, ClickGroundHandler.GetDirection(position, list), list, list, currentFormation, resultPositions, FormationSpaceFactor);
		return resultPositions.ToArray();
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		int num = 0;
		Vector3[] positions = GetPositions();
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			Vector3 position = positions[num % positions.Length];
			Transform transform = Targets[num % positions.Length]?.FindData()?.View?.ViewTransform;
			characters.Translocate(position, null);
			if (transform != null)
			{
				characters.SetOrientation(transform.rotation.eulerAngles.y);
			}
			num++;
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		foreach (UnitData affectedUnit in player.GetCommandData<Data>(this).AffectedUnits)
		{
			if (affectedUnit.Path != null)
			{
				return false;
			}
			UnitCommandHandle command = affectedUnit.Command;
			if (command != null && !command.IsFinished)
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (time > (double)m_Timeout)
		{
			InterruptImpl(commandData);
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		InterruptImpl(commandData);
	}

	private void InterruptImpl(Data commandData)
	{
		foreach (UnitData affectedUnit in commandData.AffectedUnits)
		{
			affectedUnit.Interrupt = true;
			affectedUnit.Path = null;
			UnitCommandHandle command = affectedUnit.Command;
			if (command != null && !command.IsFinished)
			{
				command.Interrupt();
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		TeleportEveryoneFailedToArrive(commandData);
		if (DisableAvoidance)
		{
			foreach (UnitData affectedUnit in commandData.AffectedUnits)
			{
				BaseUnitEntity baseUnitEntity = affectedUnit.Unit;
				if (baseUnitEntity == null)
				{
					CutscenePlayerData.Logger.Error("Lost unit {0} while executing {1}", affectedUnit.Unit, this);
				}
				else
				{
					baseUnitEntity.View.MovementAgent.AvoidanceDisabled = false;
				}
			}
		}
		foreach (UnitData affectedUnit2 in commandData.AffectedUnits)
		{
			((BaseUnitEntity)affectedUnit2.Unit).SetOrientation(affectedUnit2.TargetRotation.eulerAngles.y);
		}
	}

	public override string GetCaption()
	{
		return $"Move party ({m_UnitsList})";
	}

	private void TeleportEveryoneFailedToArrive(Data commandData)
	{
		foreach (UnitData affectedUnit in commandData.AffectedUnits)
		{
			UnitCommandHandle command = affectedUnit.Command;
			if (command == null || command.Result != AbstractUnitCommand.ResultType.Success)
			{
				BaseUnitEntity baseUnitEntity = affectedUnit.Unit;
				if (baseUnitEntity == null)
				{
					CutscenePlayerData.Logger.Error("Lost unit {0} while executing {1}", affectedUnit.Unit, this);
				}
				else
				{
					baseUnitEntity.Translocate(affectedUnit.TargetPosition, null);
				}
			}
		}
	}
}
