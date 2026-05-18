using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

[TypeId("a192ae3f137817e468939aea9620dbca")]
public class CommandPlayTimeline : CommandBase
{
	[Serializable]
	public class UnitBindingData
	{
		public string Name;

		[ValidateNotNull]
		[SerializeReference]
		public AbstractUnitEvaluator Unit;
	}

	public class Data
	{
		public DirectorAdapter Adapter { get; set; }

		public PlayableDirector Director { get; set; }

		public TimelineGateSignalReceiver SignalReceiver { get; set; }
	}

	[ValidateNotNull]
	[AllowedEntityType(typeof(DirectorAdapter))]
	[UsedImplicitly]
	public EntityReference Director;

	[ShowIf("False")]
	[UsedImplicitly]
	public List<UnitBindingData> Units;

	private bool? m_Continuous;

	private bool False => false;

	public override bool IsContinuous
	{
		get
		{
			bool valueOrDefault = m_Continuous.GetValueOrDefault();
			if (!m_Continuous.HasValue)
			{
				valueOrDefault = EvaluateIsContinuous();
				m_Continuous = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		DirectorAdapter directorAdapter = Director.FindView() as DirectorAdapter;
		PlayableDirector playableDirector = (directorAdapter ? directorAdapter.GetComponent<PlayableDirector>() : null);
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Adapter = directorAdapter;
		commandData.Director = playableDirector;
		if (!directorAdapter || !playableDirector)
		{
			return CommandResult.Fail($"Command {this} in {player.Cutscene}: cannot find PlayableDirector {Director}");
		}
		TimelineGateSignalReceiver timelineGateSignalReceiver = player.View.gameObject.EnsureComponent<TimelineGateSignalReceiver>();
		TimelineAsset playable = playableDirector.playableAsset as TimelineAsset;
		timelineGateSignalReceiver.Setup(player, playable);
		commandData.SignalReceiver = timelineGateSignalReceiver;
		directorAdapter.ApplyBindings(Units);
		DirectorAdapter.UnitBinding[] boundUnits = directorAdapter.BoundUnits;
		for (int i = 0; i < boundUnits.Length; i++)
		{
			boundUnits[i].BoundUnit.ControlledByDirector.Retain();
		}
		playableDirector.Play();
		directorAdapter.Play();
		if (skipping)
		{
			directorAdapter.SetTime((float)playableDirector.duration + 1f);
			directorAdapter.FixupUnitPositions();
		}
		else
		{
			directorAdapter.SetTime(0f);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
	}

	private bool EvaluateIsContinuous()
	{
		if (Director == null)
		{
			return false;
		}
		DirectorAdapter directorAdapter = Director.FindView() as DirectorAdapter;
		PlayableDirector obj = (directorAdapter ? directorAdapter.GetComponent<PlayableDirector>() : null);
		if ((object)obj == null)
		{
			return false;
		}
		return obj.extrapolationMode == DirectorWrapMode.Loop;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.Adapter || !commandData.Director)
		{
			return true;
		}
		PlayableDirector director = commandData.Director;
		if (director.playableGraph.IsValid())
		{
			if (director.time >= director.duration - 0.001)
			{
				return director.extrapolationMode != DirectorWrapMode.Loop;
			}
			return false;
		}
		return true;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		DirectorAdapter adapter = commandData.Adapter;
		if (adapter != null)
		{
			adapter.Stop();
		}
		PlayableDirector director = commandData.Director;
		if (director != null && director.playableGraph.IsValid())
		{
			director.Stop();
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Adapter != null)
		{
			commandData.Adapter.FixupUnitPositions();
		}
		if (commandData.SignalReceiver != null)
		{
			commandData.SignalReceiver.UpdateTime(time);
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if ((bool)commandData.Director && (bool)commandData.Adapter)
		{
			commandData.Adapter.SetTime((float)commandData.Director.duration + 1f);
			commandData.Adapter.FixupUnitPositions();
		}
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "Play timeline " + Director.EntityNameInEditor;
	}
}
