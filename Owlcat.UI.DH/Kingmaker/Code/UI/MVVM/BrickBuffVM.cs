using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickBuffVM : BrickFeatureVM, IBuffListItemVM, IDisposable
{
	private readonly Buff m_Buff;

	private readonly ReactiveProperty<string> m_Duration = new ReactiveProperty<string>();

	private int m_LastExpirationInRounds = int.MinValue;

	public readonly bool IsDOT;

	public readonly string DOTDesc;

	public readonly string DOTDamage;

	string IBuffListItemVM.Name => Name;

	public string SourceName { get; }

	public string Stack { get; }

	Sprite IBuffListItemVM.Icon => Icon;

	TooltipBaseTemplate IBuffListItemVM.Tooltip => Tooltip;

	public Buff Buff => m_Buff;

	public ReadOnlyReactiveProperty<string> Duration => m_Duration;

	public BrickBuffVM(Buff buff)
		: base(buff.Name, GetIcon(buff), new TooltipTemplateBuff(buff))
	{
		m_Buff = buff;
		if (buff.Icon != null)
		{
			IconColor = Color.white;
		}
		else
		{
			IconColor = UIUtilityText.GetColorByText(buff.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(buff.Name);
		}
		SourceName = m_Buff.GetSourceName();
		AvailableBackground = true;
		Stack = m_Buff.GetStacksText();
		DOTLogicVisual dOTLogicVisual = m_Buff?.Blueprint?.GetComponent<DOTLogicVisual>();
		IsDOT = dOTLogicVisual != null;
		if (IsDOT)
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(((IBuff)m_Buff)?.Caster);
				DOTDesc = m_Buff?.Description;
			}
			DOTDamage = CalculateDOTDamage(dOTLogicVisual);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			int expirationInRounds = m_Buff.ExpirationInRounds;
			if (expirationInRounds != m_LastExpirationInRounds)
			{
				m_LastExpirationInRounds = expirationInRounds;
				m_Duration.Value = m_Buff.GetDurationText();
			}
		}).AddTo(this);
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

	private static Sprite GetIcon(Buff buff)
	{
		if (!(buff.Icon != null))
		{
			return UIUtilityText.GetIconByText(buff.Name);
		}
		return buff.Icon;
	}
}
