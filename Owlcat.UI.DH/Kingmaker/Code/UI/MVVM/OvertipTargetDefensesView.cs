using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipTargetDefensesView : View<OvertipHitChanceBlockVM>
{
	private enum DefenseType
	{
		Cover
	}

	[Header("Cover")]
	[SerializeField]
	private CanvasGroup m_Cover;

	[SerializeField]
	private TextMeshProUGUI m_CoverChance;

	[SerializeField]
	private Image m_CoverHintPlace;

	private ReactiveProperty<string> m_CoverHint = new ReactiveProperty<string>();

	private StringBuilder m_StringBuilder = new StringBuilder();

	private List<CanvasGroup> m_CanvasGroups;

	private List<TextMeshProUGUI> m_Labels;

	private List<ReactiveProperty<string>> m_Hints;

	private List<string> m_Strings;

	private List<Image> m_HintPlaces;

	protected override void OnBind()
	{
		m_CanvasGroups = new List<CanvasGroup> { m_Cover };
		m_Labels = new List<TextMeshProUGUI> { m_CoverChance };
		m_Hints = new List<ReactiveProperty<string>> { m_CoverHint };
		UICombatTexts combatTexts = UIStrings.Instance.CombatTexts;
		m_Strings = new List<string> { combatTexts.Cover };
		m_HintPlaces = new List<Image> { m_CoverHintPlace };
		base.ViewModel.CoverChance.CombineLatest(base.ViewModel.IsVisible, (float chance, bool visible) => new { chance, visible }).Subscribe(value =>
		{
			SetValues(value.chance, value.visible, DefenseType.Cover);
		}).AddTo(this);
		for (int i = 0; i < Enum.GetNames(typeof(DefenseType)).Length; i++)
		{
			m_HintPlaces[i].SetHint(m_Hints[i], null, m_Labels[i].color).AddTo(this);
		}
	}

	private void SetValues(float chance, bool visible, DefenseType type)
	{
		m_CanvasGroups[(int)type].alpha = Mathf.Clamp01(chance);
		m_CanvasGroups[(int)type].blocksRaycasts = visible && chance > 0f;
		m_StringBuilder.Append(UIUtilityText.GetPercentString(chance));
		m_Labels[(int)type].text = m_StringBuilder.ToString();
		m_StringBuilder.Append(" " + m_Strings[(int)type]);
		m_Hints[(int)type].Value = m_StringBuilder.ToString();
		m_StringBuilder.Clear();
	}
}
