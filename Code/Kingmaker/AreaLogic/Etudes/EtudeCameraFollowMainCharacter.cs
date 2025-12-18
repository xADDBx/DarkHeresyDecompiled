using Kingmaker.Controllers.Rest;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("1141f72b719d4bdf941b141ec016c127")]
public class EtudeCameraFollowMainCharacter : EtudeBracketTrigger
{
	protected override void OnActivate()
	{
		base.OnActivate();
		FollowMainCharacter();
	}

	protected override void OnResume()
	{
		base.OnResume();
		FollowMainCharacter();
	}

	private static void FollowMainCharacter()
	{
		CameraController.CameraUnitFollower cameraUnitFollower = Game.Instance.Controllers.CameraController?.Follower;
		if (cameraUnitFollower != null && !cameraUnitFollower.HasTarget)
		{
			Game.Instance.Controllers.CameraController?.Follower.FollowMainCharacter();
			Game.Instance.Player.CameraScrollLocked.Retain();
		}
	}

	protected override void OnDeactivate()
	{
		Game.Instance.Player.CameraScrollLocked.Release();
		Game.Instance.Controllers.CameraController?.Follower.ReleaseMainCharacter();
		Game.Instance.Controllers.CameraController?.Follower.ScrollTo(Game.Instance.Player.MainCharacter.ToAbstractUnitEntity());
		base.OnDeactivate();
	}
}
