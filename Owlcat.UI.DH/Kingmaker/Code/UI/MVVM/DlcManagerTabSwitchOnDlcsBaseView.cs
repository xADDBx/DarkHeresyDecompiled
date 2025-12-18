using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsBaseView : View<DlcManagerTabSwitchOnDlcsVM>
{
	[Header("Common")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_InstalledDlcsHeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveAnyDlcsLabel;

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
		UIDlcManager dlcManager = UIStrings.Instance.DlcManager;
		m_InstalledDlcsHeaderLabel.text = dlcManager.InstalledDlcs;
		m_YouDontHaveAnyDlcsLabel.text = dlcManager.YouDontHaveAnyInstalledDlcs;
		m_YouDontHaveAnyDlcsLabel.transform.parent.gameObject.SetActive(!base.ViewModel.HaveDlcs);
		if (base.ViewModel.SelectionGroup.EntitiesCollection.Any())
		{
			m_InfoView.Bind(base.ViewModel.InfoVM);
		}
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
}
