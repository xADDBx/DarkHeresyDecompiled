using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBuffBlockView : View<UnitBuffBlockVM>
{
	[SerializeField]
	private bool m_HasVisibilityControl;

	[ShowIf("m_HasVisibilityControl")]
	[SerializeField]
	private CanvasGroup m_MainGroup;

	[SerializeField]
	private UnitBuffBlockGroupElement[] m_Groups;

	[SerializeField]
	private bool m_IsVertical;

	[SerializeField]
	private float m_Spacing;

	private Vector2 m_PivotStart;

	private Vector2 m_ElementSize;

	public bool HasBuffs => m_Groups.Any((UnitBuffBlockGroupElement g) => g.IsActive);

	private void Awake()
	{
		if (m_Groups.Length != 0)
		{
			m_PivotStart = m_Groups[0].RectTransform.anchoredPosition;
			m_ElementSize = m_Groups[0].RectTransform.rect.size;
		}
	}

	protected override void OnBind()
	{
		UnitBuffBlockGroupElement[] groups = m_Groups;
		foreach (UnitBuffBlockGroupElement unitBuffBlockGroupElement in groups)
		{
			unitBuffBlockGroupElement.SetHint(GetHint(unitBuffBlockGroupElement.Group)).AddTo(this);
		}
		DrawBuffs();
		base.ViewModel.Buffs.ObserveAdd().Subscribe(delegate
		{
			DrawBuffs();
		}).AddTo(this);
		base.ViewModel.Buffs.ObserveRemove().Subscribe(delegate
		{
			DrawBuffs();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Buffs.ObserveReset(), delegate
		{
			Clear();
		}).AddTo(this);
	}

	public void SetVisible(bool visible)
	{
		if (m_HasVisibilityControl && !(m_MainGroup == null))
		{
			m_MainGroup.alpha = (visible ? 1f : 0f);
		}
	}

	private void DrawBuffs()
	{
		Clear();
		base.ViewModel.SortBuffs();
		Vector2 vector = (m_IsVertical ? new Vector2(0f, 0f - (m_ElementSize.y + m_Spacing)) : new Vector2(m_ElementSize.x + m_Spacing, 0f));
		int num = 0;
		UnitBuffBlockGroupElement[] groups = m_Groups;
		foreach (UnitBuffBlockGroupElement groupElement in groups)
		{
			bool flag = base.ViewModel.Buffs.Any((BuffVM b) => b.Group == groupElement.Group);
			groupElement.SetActive(flag);
			if (flag)
			{
				groupElement.RectTransform.anchoredPosition = m_PivotStart + vector * num;
				num++;
			}
		}
	}

	private void Clear()
	{
		UnitBuffBlockGroupElement[] groups = m_Groups;
		for (int i = 0; i < groups.Length; i++)
		{
			groups[i].SetActive(active: false);
		}
	}

	private string GetHint(BuffGroupType group)
	{
		return group switch
		{
			BuffGroupType.Positive => string.Format(UIStrings.Instance.Inspect.HasStatusEffects.Text, UIStrings.Instance.Inspect.EffectsPositive.Text), 
			BuffGroupType.Negative => string.Format(UIStrings.Instance.Inspect.HasStatusEffects.Text, UIStrings.Instance.Inspect.EffectsNegative.Text), 
			BuffGroupType.DOT => string.Format(UIStrings.Instance.Inspect.HasStatusEffects.Text, UIStrings.Instance.Inspect.EffectsDOT.Text), 
			BuffGroupType.CriticalEffect => string.Format(UIStrings.Instance.Inspect.HasStatusEffects.Text, UIStrings.Instance.Inspect.EffectsCritical.Text), 
			_ => throw new ArgumentOutOfRangeException("group", group, null), 
		};
	}
}
