using Code.View.UI.Helpers;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoStatusEffectsView : CharInfoComponentView<CharInfoStatusEffectsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_StatusEffectsTitle;

	[Header("No Status Effects")]
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_NoStatusContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoStatusEffectsLabel;

	[Header("Widget Collection")]
	[SerializeField]
	protected ScrollRectExtended m_Scroll;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private StatusEffectBaseView m_WidgetEntityView;

	private AccessibilityTextHelper m_TextHelper;

	private UIStrings t => UIStrings.Instance;

	public override void Initialize()
	{
		base.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_StatusEffectsTitle, m_NoStatusEffectsLabel);
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetupLabels();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}

	private void SetupLabels()
	{
		m_StatusEffectsTitle.text = t.CharacterSheet.StatusEffects;
		m_NoStatusEffectsLabel.text = t.CharacterSheet.NoBuffText;
		m_TextHelper.UpdateTextSize();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		DrawEntities();
		DrawNoBuffsLabel();
		m_Scroll.ScrollToTop();
	}

	private void DrawEntities()
	{
		if (base.ViewModel.BuffsGroup != null)
		{
			m_WidgetList.DrawEntries(base.ViewModel.BuffsGroup.FeatureList, m_WidgetEntityView, unused: true);
		}
	}

	private void DrawNoBuffsLabel()
	{
		if (base.ViewModel.NoBuffs)
		{
			m_NoStatusContainer.AppearAnimation();
		}
		else
		{
			m_NoStatusContainer.DisappearAnimation();
		}
	}
}
