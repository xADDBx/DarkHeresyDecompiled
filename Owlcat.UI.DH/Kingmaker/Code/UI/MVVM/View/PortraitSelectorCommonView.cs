using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class PortraitSelectorCommonView : View<CharGenPortraitsSelectorVM>, ICharGenAppearancePhasePortraitHandler, ISubscriber
{
	[SerializeField]
	private CharGenPortraitTabSelectorView m_TabSelector;

	[SerializeField]
	private WidgetList m_WidgetListDefaultGroups;

	[SerializeField]
	private CharGenDefaultPortraitGroupView m_DefaultGroupPrefab;

	[SerializeField]
	private CharGenCustomPortraitGroupView m_CustomPortraitGroup;

	[SerializeField]
	private CharGenCustomPortraitCreatorView m_CustomPortraitCreatorView;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	[Header("Containers")]
	[SerializeField]
	private GameObject m_PortraitSelectorContainer;

	[SerializeField]
	private GameObject m_DefaultPortraitsContainer;

	[SerializeField]
	private GameObject m_CustomPortraitsContainer;

	[SerializeField]
	private GameObject m_CustomPortraitsInfoContainer;

	[Header("Controls")]
	[SerializeField]
	private OwlcatMultiButton m_ChangePortraitButton;

	[SerializeField]
	private TextMeshProUGUI m_ChangePortraitLabel;

	[SerializeField]
	private TextMeshProUGUI m_ChangePortraitDescription;

	private bool m_IsInit;

	void ICharGenAppearancePhasePortraitHandler.HandlePortraitTabChange(CharGenPortraitTab tab)
	{
		base.ViewModel.SetCurrentTab(tab);
		m_DefaultPortraitsContainer.SetActive(tab == CharGenPortraitTab.Default);
		m_CustomPortraitsContainer.SetActive(tab == CharGenPortraitTab.Custom);
		m_CustomPortraitsInfoContainer.SetActive(tab == CharGenPortraitTab.Custom);
	}

	private void Initialize()
	{
		if (!m_IsInit)
		{
			m_TabSelector.gameObject.SetActive(value: true);
			m_CustomPortraitCreatorView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Initialize();
		EventBus.Subscribe(this).AddTo(this);
		m_TabSelector.Bind(base.ViewModel.TabSelector);
		m_WidgetListDefaultGroups.DrawEntries(base.ViewModel.PortraitGroupVms.Values.OrderBy((CharGenPortraitGroupVM gr) => gr.PortraitCategory), m_DefaultGroupPrefab).AddTo(this);
		m_CustomPortraitGroup.Bind(base.ViewModel.CustomPortraitGroup);
		base.ViewModel.CustomPortraitCreatorVM.Subscribe(delegate(CharGenCustomPortraitCreatorVM value)
		{
			m_CustomPortraitCreatorView.Bind(value);
			m_ScrollRectExtended.ScrollToTop();
		}).AddTo(this);
		if ((bool)m_ChangePortraitButton)
		{
			SetChangePortraitButton();
		}
		base.ViewModel.CurrentTab.Subscribe(delegate(CharGenPortraitTabVM tab)
		{
			OnSwitchTab(tab.Tab);
		}).AddTo(this);
		SetupLabels();
	}

	private void SetChangePortraitButton()
	{
		bool flag = UtilityNet.IsControlMainCharacter();
		m_ChangePortraitButton.Or(null)?.SetInteractable(flag);
		if (flag)
		{
			base.ViewModel.PortraitVM.Subscribe(delegate(PortraitVM value)
			{
				m_ChangePortraitButton.SetInteractable(value.PortraitData.IsCustom);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_ChangePortraitButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.OpenCustomPortraitCreator();
			}).AddTo(this);
		}
	}

	private void OnSwitchTab(CharGenPortraitTab tab)
	{
		Game.Instance.GameCommandQueue.CharGenSwitchPortraitTab(tab);
	}

	private void SetupLabels()
	{
		if ((bool)m_ChangePortraitLabel)
		{
			m_ChangePortraitLabel.text = UIStrings.Instance.CharGen.ChangePortrait;
		}
		if ((bool)m_ChangePortraitDescription)
		{
			string text = (Game.Instance.IsControllerMouse ? UIStrings.Instance.CharGen.ChangePortraitDescription : UIStrings.Instance.CharGen.ChangePortraitDescriptionConsole);
			m_ChangePortraitDescription.text = text;
		}
	}
}
