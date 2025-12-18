using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("e9789e02cf4144c7a337fd494b44b67c")]
public class EtudeBracketForbidOpenContracts : EtudeBracketTrigger
{
	protected override void OnEnter()
	{
		Game.Instance.Player.CannotAccessContracts.Retain();
		EventBus.RaiseEvent(delegate(ICanAccessContractsHandler h)
		{
			h.HandleCanAccessContractsChanged();
		});
	}

	protected override void OnExit()
	{
		Game.Instance.Player.CannotAccessContracts.Release();
		EventBus.RaiseEvent(delegate(ICanAccessContractsHandler h)
		{
			h.HandleCanAccessContractsChanged();
		});
	}
}
