using Kingmaker.Cheats;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickCheatRemoteTeleportHandler : IClickEventHandler
{
	private BaseUnitEntity _target;

	public PointerMode GetMode()
	{
		return PointerMode.CheatRemoteTeleport;
	}

	public void Begin()
	{
		_target = null;
		Notify("Remote Teleport: click on target unit");
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		if (gameObject == null)
		{
			return new HandlerPriorityResult(0f);
		}
		if (_target == null)
		{
			return new HandlerPriorityResult((gameObject.GetComponentNonAlloc<AbstractUnitEntityView>()?.Data != null) ? 100f : 0f);
		}
		return new HandlerPriorityResult(100f);
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		if (button != 0)
		{
			return false;
		}
		if (_target == null)
		{
			if (!(gameObject.GetComponentNonAlloc<AbstractUnitEntityView>()?.Data is BaseUnitEntity baseUnitEntity))
			{
				return false;
			}
			_target = baseUnitEntity;
			Notify("Remote Teleport: '" + baseUnitEntity.CharacterName + "' selected — click on destination");
			return true;
		}
		CheatsTransfer.LocalTeleport(worldPosition, new BaseUnitEntity[1] { _target });
		_target = null;
		Game.Instance.Controllers.PointerController?.ClearPointerMode();
		return true;
	}

	private static void Notify(string text)
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(text, addToLog: false, WarningNotificationFormat.Attention);
		});
	}
}
