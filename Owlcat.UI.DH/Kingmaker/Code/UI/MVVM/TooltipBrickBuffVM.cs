using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuffVM : TooltipBrickFeatureVM
{
	private readonly ReactiveProperty<string> m_Duration = new ReactiveProperty<string>();

	public string SourceName;

	public string Stack;

	public bool IsDOT;

	public string DOTDesc;

	public string DOTDamage;

	private readonly Buff m_Buff;

	public ReadOnlyReactiveProperty<string> Duration => m_Duration;

	public TooltipBrickBuffVM(Buff buff)
	{
		m_Buff = buff;
		Name = buff.Name;
		if (buff.Icon != null)
		{
			Icon = buff.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(buff.Name);
			IconColor = UIUtilityText.GetColorByText(buff.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(buff.Name);
		}
		SourceName = (m_Buff?.Context?.MaybeCaster as BaseUnitEntity)?.CharacterName ?? string.Empty;
		Tooltip = new TooltipTemplateBuff(buff);
		AvailableBackground = true;
		Stack = ((m_Buff != null && m_Buff.Blueprint.MaxRank > 1) ? (m_Buff.GetRank() + "/" + m_Buff.Blueprint.MaxRank) : string.Empty);
		DOTLogicVisual dOTLogicVisual = m_Buff?.Blueprint?.GetComponent<DOTLogicVisual>();
		IsDOT = dOTLogicVisual != null;
		if (IsDOT)
		{
			DOTDesc = UIUtilityText.UpdateDescriptionWithUIProperties(m_Buff?.Description, ((IBuff)m_Buff)?.Caster);
			DOTDamage = CalculateDOTDamage(dOTLogicVisual);
		}
		AddDisposable(ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			m_Duration.Value = BuffTooltipUtils.GetDuration(m_Buff);
		}));
	}

	private string CalculateDOTDamage(DOTLogicVisual dotLogicVisual)
	{
		Buff buff = null;
		DOTLogic dOTLogic = null;
		foreach (Buff item in m_Buff.Owner.Buffs.Enumerable)
		{
			dOTLogic = item.Blueprint?.GetComponent<DOTLogic>();
			if (dOTLogic != null && dOTLogic.Type == dotLogicVisual.Type)
			{
				buff = item;
				break;
			}
		}
		if (dOTLogic == null || buff == null)
		{
			return string.Empty;
		}
		return DOTLogic.CalculateDamage(buff, dOTLogic).AverageValue.ToString();
	}
}
