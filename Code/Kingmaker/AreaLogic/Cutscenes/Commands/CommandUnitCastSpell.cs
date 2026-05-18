using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandUnitCastSpell")]
[TypeId("7be2c9b8172503249bec4d74dac25632")]
public class CommandUnitCastSpell : CommandBase
{
	private class Data
	{
		public BaseUnitEntity Caster;

		public Ability Ability;

		public bool RemoveAbilityOnStop;

		[CanBeNull]
		public UnitCommandHandle CommandHandle;

		public bool Waiting;

		public bool TakingTooLong;

		public bool Skipping;

		[CanBeNull]
		public UnitUseAbility Command => (UnitUseAbility)(CommandHandle?.Cmd);
	}

	private enum AbilityType
	{
		Custom,
		WeaponAttack
	}

	[SerializeField]
	private AbilityType m_Type;

	[SerializeField]
	[ShowIf("UseCustomAbility")]
	private BlueprintAbilityReference m_SpellBlueprint;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_UnitEvaluator;

	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_TargetUnitEvaluator;

	[SerializeField]
	[SerializeReference]
	private PositionEvaluator m_TargetPointEvaluator;

	[SerializeField]
	private bool m_DisableLog;

	public BlueprintAbility SpellBlueprint => m_SpellBlueprint?.Get();

	public bool HasTargetUnitEvaluator => m_TargetUnitEvaluator != null;

	public bool HasTargetPointEvaluator => m_TargetPointEvaluator != null;

	public bool UseCustomAbility => m_Type == AbilityType.Custom;

	public override bool ShouldHaveControlledUnit => true;

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (m_UnitEvaluator == null || !m_UnitEvaluator.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	private TargetWrapper EvaluateTarget()
	{
		return ((TargetWrapper)(m_TargetUnitEvaluator?.GetValue())) ?? ((TargetWrapper)(m_TargetPointEvaluator?.GetValue()));
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!m_UnitEvaluator.TryGetValue(out var value) || !(value is BaseUnitEntity caster))
		{
			return CommandResult.Fail("Cant find unit to cast");
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Caster = caster;
		if (UseCustomAbility)
		{
			commandData.Ability = commandData.Caster.Abilities.Add(m_SpellBlueprint.Get());
			commandData.Ability.HideByCommand();
			commandData.RemoveAbilityOnStop = true;
		}
		else
		{
			commandData.Ability = commandData.Caster.GetFirstWeapon()?.Abilities.FirstItem();
		}
		commandData.Skipping = skipping;
		RunCommand(commandData, skipping);
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Caster != null && commandData.Ability != null)
		{
			commandData.Command?.Interrupt();
			if (commandData.RemoveAbilityOnStop)
			{
				commandData.Caster.Abilities.Remove(commandData.Ability);
			}
		}
		commandData.Caster = null;
		commandData.Ability = null;
		commandData.CommandHandle = null;
		commandData.RemoveAbilityOnStop = false;
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		CommandResult result = OnRun(player, skipping: true);
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.RemoveAbilityOnStop)
		{
			commandData.Caster.Abilities.Remove(commandData.Ability);
		}
		return result;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.TakingTooLong)
		{
			return true;
		}
		if (commandData.Waiting)
		{
			return false;
		}
		AbilityExecutionProcess abilityExecutionProcess = commandData.Command?.ExecutionProcess;
		UnitUseAbility command = commandData.Command;
		if (command != null)
		{
			if (command.IsFinished)
			{
				return abilityExecutionProcess?.IsEnded ?? true;
			}
			return false;
		}
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Waiting)
		{
			RunCommand(commandData, commandData.Skipping);
		}
		else if (commandData.Skipping)
		{
			UnitUseAbility command = commandData.Command;
			if (command != null && command.ShouldUnitApproach)
			{
				commandData.Caster.View.MovementAgent.Stop();
				Vector3 vector = commandData.Command.ApproachPoint - commandData.Command.Executor.Position;
				float magnitude = vector.magnitude;
				if (magnitude > commandData.Command.ApproachRadius.Cells().Meters)
				{
					commandData.Caster.Translocate(commandData.Command.Executor.Position + vector.normalized * (magnitude - commandData.Command.ApproachRadius.Cells().Meters), null);
				}
			}
		}
		if (time > 20.0)
		{
			player.GetCommandData<Data>(this).TakingTooLong = true;
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Skipping = true;
		commandData.Command?.ConvertToOneFrame();
		return CommandResult.Success;
	}

	private void RunCommand(Data data, bool immediate)
	{
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(data.Ability.Data, EvaluateTarget())
		{
			NeedLoS = false,
			DisableLog = m_DisableLog,
			HitPolicy = AttackHitPolicyType.AutoHit,
			IgnoreCooldown = true,
			FreeAction = true,
			IsOneFrameCommand = immediate
		};
		data.CommandHandle = data.Caster.Commands.Run(cmdParams);
		data.Waiting = !data.CommandHandle.Params.IsOneFrameCommand && data.Caster.Commands.Current != data.Command;
	}

	public override string GetCaption()
	{
		string text = m_UnitEvaluator?.GetCaptionShort() ?? "<null>";
		string text2 = ((!UseCustomAbility) ? m_Type.ToString() : (m_SpellBlueprint?.Get()?.NameSafe() ?? "<null>"));
		string text3 = m_TargetUnitEvaluator?.GetCaptionShort() ?? m_TargetPointEvaluator?.GetCaptionShort() ?? "<null>";
		return text + " <b>cast ability</b> " + text2 + " <b>to</b> " + text3;
	}

	public override string GetWarning()
	{
		if (!m_UnitEvaluator || !m_UnitEvaluator.CanEvaluate())
		{
			return "No caster";
		}
		if ((!m_TargetUnitEvaluator || !m_TargetUnitEvaluator.CanEvaluate()) && (!m_TargetPointEvaluator || !m_TargetPointEvaluator.CanEvaluate()))
		{
			return "No target";
		}
		return null;
	}
}
