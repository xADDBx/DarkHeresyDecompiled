using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Code.Framework.CutsceneSystem;

[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("c44efb36d5bb463e868fa387b76da24c")]
public class BlueprintCutscene : BlueprintScriptableObject, IEvaluationErrorHandlingPolicyHolder, ICutscene
{
	public enum MarkedUnitHandlingType
	{
		Pause,
		Stop,
		PauseAndRestart
	}

	public const float AwakeRange = 24.2f;

	public CutscenePriority Priority;

	public bool LockControl;

	public bool NonSkippable;

	[Tooltip("If set, units moved by this cutscene cannot start a dialog")]
	public bool ForbidDialogs;

	[Tooltip("If set, stops all playing barks on cutscene start and prevents other barks from playing while this cutscene is active")]
	public bool SuppressOtherBarks;

	[SerializeField]
	[HideIf("LockControl")]
	[Tooltip("If set, the cutscene auto-pauses when there's a dialog, rest, or exclusive cutscene playing")]
	private bool _isBackground;

	[Tooltip("If set, cutscene will override unit FreezeOutsideCamera flag to false")]
	public bool Freezeless;

	[Tooltip("If not set, cutscene is paused when all anchors are in fog of war or away enough from party")]
	public bool Sleepless;

	[HideIf("PreventCopies")]
	[Tooltip("If set, exact copies of this cutscene (with the same parameters) can play at the same time. You probably do not need to set this.")]
	public bool AllowCopies;

	[HideIf("AllowCopies")]
	[Tooltip("If set, PlayCutscene is a no-op when a live instance with the same parameters is already playing.")]
	public bool PreventCopies;

	public bool ShowOverlay;

	[Tooltip("Usually if unit is Marked by cutscene, Roaming is disabled. This option allows Roaming to remain enabled even if unit is Marked by this cutscene (other cutscenes may still prevent unit from roaming).")]
	public bool AllowRoaming;

	[HideIf("Sleepless")]
	[AllowedEntityType(typeof(CutsceneAnchorView))]
	public EntityReference[] Anchors = new EntityReference[0];

	[Tooltip("How to react when a unit marked by this cutscene is in combat or marked by a higher priority cutscene")]
	public MarkedUnitHandlingType MarkedUnitHandling;

	public ParametrizedContextSetter DefaultParameters;

	public List<CutsceneBlock> Blocks = new List<CutsceneBlock>();

	public List<CutsceneStageSwitch> StageSwitches = new List<CutsceneStageSwitch>();

	public ActionList OnFinished;

	public string Name => name;

	public bool IsBackground
	{
		get
		{
			if (!LockControl)
			{
				return _isBackground;
			}
			return false;
		}
	}

	public bool IsDebugMode => false;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy { get; set; }

	public IEnumerable<CommandBase> AllCommands()
	{
		return from com in Blocks.SelectMany((CutsceneBlock block) => block.Gates).SelectMany((CutsceneGate gate) => gate.Tracks).SelectMany((CutsceneTrack track) => track.Commands)
			select com.Get();
	}

	public IEnumerable<T> AllCommandsOfType<T>() where T : CommandBase
	{
		return AllCommands().OfType<T>();
	}
}
