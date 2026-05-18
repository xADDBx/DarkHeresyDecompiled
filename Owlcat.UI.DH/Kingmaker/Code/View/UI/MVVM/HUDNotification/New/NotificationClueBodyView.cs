using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationClueBodyView : View<NotificationClueBodyVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_ClueName;

	[SerializeField]
	private TMP_Text m_AddendumsCount;

	[SerializeField]
	private GameObject m_AddendumsCountContainer;

	[SerializeField]
	private TypeToPrefabSelector<Image> m_IconsSelector;

	[SerializeField]
	private TypeToPrefabSelector<GameObject> m_IconAdditions;

	[SerializeField]
	private Image m_CluTooltipParent;

	[SerializeField]
	private RectTransform m_IconRectTransform;

	[SerializeField]
	private TMP_Text m_NewClueLabel;

	[SerializeField]
	private TMP_Text m_NewAddendumsLabel;

	[Header("Values")]
	[SerializeField]
	private Sprite m_DefaultIcon;

	private Image m_CurrentIcon;

	private GameObject m_CurrentIconAddition;

	protected override void OnBind()
	{
		m_ClueName.text = base.ViewModel.BlueprintClue.GetUIData().Name.Text;
		m_NewClueLabel.text = UIStrings.Instance.CaseNotificationTexts.NewClueLabel.Text;
		m_NewAddendumsLabel.text = UIStrings.Instance.CaseNotificationTexts.NewAddendumsLabel.Text;
		m_CurrentIcon = m_IconsSelector.GetEntity(base.ViewModel.BlueprintClue.UIClueType);
		m_CurrentIcon.sprite = base.ViewModel.BlueprintClue.GetUIData().Icon ?? m_DefaultIcon;
		m_CurrentIcon.gameObject.SetActive(value: true);
		m_CurrentIconAddition = m_IconAdditions.GetEntity(base.ViewModel.BlueprintClue.UIClueType);
		m_CurrentIconAddition.Or(null)?.SetActive(value: true);
		base.ViewModel.Addendums.ObserveCountChanged().Subscribe(delegate
		{
			UpdateAddendumsCount();
		}).AddTo(this);
		base.ViewModel.IsNewClue.Subscribe(delegate(bool value)
		{
			m_NewClueLabel.gameObject.SetActive(value);
			m_NewAddendumsLabel.gameObject.SetActive(!value);
		}).AddTo(this);
		SetupTooltips();
		UpdateAddendumsCount();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CurrentIcon.gameObject.SetActive(value: false);
		m_CurrentIcon = null;
		m_CurrentIconAddition.Or(null)?.SetActive(value: true);
		m_CurrentIconAddition = null;
	}

	private void UpdateAddendumsCount()
	{
		m_AddendumsCountContainer.gameObject.SetActive(base.ViewModel.Addendums.Count > 0);
		m_AddendumsCount.text = "+" + base.ViewModel.Addendums.Count;
	}

	private void SetupTooltips()
	{
		TooltipConfig config = new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, GetComponent<RectTransform>(), 0, 0, 0, new List<Vector2>
		{
			new Vector2(0f, 0.5f)
		});
		string additionalDescription = DetectiveInfoEncryption.EncryptAddendums(base.ViewModel.BlueprintClue, base.ViewModel.Addendums);
		TooltipTemplateDetective template = new TooltipTemplateDetective(base.ViewModel.BlueprintClue, useHeader: false, additionalDescription);
		m_ClueName.SetTooltip(template, config).AddTo(this);
		m_CluTooltipParent.SetTooltip(template, config).AddTo(this);
	}
}
