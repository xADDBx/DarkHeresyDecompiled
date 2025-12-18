using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.CombatText;

public class CombatTextBlockVM : ViewModel, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>
{
	private readonly ReactiveCommand<CombatMessageBase> m_CombatMessage = new ReactiveCommand<CombatMessageBase>();

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public Observable<CombatMessageBase> CombatMessage => m_CombatMessage;

	private MechanicEntity m_MechanicEntity => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	public CombatTextBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (!evt.Ability.Blueprint.DisableLog && Rulebook.CurrentContext.Current != null && Rulebook.CurrentContext.Current.Initiator == m_MechanicEntity)
		{
			m_CombatMessage.Execute(new CombatMessageAbility
			{
				Name = evt.Ability.Blueprint.Name,
				Sprite = evt.Ability.Blueprint.Icon
			});
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null && mechanicEntity == m_MechanicEntity && !mechanicEntity.CanAct)
		{
			BlueprintBuff disabledCommonBuff = ConfigRoot.Instance.SystemMechanics.DisabledCommonBuff;
			if (mechanicEntity.Buffs?.GetBuff(disabledCommonBuff) != null)
			{
				m_CombatMessage.Execute(new CombatMessageAbility
				{
					Name = disabledCommonBuff.Name,
					Sprite = disabledCommonBuff.Icon,
					BigSprite = ConfigRoot.Instance.UIConfig.UIIcons.CantAct
				});
			}
		}
	}
}
