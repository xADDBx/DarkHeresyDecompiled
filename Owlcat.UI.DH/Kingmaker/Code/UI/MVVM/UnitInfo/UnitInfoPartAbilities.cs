using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using ObservableCollections;
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
		DrawAbilities();
		base.ViewModel.Abilities.ObserveAdd().Subscribe(delegate
		{
			DrawAbilities();
		}).AddTo(this);
		base.ViewModel.Abilities.ObserveRemove().Subscribe(delegate
		{
			DrawAbilities();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Abilities.ObserveReset(), delegate
		{
			Clear();
		}).AddTo(this);
	}

	private void DrawAbilities()
	{
		DrawAbilitiesInternal(base.ViewModel.Abilities.ToList(), m_AbilitiesList);
		int num = m_Groups.Length;
		for (int i = 0; i < num; i++)
		{
			UnitInfoAbilityGroupView groupView = m_Groups[i];
			groupView.SetActive(m_AbilitiesList.Any((UnitInfoAbilityView b) => b.Group == groupView.Group));
			groupView.SetSeparator(i + 1 < num);
		}
	}

	private void DrawAbilitiesInternal(List<(Sprite sprite, AbilityUIGroup group)> abilities, List<UnitInfoAbilityView> views)
	{
		Clear();
		foreach (var ability in abilities)
		{
			UnitInfoAbilityView widget = WidgetFactory.GetWidget(m_AbilityView);
			widget.Initialize(ability);
			RectTransform abilityParent = GetAbilityParent(ability.group);
			widget.transform.SetParent(abilityParent, worldPositionStays: false);
			views.Add(widget);
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
