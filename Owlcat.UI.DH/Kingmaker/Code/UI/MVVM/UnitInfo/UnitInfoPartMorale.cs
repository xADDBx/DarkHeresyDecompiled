using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartMorale : UnitInfoPart
{
	[SerializeField]
	private BaseProgressBar<int> m_MoraleProgressPositive;

	[SerializeField]
	private BaseProgressBar<int> m_MoraleProgressNegative;

	[SerializeField]
	private UnitInfoPartHeader m_Header;

	[Space]
	[SerializeField]
	private UnitInfoPartHeader.Colors m_NeutralColors;

	[SerializeField]
	private UnitInfoPartHeader.Colors m_PositiveColors;

	[SerializeField]
	private UnitInfoPartHeader.Colors m_NegativeColors;

	[SerializeField]
	private UnitInfoPartHeader.Colors m_PositiveMaxColors;

	[SerializeField]
	private UnitInfoPartHeader.Colors m_NegativeMaxColors;

	private void Awake()
	{
		m_Header.SetTitle(UIStrings.Instance.HUDTexts.MoraleTitle.Text);
	}

	protected override void OnBind()
	{
		base.ViewModel.Data.MoraleValue.Subscribe(SetMoraleValue).AddTo(this);
		base.OnBind();
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		base.gameObject.SetActive(CanShowPart(state));
	}

	private bool CanShowPart(UnitInfoPartState state)
	{
		if (state.IsDeadOrUnconscious || state.PreciseAttackHasNoTarget)
		{
			return false;
		}
		if (state.HasHit)
		{
			if (!state.HasDamage)
			{
				return !state.HasHeal;
			}
			return false;
		}
		return true;
	}

	private void SetMoraleValue((int min, int max, int current) morale)
	{
		m_Header.SetValue(morale.current.ToString());
		m_MoraleProgressPositive.SetLimits(0, morale.max);
		m_MoraleProgressNegative.SetLimits(0, Mathf.Abs(morale.min));
		bool flag = morale.current >= 0;
		m_MoraleProgressPositive.SetValue(flag ? morale.current : 0);
		m_MoraleProgressNegative.SetValue((!flag) ? Mathf.Abs(morale.current) : 0);
		SetColors(morale.min, morale.max, morale.current);
	}

	private void SetColors(int min, int max, int current)
	{
		if (current == 0)
		{
			m_Header.SetColors(m_NeutralColors);
			return;
		}
		if (current <= min)
		{
			m_Header.SetColors(m_NegativeMaxColors);
			return;
		}
		if (current >= max)
		{
			m_Header.SetColors(m_PositiveMaxColors);
			return;
		}
		UnitInfoPartHeader.Colors colors = ((current >= 0) ? m_PositiveColors : m_NegativeColors);
		m_Header.SetColors(colors);
	}
}
