using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartMorale : UnitInfoPart
{
	[SerializeField]
	private UnitInfoPartHeader m_Header;

	[SerializeField]
	private DoubleSidedSegmentedProgressBar m_MoraleProgressBar;

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
		if (state.IsDeadOrUnconscious || state.PreciseAttackHasNoTarget || !state.HasMorale || base.ViewModel.HasSettingsFlags(UnitInspectUIFlags.HideMorale))
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
		m_MoraleProgressBar.SetValue(morale.min, morale.max, morale.current);
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
