using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartAbilities : UnitInfoPart
{
	[SerializeField]
	private UnitInfoAbilityView m_AbilityView;

	[SerializeField]
	private UnitInfoAbilityGroupView[] m_Groups;

	private readonly List<UnitInfoAbilityView> m_AbilitiesList = new List<UnitInfoAbilityView>();

	private bool HasAbilities => m_AbilitiesList.Any();

	private void Awake()
	{
		UnitInfoAbilityGroupView[] groups = m_Groups;
		foreach (UnitInfoAbilityGroupView unitInfoAbilityGroupView in groups)
		{
			unitInfoAbilityGroupView.SetHeader(GetGroupHeader(unitInfoAbilityGroupView.Group));
		}
	}

	private string GetGroupHeader(AbilityUIGroup group)
	{
		return group switch
		{
			AbilityUIGroup.Active => UIStrings.Instance.Inspect.ActiveAbilitiesShortTitle.Text, 
			AbilityUIGroup.Passive => UIStrings.Instance.Inspect.PassiveAbilitiesShortTitle.Text, 
			_ => throw new ArgumentOutOfRangeException("group", group, null), 
		};
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Abilities.Subscribe(DrawAbilities).AddTo(this);
	}

	private void DrawAbilities(IReadOnlyList<UnitInfoAbilityVM> abilities)
	{
		DrawAbilitiesInternal(abilities);
		int num = m_Groups.Length;
		for (int i = 0; i < num; i++)
		{
			UnitInfoAbilityGroupView unitInfoAbilityGroupView = m_Groups[i];
			unitInfoAbilityGroupView.SetActive(base.ViewModel.HasAbilityGroup(unitInfoAbilityGroupView.Group));
			unitInfoAbilityGroupView.SetSeparator(i + 1 < num);
		}
	}

	private void DrawAbilitiesInternal(IReadOnlyList<UnitInfoAbilityVM> abilities)
	{
		Clear();
		foreach (UnitInfoAbilityVM ability in abilities)
		{
			UnitInfoAbilityView widget = WidgetFactory.GetWidget(m_AbilityView);
			widget.Bind(ability);
			RectTransform abilityParent = GetAbilityParent(ability.Group);
			widget.transform.SetParent(abilityParent, worldPositionStays: false);
			m_AbilitiesList.Add(widget);
		}
	}

	private RectTransform GetAbilityParent(AbilityUIGroup group)
	{
		return m_Groups.FirstOrDefault((UnitInfoAbilityGroupView g) => g.Group == group)?.Container;
	}

	private void Clear()
	{
		m_AbilitiesList.ForEach(WidgetFactory.DisposeWidget);
		m_AbilitiesList.Clear();
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		bool active = !base.ViewModel.IsPreciseAttack.CurrentValue && !state.HasHit && HasAbilities && !state.IsDeadOrUnconscious;
		base.gameObject.SetActive(active);
	}
}
