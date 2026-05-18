using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[ComponentName("Command/CommandSwitchDoor")]
[TypeId("22c096e9da5a20444aa206de7156cd6a")]
public class CommandSwitchDoor : CommandBase
{
	public class PlayerData : ContextData<PlayerData>
	{
		public CutscenePlayerData Player { get; private set; }

		public PlayerData Setup(CutscenePlayerData player)
		{
			Player = player;
			return this;
		}

		protected override void Reset()
		{
			Player = null;
		}
	}

	private bool m_Finished;

	private Vector3 m_UnitOffset = Vector3.zero;

	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	[SerializeReference]
	public MapObjectEvaluator Door;

	public bool UnlockIfLocked;

	public bool CloseIfAlreadyOpen;

	public bool OpenIfAlreadyClosed;

	public bool WaitUntilAnimationEnds;

	public bool SyncUnitRotation;

	[SerializeReference]
	[ShowIf("SyncUnitRotation")]
	public AbstractUnitEvaluator Unit;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		using (ContextData<PlayerData>.Request().Setup(player))
		{
			if (!Door.TryGetValue(out var value))
			{
				m_Finished = true;
				return CommandResult.Fail("Failed to find door map object");
			}
			InteractionDoorPart optional = value.GetOptional<InteractionDoorPart>();
			if (optional == null)
			{
				m_Finished = true;
				return CommandResult.Fail("Door " + value.View.name + " does not have an InteractionDoorPart component");
			}
			if (UnlockIfLocked)
			{
				optional.SetUnlocked();
			}
			if (optional.GetState())
			{
				if (CloseIfAlreadyOpen)
				{
					optional.Open();
				}
			}
			else if (OpenIfAlreadyClosed)
			{
				optional.Open();
			}
			m_Finished = (skipping && !WaitUntilAnimationEnds) || !WaitUntilAnimationEnds;
			if (SyncUnitRotation)
			{
				if (Unit == null || !Unit.TryGetValue(out var value2))
				{
					return CommandResult.Fail("Failed to find unit to sync rotation with");
				}
				m_UnitOffset = value2.Position - optional.View.transform.position;
				m_UnitOffset.y = 0f;
			}
			return CommandResult.Success;
		}
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		if (base.TrySkip(player))
		{
			return !WaitUntilAnimationEnds;
		}
		return false;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (m_Finished)
		{
			return true;
		}
		return Door.GetValue().GetOptional<InteractionDoorPart>()?.IsAnimationFinished ?? false;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (!SyncUnitRotation)
		{
			return CommandResult.Success;
		}
		if (Unit == null || !Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit to sync rotation with");
		}
		if (!Door.TryGetValue(out var value2))
		{
			m_Finished = true;
			return CommandResult.Fail("Failed to find door map object");
		}
		Transform transform = value2.View.transform.GetChild(0).transform;
		value.Position = transform.position + transform.rotation * m_UnitOffset;
		value.DesiredOrientation = transform.rotation.eulerAngles.y;
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		m_Finished = !WaitUntilAnimationEnds;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "<b>Open door</b> " + Door?.GetCaptionShort();
	}
}
