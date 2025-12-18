using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsBaseView : View<DlcManagerTabModsVM>
{
	[Header("Common")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected RectTransform m_BottomBlock;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_InstalledModsHeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_DiscoverModsLabel;

	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveAnyModsLabel;

	[SerializeField]
	protected OwlcatButton m_NexusModsButton;

	[SerializeField]
	private TextMeshProUGUI m_NexusModsLabel;

	[SerializeField]
	protected OwlcatButton m_SteamWorkshopButton;

	[SerializeField]
	private TextMeshProUGUI m_SteamWorkshopLabel;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		ScrollToTop();
		base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				ScrollToTop();
				base.ViewModel.SelectedEntity.CurrentValue?.ShowDescription(state: true);
			}
		}).AddTo(this);
		m_InstalledModsHeaderLabel.text = UIStrings.Instance.DlcManager.InstalledMods;
		m_YouDontHaveAnyModsLabel.text = UIStrings.Instance.DlcManager.YouDontHaveAnyMods;
		m_YouDontHaveAnyModsLabel.transform.parent.gameObject.SetActive(!base.ViewModel.HaveMods);
		if (base.ViewModel.SelectionGroup.EntitiesCollection.Any())
		{
			m_InfoView.Bind(base.ViewModel.InfoVM);
		}
		SetBottomButtons();
	}

	private void SetBottomButtons()
	{
		m_DiscoverModsLabel.text = UIStrings.Instance.DlcManager.DiscoverMoreMods;
		m_NexusModsLabel.text = UIStrings.Instance.DlcManager.NexusMods;
		m_SteamWorkshopButton.gameObject.SetActive(base.ViewModel.IsSteam.CurrentValue);
		if (base.ViewModel.IsSteam.CurrentValue)
		{
			m_SteamWorkshopLabel.text = UIStrings.Instance.DlcManager.SteamWorkshop;
		}
		SetBottomButtonsImpl();
	}

	protected virtual void SetBottomButtonsImpl()
	{
	}

	public void ScrollToTop()
	{
		m_ScrollRect.Or(null)?.ScrollToTop();
		m_InfoView.Or(null)?.ScrollRectExtended.Or(null)?.ScrollToTop();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.Or(null)?.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	protected void OpenNexusMods()
	{
		base.ViewModel.OpenNexusMods();
	}

	protected void OpenSteamWorkshop()
	{
		base.ViewModel.OpenSteamWorkshop();
	}
}
