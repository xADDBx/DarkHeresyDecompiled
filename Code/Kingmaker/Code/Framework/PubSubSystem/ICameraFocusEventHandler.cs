using Kingmaker.Blueprints.Camera;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Code.Framework.PubSubSystem;

public interface ICameraFocusEventHandler : ISubscriber
{
	void HandleCameraFocusEvent(Transform target, CameraFollowTaskParams taskParams, bool pauseCombatTurnOrder);
}
