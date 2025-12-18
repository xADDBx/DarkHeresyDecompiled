using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipConcentrationActionVM : ViewModel
{
	private readonly MechanicEntityUIState MechanicEntityUIState;

	private readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<bool> m_HasAction = new ReactiveProperty<bool>();

	private TooltipTemplateBuff m_ActionAbilityTooltip;

	private Buff m_Buff;

	public TooltipTemplateBuff ActionAbilityTooltip
	{
		get
		{
			if (m_ActionAbilityTooltip != null)
			{
				return m_ActionAbilityTooltip;
			}
			m_ActionAbilityTooltip = new TooltipTemplateBuff(m_Buff, null, isConcentration: true);
			return m_ActionAbilityTooltip;
		}
	}

	public ReadOnlyReactiveProperty<Sprite> Icon => m_Icon;

	public ReadOnlyReactiveProperty<bool> HasAction => m_HasAction;

	public OvertipConcentrationActionVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		MechanicEntityUIState.ConcentrationBuff.Subscribe(UpdateAbility).AddTo(this);
	}

	private void UpdateAbility(Buff buff)
	{
		m_Buff = buff;
		m_Icon.Value = buff?.Icon;
		m_HasAction.Value = m_Buff != null;
		m_ActionAbilityTooltip = null;
	}
}
