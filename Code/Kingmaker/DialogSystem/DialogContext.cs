using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public class DialogContext : IDialogContext
{
	public Vector3 DialogPosition;

	[NotNull]
	public readonly HashSet<BaseUnitEntity> InvolvedUnits = new HashSet<BaseUnitEntity>();

	private EntityRef<BaseUnitEntity> m_CurrentSpeaker;

	private readonly string m_CustomSpeakerName;

	public BlueprintDialog Dialog { get; }

	[CanBeNull]
	public BaseUnitEntity Initiator { get; }

	[CanBeNull]
	public BaseUnitEntity FirstSpeaker { get; }

	[CanBeNull]
	public MapObjectEntity MapObject { get; }

	[CanBeNull]
	public BaseUnitEntity ActingUnit { get; private set; }

	public BaseUnitEntity CurrentSpeaker => m_CurrentSpeaker;

	public string CurrentSpeakerName { get; private set; }

	public BlueprintUnit CurrentSpeakerBlueprint { get; private set; }

	public DialogContext(DialogData dialogData)
		: this(dialogData.Dialog, dialogData.Initiator.ToBaseUnitEntity(), dialogData.Unit.ToBaseUnitEntity(), dialogData.MapObject.Entity, dialogData.CustomSpeakerName)
	{
	}

	public DialogContext([NotNull] BlueprintDialog dialog, [CanBeNull] BaseUnitEntity initiator = null, [CanBeNull] BaseUnitEntity firstSpeaker = null, [CanBeNull] MapObjectEntity mapObject = null, [CanBeNull] string customSpeakerName = null)
	{
		Dialog = dialog;
		Initiator = initiator;
		FirstSpeaker = firstSpeaker;
		MapObject = mapObject;
		m_CurrentSpeaker = FirstSpeaker;
		m_CustomSpeakerName = customSpeakerName;
		FillStartPosition(Dialog);
		AddInvolvedUnit(Initiator);
		AddInvolvedUnit(FirstSpeaker);
	}

	public void Update(BlueprintCue cue)
	{
		if (cue != null)
		{
			m_CurrentSpeaker = cue.Speaker.GetSpeaker(cue) ?? FirstSpeaker;
			AddInvolvedUnit(m_CurrentSpeaker);
		}
		CurrentSpeakerName = GetCurrentSpeakerName(cue);
		CurrentSpeakerBlueprint = GetCurrentSpeakerBlueprint(cue);
	}

	public void Update(BlueprintAnswer answer, BaseUnitEntity manualUnitSelection)
	{
		ActingUnit = answer.CharacterSelection.SelectUnit(answer, manualUnitSelection, forceManual: true);
	}

	private string GetCurrentSpeakerName(BlueprintCue cue)
	{
		if (cue?.Speaker.SpeakerPortrait != null)
		{
			return cue.Speaker.SpeakerPortrait.CharacterName;
		}
		if (CurrentSpeaker.FromBaseUnitEntity() != null)
		{
			return CurrentSpeaker.CharacterName;
		}
		if (!string.IsNullOrEmpty(m_CustomSpeakerName) && m_CustomSpeakerName != "<null>")
		{
			return m_CustomSpeakerName;
		}
		return string.Empty;
	}

	private BlueprintUnit GetCurrentSpeakerBlueprint(BlueprintCue cue)
	{
		object obj;
		if (cue == null || !cue.Speaker.NoSpeaker)
		{
			obj = cue?.Speaker.GetSpeakerBlueprint();
			if (obj == null)
			{
				return CurrentSpeaker?.Blueprint;
			}
		}
		else
		{
			obj = null;
		}
		return (BlueprintUnit)obj;
	}

	private void FillStartPosition(BlueprintDialog dialog)
	{
		bool flag = false;
		if (dialog.StartPosition != null)
		{
			try
			{
				DialogPosition = dialog.StartPosition.GetValue();
				flag = true;
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		if (flag)
		{
			return;
		}
		if (FirstSpeaker != null)
		{
			DialogPosition = FirstSpeaker.Position;
			return;
		}
		if (MapObject != null)
		{
			DialogPosition = MapObject.ViewPosition;
			return;
		}
		Game instance = Game.Instance;
		if (instance != null && instance.IsModeActive(GameModeType.GlobalMap))
		{
			DialogPosition = CameraRig.Instance.transform.position;
		}
		else
		{
			DialogPosition = Game.Instance?.Player.MainCharacter.Entity.Position ?? Vector3.zero;
		}
	}

	private void AddInvolvedUnit([CanBeNull] BaseUnitEntity unit)
	{
		if (unit != null)
		{
			if (CutsceneControlledUnit.GetControllingPlayer(unit) == null)
			{
				unit.Commands.InterruptAllInterruptible();
			}
			InvolvedUnits.Add(unit);
		}
	}
}
