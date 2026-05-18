using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerSquadView : View<InitiativeTrackerMechanicEntityVM>
{
	[SerializeField]
	private CombatUnitEntityView m_EntityViewPrefab;

	[SerializeField]
	private LayoutGroup m_UnitsLayout;

	[SerializeField]
	private Vector2 m_UnitSize;

	private readonly List<(CombatMechanicEntityVM model, CombatUnitEntityView view)> m_Elements = new List<(CombatMechanicEntityVM, CombatUnitEntityView)>();

	private bool m_IsFocused;

	public Vector2 GetSize(int squadCount)
	{
		LayoutGroup unitsLayout = m_UnitsLayout;
		if (!(unitsLayout is HorizontalLayoutGroup layout2))
		{
			if (unitsLayout is VerticalLayoutGroup layout3)
			{
				return new Vector2(m_UnitSize.x, GetSizeInternal(layout3, squadCount, m_UnitSize.y));
			}
			throw new ArgumentException($"Unhandled layout type: {m_UnitsLayout.GetType()}");
		}
		return new Vector2(GetSizeInternal(layout2, squadCount, m_UnitSize.x), m_UnitSize.y);
		static float GetSizeInternal(HorizontalOrVerticalLayoutGroup layout, int count, float elementSize)
		{
			float num = ((layout is HorizontalLayoutGroup horizontalLayoutGroup) ? ((float)(horizontalLayoutGroup.padding.left + horizontalLayoutGroup.padding.right)) : ((layout is VerticalLayoutGroup verticalLayoutGroup) ? ((float)(verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom)) : 0f));
			return elementSize * (float)count + layout.spacing * (float)(count - 1) + num;
		}
	}

	public RectTransform GetUnitNameAnchor(MechanicEntity entity)
	{
		foreach (var (combatMechanicEntityVM, combatUnitEntityView) in m_Elements)
		{
			if (combatMechanicEntityVM.MechanicEntity == entity)
			{
				return combatUnitEntityView.UnitNameAnchor;
			}
		}
		return null;
	}

	protected override void OnBind()
	{
		Observable<bool> observable = new ReactiveProperty<bool>().AddTo(this);
		foreach (UnitReference unit in base.ViewModel.Squad.Units)
		{
			BaseUnitEntity baseUnitEntity = unit.ToBaseUnitEntity();
			if (baseUnitEntity != null && !baseUnitEntity.IsDeadOrUnconscious)
			{
				CombatUnitEntityView widget = WidgetFactory.GetWidget(m_EntityViewPrefab, activate: false, strictMatching: true);
				Transform obj = widget.transform;
				obj.SetParent(m_UnitsLayout.transform, worldPositionStays: false);
				obj.localScale = Vector3.one;
				CombatMechanicEntityVM combatMechanicEntityVM = new CombatMechanicEntityVM(baseUnitEntity, null);
				m_Elements.Add((combatMechanicEntityVM, widget));
				widget.Bind(combatMechanicEntityVM);
				observable = observable.CombineLatest(widget.IsHovered, (bool lhs, bool rhs) => lhs || rhs);
			}
		}
		observable.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(SetFocused).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		foreach (var (combatMechanicEntityVM, widget) in m_Elements)
		{
			combatMechanicEntityVM.Dispose();
			WidgetFactory.DisposeWidget(widget);
		}
		m_Elements.Clear();
		m_IsFocused = false;
	}

	private void SetFocused(bool isFocused)
	{
		if (m_IsFocused == isFocused)
		{
			return;
		}
		m_IsFocused = isFocused;
		foreach (var element in m_Elements)
		{
			element.view.SetFocused(isFocused);
		}
	}
}
