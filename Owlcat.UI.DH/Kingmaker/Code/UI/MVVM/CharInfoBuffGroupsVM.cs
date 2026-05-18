using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBuffGroupsVM : ViewModel
{
	private readonly ReactiveProperty<TooltipBaseTemplate> m_CriticalEffectsTooltip;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_StatusEffectsTooltip;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_DotEffectsTooltip;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_NegativeEffectsTooltip;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_PositiveEffectsTooltip;

	public UnitBuffBlockVM BuffBlockVM { get; }

	public BuffGroupsVM BuffGroupsVM { get; }

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> CriticalEffectsTooltip => m_CriticalEffectsTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> StatusEffectsTooltip => m_StatusEffectsTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> DotEffectsTooltip => m_DotEffectsTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> NegativeEffectsTooltip => m_NegativeEffectsTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> PositiveEffectsTooltip => m_PositiveEffectsTooltip;

	public CharInfoBuffGroupsVM(ReadOnlyReactiveProperty<BaseUnitEntity> entity, UnitBuffBlockVM buffBlockVM, BuffGroupsVM buffGroupsVM)
	{
		m_CriticalEffectsTooltip = new ReactiveProperty<TooltipBaseTemplate>();
		m_StatusEffectsTooltip = new ReactiveProperty<TooltipBaseTemplate>();
		m_DotEffectsTooltip = new ReactiveProperty<TooltipBaseTemplate>();
		m_NegativeEffectsTooltip = new ReactiveProperty<TooltipBaseTemplate>();
		m_PositiveEffectsTooltip = new ReactiveProperty<TooltipBaseTemplate>();
		BuffBlockVM = buffBlockVM;
		BuffGroupsVM = buffGroupsVM;
		entity.Subscribe(SetUnit).AddTo(this);
	}

	private void SetUnit(MechanicEntity entity)
	{
		m_CriticalEffectsTooltip.Value = new TooltipTemplateBuffs(entity, BuffGroupsVM, BuffGroupFlags.CriticalEffects);
		m_StatusEffectsTooltip.Value = new TooltipTemplateBuffs(entity, BuffGroupsVM, BuffGroupFlags.StatusEffects);
		m_DotEffectsTooltip.Value = new TooltipTemplateBuffs(entity, BuffGroupsVM, BuffGroupFlags.DotEffects);
		m_NegativeEffectsTooltip.Value = new TooltipTemplateBuffs(entity, BuffGroupsVM, BuffGroupFlags.NegativeEffects);
		m_PositiveEffectsTooltip.Value = new TooltipTemplateBuffs(entity, BuffGroupsVM, BuffGroupFlags.PositiveEffects);
	}
}
