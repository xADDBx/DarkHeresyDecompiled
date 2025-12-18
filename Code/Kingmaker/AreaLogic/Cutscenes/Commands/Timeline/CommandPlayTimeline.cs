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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		DirectorAdapter directorAdapter = Director.FindView() as DirectorAdapter;
		PlayableDirector playableDirector = (directorAdapter ? directorAdapter.GetComponent<PlayableDirector>() : null);
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Adapter = directorAdapter;
		commandData.Director = playableDirector;
		if (!directorAdapter || !playableDirector)
		{
			PFLog.Default.Error($"Command {this} in {player.Cutscene}: cannot find PlayableDirector {Director}");
			return;
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
		directorAdapter.SetTime(skipping ? ((float)playableDirector.duration + 1f) : 0f);
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	private bool EvaluateIsContinuous()
	{
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

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		PlayableDirector director = commandData.Director;
		if ((bool)director && director.playableGraph.IsValid())
		{
			director.Stop();
		}
		DirectorAdapter adapter = commandData.Adapter;
		if ((bool)adapter)
		{
			adapter.Stop();
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
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
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		if ((bool)commandData.Director && (bool)commandData.Adapter)
		{
			commandData.Adapter.SetTime((float)commandData.Director.duration + 1f);
		}
	}

	public override string GetCaption()
	{
		return "Play timeline " + Director.EntityNameInEditor;
	}
}
