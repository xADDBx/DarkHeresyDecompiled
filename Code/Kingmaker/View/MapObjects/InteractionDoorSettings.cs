using System;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionDoorSettings : InteractionSettings
{
	public enum NavMeshCutActionSettings
	{
		DoNotTouchNavmeshCut,
		EnableNavmeshCutWhenOpen,
		EnableNavmeshCutWhenClosed
	}

	public enum GridObstacleActionSettings
	{
		DoNotTouchGridObstacle,
		EnableGridObstacleWhenOpen,
		EnableGridObstacleWhenClosed
	}

	public AnimationClip ObstacleAnimation;

	public bool DisableOnOpen;

	public bool OpenByDefault;

	public NavMeshCutActionSettings NavmeshCutAction;

	public GridObstacleActionSettings GridObstacleAction;

	public StaticRendererLink HideWhenOpen;

	[AkEventReference]
	public string OpenSound;

	[AkEventReference]
	public string CloseSound;

	public bool DonNotNeedNavmeshCut;

	public ActionsReference OnOpenActions;

	public bool DoOpenActionsOnce;

	public ActionsReference OnCloseActions;

	public bool DoCloseActionsOnce;

	public override bool ShouldShowAdditionalCombatObjective => true;
}
