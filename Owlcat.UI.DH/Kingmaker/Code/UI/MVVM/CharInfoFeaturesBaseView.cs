using System.Collections.Generic;
using System.Linq;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharInfoFeaturesBaseView : CharInfoComponentView<CharInfoFeaturesVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoFeatureGroupPCView m_WidgetAbilitiesView;

	[SerializeField]
	private CharInfoFeatureGroupPCView m_WidgetTalentsView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private bool m_ExpandAll;

	[Header("Action Bar")]
	[SerializeField]
	private GameObject ActionBarContainer;

	[SerializeField]
	protected ActionBarPartAbilitiesBaseView m_ActionBarPartAbilitiesView;

	[SerializeField]
	private TextMeshProUGUI m_ActionBarLabel;

	[Header("Abilities")]
	[SerializeField]
	protected OwlcatMultiButton m_ActiveAbilities;

	[SerializeField]
	private TextMeshProUGUI m_ActiveAbilitiesLabel;

	[SerializeField]
	protected OwlcatMultiButton m_PassiveAbilities;

	[SerializeField]
	private TextMeshProUGUI m_PassiveAbilitiesLabel;

	[SerializeField]
	private GameObject m_NoAbilitiesContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoAbilitiesLabel;

	protected readonly ReactiveProperty<bool> ActiveAbilitiesSelected = new ReactiveProperty<bool>(value: true);

	private AccessibilityTextHelper m_TextHelper;

	private const string ActiveLayerState = "Active";

	private const string NormalLayerState = "Normal";

	public override void Initialize()
	{
		base.Initialize();
		m_ActionBarPartAbilitiesView.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_ActiveAbilitiesLabel, m_PassiveAbilitiesLabel, m_NoAbilitiesLabel);
		SetLocalizedTexts();
	}

	protected override void OnBind()
	{
		m_ScrollRect.ScrollToTop();
		m_ActionBarPartAbilitiesView.Bind(base.ViewModel.ActionBarPartAbilitiesVM);
		m_TextHelper.UpdateTextSize();
		ActiveAbilitiesSelected.Subscribe(delegate
		{
			UpdateAbilitiesSelectableView();
		}).AddTo(this);
		base.OnBind();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ActionBarPartAbilitiesView.Unbind();
		m_TextHelper.Dispose();
		base.gameObject.SetActive(value: false);
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		DrawEntities();
		UpdateNoAbilitiesContainerView();
		m_ScrollRect.ScrollToTop();
	}

	private void DrawEntities()
	{
		AutoDisposingList<CharInfoFeatureGroupVM> datas = (ActiveAbilitiesSelected.Value ? base.ViewModel.ActiveAbilities : base.ViewModel.PassiveAbilities);
		m_WidgetList.Entries?.ForEach(delegate(IBindable e)
		{
			(e as MonoBehaviour).gameObject.SetActive(value: false);
		});
		m_WidgetList.DrawMultiEntries(datas, new List<CharInfoFeatureGroupPCView> { m_WidgetAbilitiesView, m_WidgetTalentsView }).AddTo(this);
		if (m_ExpandAll)
		{
			Expand();
		}
	}

	private void Expand()
	{
		m_WidgetList.Entries.ForEach(delegate(IBindable e)
		{
			((CharInfoFeatureGroupPCView)e).Expand();
		});
	}

	private void UpdateAbilitiesSelectableView()
	{
		bool value = ActiveAbilitiesSelected.Value;
		m_ActiveAbilities.SetActiveLayer(value ? "Active" : "Normal");
		m_PassiveAbilities.SetActiveLayer(value ? "Normal" : "Active");
		ActionBarContainer.SetActive(value);
	}

	protected void SetActiveAbilitiesState(bool state)
	{
		ActiveAbilitiesSelected.Value = state;
		RefreshView();
	}

	private void SetLocalizedTexts()
	{
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		m_ActiveAbilitiesLabel.text = characterSheet.ActiveAbilitiesLabel;
		m_PassiveAbilitiesLabel.text = characterSheet.PassiveAbilitiesLabel;
		if (m_NoAbilitiesLabel != null)
		{
			m_NoAbilitiesLabel.text = characterSheet.NoAbilitiesLabel;
		}
		if (m_ActionBarLabel != null)
		{
			m_ActionBarLabel.text = characterSheet.ActionPanelLabel;
		}
	}

	private void UpdateNoAbilitiesContainerView()
	{
		if (m_NoAbilitiesContainer != null)
		{
			m_NoAbilitiesContainer.SetActive(m_WidgetList.Entries?.All((IBindable e) => ((CharInfoFeatureGroupPCView)e).IsEmpty) ?? true);
		}
	}
}
