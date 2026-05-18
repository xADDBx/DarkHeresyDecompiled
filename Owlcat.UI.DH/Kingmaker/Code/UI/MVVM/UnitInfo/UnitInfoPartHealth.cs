using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartHealth : UnitInfoPart
{
	[SerializeField]
	private UnitInfoPartHeader m_Durability;

	[SerializeField]
	private UnitInfoPartHeader m_Wounds;

	[SerializeField]
	private UnitInfoPartHeader.Colors m_AllyColors;

	[SerializeField]
	private UnitInfoPartHeader.Colors m_EnemyColors;

	private void Awake()
	{
		m_Wounds.SetTitle(UIStrings.Instance.Inspect.Wounds.Text);
		m_Durability.SetTitle(UIStrings.Instance.Inspect.Durability.Text);
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.HitPointLeft.CombineLatest(base.ViewModel.Data.HitPointMax, (int left, int max) => (left: left, max: max)).Subscribe(SetWounds).AddTo(this);
		base.ViewModel.Data.ArmorLeft.CombineLatest(base.ViewModel.Data.ArmorMax, (int left, int max) => (left: left, max: max)).Subscribe(SetDurability).AddTo(this);
		base.ViewModel.Data.IsEnemy.Subscribe(SetFaction).AddTo(this);
	}

	private void SetFaction(bool isEnemy)
	{
		UnitInfoPartHeader.Colors colors = (isEnemy ? m_EnemyColors : m_AllyColors);
		m_Wounds.SetColors(colors);
	}

	private void SetWounds((int left, int max) wounds)
	{
		if (base.ViewModel.IsCountHpAsArmor)
		{
			m_Wounds.gameObject.SetActive(value: false);
			return;
		}
		m_Wounds.gameObject.SetActive(value: true);
		if (base.ViewModel.HideRealHealthInUI)
		{
			m_Wounds.SetValue("???");
		}
		else
		{
			m_Wounds.SetValue($"{wounds.left}/<size=74%>{wounds.max}</size>");
		}
	}

	private void SetDurability((int left, int max) durability)
	{
		if (!base.ViewModel.IsCountHpAsArmor && durability.max <= 0)
		{
			m_Durability.gameObject.SetActive(value: false);
		}
		else if (base.ViewModel.HideRealHealthInUI)
		{
			m_Durability.SetValue("???");
			m_Durability.gameObject.SetActive(value: true);
		}
		else
		{
			m_Durability.SetValue($"{durability.left}/<size=74%>{durability.max}</size>");
			m_Durability.gameObject.SetActive(value: true);
		}
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		bool active = !state.HasHit && !state.PreciseAttackHasNoTarget && !base.ViewModel.HasSettingsFlags(UnitInspectUIFlags.HideUnitInfo);
		base.gameObject.SetActive(active);
	}
}
