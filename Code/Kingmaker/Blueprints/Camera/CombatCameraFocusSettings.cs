using System;

namespace Kingmaker.Blueprints.Camera;

[Serializable]
public class CombatCameraFocusSettings
{
	public bool FocusOnStartTurn;

	public bool FocusCasterBeforeAction;

	public bool FocusTargetOnAttack;

	public bool FocusTargetOnDeath;

	public bool FocusAoeTargetsOnKill;

	public bool FocusCasterAfterAction;

	public bool FollowMovement;

	public bool FocusOnMissedTurn;
}
