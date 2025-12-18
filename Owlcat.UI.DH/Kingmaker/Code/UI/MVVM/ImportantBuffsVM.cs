using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ImportantBuffsVM : ViewModel, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	private readonly MechanicEntity m_TargetEntity;

	private TurnController TurnController => Game.Instance.Controllers.TurnController;

	public ImportantBuffsVM(MechanicEntity target)
	{
		m_TargetEntity = target;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
	}
}
