using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BugReportBaseView : View<BugReportVM>
{
	[Header("Localizations")]
	[SerializeField]
	private TextMeshProUGUI m_MainTitleText;

	[SerializeField]
	private TextMeshProUGUI m_AspectTitleText;

	[SerializeField]
	private TextMeshProUGUI m_ContextTitleText;

	[SerializeField]
	private TextMeshProUGUI m_SendButtonText;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	[SerializeField]
	private TextMeshProUGUI m_SuggestionToggleText;

	[SerializeField]
	private TextMeshProUGUI m_NormalToggleText;

	[SerializeField]
	private TextMeshProUGUI m_CriticalToggleText;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionTitleText;

	[SerializeField]
	private TextMeshProUGUI m_EmailTitleText;

	[SerializeField]
	private TextMeshProUGUI m_BottomDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_PrivacyDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_DiscordTitleText;

	[SerializeField]
	private TextMeshProUGUI m_EmailUpdatesDescriptionText;

	[Header("Other")]
	[SerializeField]
	protected OwlcatMultiButton m_CloseButton;

	[SerializeField]
	protected OwlcatMultiButton m_DrawingButton;

	[SerializeField]
	protected OwlcatMultiButton m_DuplicatesButton;

	[SerializeField]
	private OwlcatInputField m_MessageInputField;

	[SerializeField]
	private GameObject m_EmailGroup;

	[SerializeField]
	private OwlcatInputField m_EmailInputField;

	[SerializeField]
	private OwlcatInputField m_DiscordInputField;

	[SerializeField]
	private GameObject m_DiscordGameObject;

	[SerializeField]
	protected OwlcatDropdown m_ContextDropdown;

	[SerializeField]
	private OwlcatDropdown m_AspectDropdown;

	[SerializeField]
	private GameObject m_AssigneeGO;

	[SerializeField]
	private OwlcatDropdown m_AssigneeDropdown;

	[SerializeField]
	private GameObject m_ManualSaveGroup;

	[SerializeField]
	private OwlcatDropdown m_ManualSaveDropdown;

	[SerializeField]
	private GameObject m_FixVersionGO;

	[SerializeField]
	private OwlcatDropdown m_FixVersionDropdown;

	[SerializeField]
	protected OwlcatMultiButton m_SendButton;

	[SerializeField]
	private OwlcatMultiButton m_BlinkButton;

	[SerializeField]
	private BlinkWidget m_BlinkEmail;

	[SerializeField]
	private BlinkWidget m_BlinkPrivacy;

	[Header("Issue Type")]
	[SerializeField]
	private OwlcatToggleGroup m_IssueTypeToggleGroup;

	[SerializeField]
	private OwlcatToggle m_CriticalToggle;

	[SerializeField]
	private OwlcatToggle m_NormalToggle;

	[SerializeField]
	private OwlcatToggle m_SuggestionToggle;

	[SerializeField]
	private GameObject m_devPriorityGroup;

	[SerializeField]
	private OwlcatDropdown m_devPriorityDropdown;

	[Header("Feedback")]
	[SerializeField]
	private OwlcatToggle m_FeedbackToggle;

	[Header("Labels")]
	[SerializeField]
	private GameObject m_LabelsGroup;

	[SerializeField]
	protected OwlcatMultiButton m_LabelsButton;

	[SerializeField]
	private TextMeshProUGUI m_LabelsButtonText;

	[SerializeField]
	[FormerlySerializedAs("m_LabelTogglePrefab")]
	private GameObject m_LabelsPrefab;

	[SerializeField]
	[FormerlySerializedAs("m_LabelsBackgroundRectTransform")]
	private RectTransform m_LabelsList;

	[SerializeField]
	private RectTransform m_LabelsListContainer;

	[SerializeField]
	private Button m_LabelsBlocker;

	[Header("Privacy and Email")]
	[SerializeField]
	protected OwlcatToggle m_PrivacyToggle;

	[SerializeField]
	private OwlcatToggle m_EmailUpdatesToggle;

	[SerializeField]
	private GameObject m_EmailUpdatesGroup;

	[Header("Drawing")]
	[SerializeField]
	protected BugReportDrawingView m_BugReportDrawingView;

	[Header("Duplicates")]
	[SerializeField]
	protected BugReportDuplicatesBaseView m_BugReportDuplicatesBaseView;

	private string m_PrevEmail;

	private string m_PrevDiscord;

	private bool m_IsLabelsShow;

	private List<GameObject> m_LabelsListItems = new List<GameObject>();

	private CompositeDisposable m_LabelsDisposable;

	public static string InputLayerContextName = "BugReportInput";

	private ReactiveProperty<bool> m_CanSendReport = new ReactiveProperty<bool>();

	private Dictionary<OwlcatToggle, string> m_IssueTypes;

	private IDisposable m_AspectDropdownDisposable;

	private IDisposable m_FixVersionDropdownDisposable;

	private IDisposable m_AssigneeDropdownDisposable;

	private IDisposable m_ManualSaveDropdownDisposable;

	private IDisposable m_CannotSendReportHint;

	public static string LabelsDisposableString => "m_LabelsDisposable";

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
		m_IssueTypes = new Dictionary<OwlcatToggle, string>
		{
			{ m_CriticalToggle, "Crit" },
			{ m_NormalToggle, "Normal" },
			{ m_SuggestionToggle, "Minor" }
		};
		m_MessageInputField.SetMaxTextLength(BuildModeUtility.IsDevelopment ? uint.MaxValue : 1000u);
		m_BugReportDrawingView.gameObject.SetActive(value: false);
		m_BugReportDuplicatesBaseView.gameObject.SetActive(value: false);
		m_LabelsBlocker.onClick.AddListener(OnLabelsShow);
	}

	protected override void OnBind()
	{
		m_LabelsDisposable = new CompositeDisposable().AddTo(this);
		base.OnBind();
		Show(state: true);
		base.ViewModel.BugReportDrawingVM.Subscribe(m_BugReportDrawingView.Bind).AddTo(this);
		base.ViewModel.BugReportDuplicatesVM.Subscribe(m_BugReportDuplicatesBaseView.Bind).AddTo(this);
		m_FeedbackToggle.IsOn.Subscribe(OnFeedbackToggleValueChanged).AddTo(this);
		m_PrivacyToggle.IsOn.Subscribe(delegate
		{
			UpdateCanSendReport();
		}).AddTo(this);
		m_ContextDropdown.Index.Subscribe(delegate
		{
			OnContextDropdownValueChanged();
		}).AddTo(this);
		m_MessageInputField.SetPlaceholderText(UIStrings.Instance.UIBugReport.AdditionalPlaceholderText.Text);
		m_CanSendReport.Subscribe(delegate(bool value)
		{
			m_SendButton.SetInteractable(value);
			m_BlinkButton.Or(null)?.gameObject.SetActive(!value);
		}).AddTo(this);
		if (m_BlinkButton != null && m_BlinkEmail != null && m_BlinkPrivacy != null)
		{
			ObservableSubscribeExtensions.Subscribe(m_BlinkButton.OnLeftClickAsObservable(), delegate
			{
				if (!m_PrivacyToggle.IsOn.CurrentValue)
				{
					m_BlinkPrivacy.Blink();
				}
				if (!IsEmailMatchRegexp() && BuildModeUtility.IsDevelopment)
				{
					m_BlinkEmail.Blink();
				}
			}).AddTo(this);
		}
		m_DuplicatesButton.gameObject.SetActive(value: false);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Show(state: false);
		m_AspectDropdownDisposable?.Dispose();
		m_AspectDropdownDisposable = null;
		DisposeDropdowns();
	}

	private void SetupDropdowns()
	{
		DisposeDropdowns();
		if (!BuildModeUtility.IsDevelopment)
		{
			return;
		}
		m_FixVersionDropdownDisposable = m_FixVersionDropdown.Index.Subscribe(delegate
		{
			OnFixVersionDropdownValueChanged();
		});
		m_AssigneeDropdownDisposable = m_AssigneeDropdown.Index.Subscribe(delegate
		{
			OnAssigneeDropdownValueChanged();
		});
		if (m_ManualSaveDropdown != null)
		{
			m_ManualSaveDropdownDisposable = m_ManualSaveDropdown.Index.Subscribe(delegate
			{
				OnManualSaveDropdownValueChanged();
			});
		}
	}

	private void DisposeDropdowns()
	{
		m_FixVersionDropdownDisposable?.Dispose();
		m_FixVersionDropdownDisposable = null;
		m_AssigneeDropdownDisposable?.Dispose();
		m_AssigneeDropdownDisposable = null;
		m_ManualSaveDropdownDisposable?.Dispose();
		m_ManualSaveDropdownDisposable = null;
	}

	protected void OnShowDrawing()
	{
		base.ViewModel.ShowDrawing();
	}

	protected void OnShowDuplicates()
	{
		base.ViewModel.ShowDuplicates();
	}

	private void StoreUserData()
	{
		PlayerPrefs.SetString("BugReportEmail", m_EmailInputField.Text);
		PlayerPrefs.SetString("BugReportDiscord", m_DiscordInputField.Text);
		int currentFixVersionIndex = ReportingUtils.Instance.GetCurrentFixVersionIndex();
		PlayerPrefs.SetInt("BugReportFixVersion", currentFixVersionIndex);
	}

	private void RestoreUserData(bool restoreDevFields)
	{
		m_EmailInputField.Text = PlayerPrefs.GetString("BugReportEmail", string.Empty);
		m_DiscordInputField.Text = PlayerPrefs.GetString("BugReportDiscord", string.Empty);
		if (restoreDevFields && m_FixVersionDropdown.Index != null)
		{
			int @int = PlayerPrefs.GetInt("BugReportFixVersion", 0);
			m_FixVersionDropdown.SetIndex(@int);
		}
		(bool, bool) tuple = ReportingUtils.Instance.PrivacyStuffGetEmailAgreements(m_EmailInputField.Text);
		m_PrivacyToggle.Set(tuple.Item2);
		m_EmailUpdatesToggle.Set(tuple.Item1 && IsEmailMatchRegexp());
	}

	public async void OnSend()
	{
		if (!m_CanSendReport.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.UIBugReport.SendindIsNotAvailable, addToLog: false, WarningNotificationFormat.Attention);
			});
			return;
		}
		string issueType = GetIssueType();
		await ReportingUtils.Instance.SendReport(m_MessageInputField.Text, m_EmailInputField.Text, SystemInfo.deviceUniqueIdentifier, issueType, m_DiscordInputField.Text, m_EmailUpdatesToggle.IsOn.CurrentValue);
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CommonTexts.WarningBugReportWasSend);
		});
		m_MessageInputField.Text = "";
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
		StoreUserData();
		ReportingUtils.Instance.PrivacyStuffManage(m_EmailInputField.Text, m_EmailUpdatesToggle.IsOn.CurrentValue, m_PrivacyToggle.IsOn.CurrentValue, isSend: true);
		ReportingUtils.Instance.Clear();
		InputLog.SetLogInput(state: false);
	}

	public void OnClose()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
		StoreUserData();
		ReportingUtils.Instance.PrivacyStuffManage(m_EmailInputField.Text, m_EmailUpdatesToggle.IsOn.CurrentValue, m_PrivacyToggle.IsOn.CurrentValue, isSend: false);
		ReportingUtils.Instance.DiscardReport();
		ReportingUtils.Instance.Clear();
	}

	public void Show(bool state)
	{
		SetTranslationText();
		m_DescriptionTitleText.text = UIStrings.Instance.UIBugReport.DesctiptionHeader;
		base.gameObject.SetActive(state);
		Game.Instance.RequestPauseUi(state);
		if (state)
		{
			OnShow();
		}
	}

	private void OnShow()
	{
		m_NormalToggle.Set(value: true);
		m_IsLabelsShow = false;
		HideLabelsButton();
		m_ContextDropdown.Bind(base.ViewModel.ContextDropdownVM);
		m_AspectDropdown.Bind(base.ViewModel.GetAspectDropDownVM());
		bool isDevelopment = BuildModeUtility.IsDevelopment;
		if (isDevelopment)
		{
			try
			{
				m_EmailGroup?.SetActive(value: true);
				m_AssigneeGO.gameObject.SetActive(value: true);
				m_AssigneeDropdown.Bind(base.ViewModel.GetAssigneeDropDownVM(m_ContextDropdown.Index.CurrentValue));
				m_FixVersionGO.SetActive(value: true);
				m_FixVersionDropdown.Bind(base.ViewModel.GetFixVersionDropDownVM());
				if (m_ManualSaveGroup != null && m_ManualSaveDropdown != null)
				{
					m_ManualSaveGroup.SetActive(value: true);
					m_ManualSaveDropdown.Bind(base.ViewModel.GetManualSaveDropDownVM());
				}
				m_devPriorityGroup.SetActive(value: true);
				m_devPriorityDropdown.Bind(base.ViewModel.GetPriorityDropDownVM());
				m_devPriorityDropdown.SetIndex(base.ViewModel.GetDefaultDevPriorityIndex());
				m_CriticalToggle.gameObject.SetActive(value: false);
				m_NormalToggle.gameObject.SetActive(value: false);
				m_SuggestionToggle.gameObject.SetActive(value: false);
				SetupDropdowns();
				m_LabelsGroup.SetActive(value: true);
				ReportingUtils.Instance.ResetLabelsList();
				ReportingUtils.Instance.FillLabelsDictionary();
				LabelsButtonChangeText();
			}
			catch
			{
				m_AssigneeGO.gameObject.SetActive(value: false);
				m_FixVersionGO.SetActive(value: false);
				m_LabelsGroup.SetActive(value: false);
				m_devPriorityGroup.SetActive(value: false);
			}
		}
		else
		{
			m_AssigneeGO.gameObject.SetActive(value: false);
			m_FixVersionGO.SetActive(value: false);
			m_LabelsGroup.SetActive(value: false);
			m_devPriorityGroup.SetActive(value: false);
		}
		m_FeedbackToggle.gameObject.SetActive(isDevelopment);
		ToggleAdditionalContactsVisibility(IsEmailMatchRegexp());
		ExpandDescriptionOverMarket();
		RestoreUserData(isDevelopment);
		UpdateCanSendReport();
	}

	public void OnLabelsShow()
	{
		if (m_IsLabelsShow)
		{
			HideLabelsButton();
		}
		else
		{
			CreateLabelsObjectList();
		}
		m_IsLabelsShow = !m_IsLabelsShow;
	}

	public void OnLabelSelected(string label, bool b)
	{
		ReportingUtils.Instance.LabelChangeValue(label, b);
		LabelsButtonChangeText();
	}

	private void LabelsButtonChangeText()
	{
		int num = 0;
		Dictionary<string, bool> labelsList = ReportingUtils.Instance.GetLabelsList();
		foreach (bool value in labelsList.Values)
		{
			if (value)
			{
				num++;
			}
		}
		m_LabelsButtonText.text = $"{num}/{labelsList.Count} labels selected";
	}

	private void CreateLabelsObjectList()
	{
		Dictionary<string, bool> labelsList = ReportingUtils.Instance.GetLabelsList();
		if (labelsList == null || labelsList.Count == 0)
		{
			return;
		}
		m_LabelsList.gameObject.SetActive(value: true);
		m_LabelsListItems = new List<GameObject>();
		foreach (KeyValuePair<string, bool> item in labelsList)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_LabelsPrefab, m_LabelsListContainer);
			gameObject.SetActive(value: true);
			m_LabelsListItems.Add(gameObject);
			gameObject.GetComponent<BugReportLabelToggle>().Initiate(OnLabelSelected, item.Key, item.Value);
		}
	}

	public void HideLabelsButton()
	{
		m_LabelsList.gameObject.SetActive(value: false);
		foreach (GameObject labelsListItem in m_LabelsListItems)
		{
			UnityEngine.Object.Destroy(labelsListItem);
		}
		m_LabelsDisposable.Clear();
	}

	public void OnFeedbackToggleValueChanged(bool isFeedback)
	{
		ReportingUtils.Instance.SelectIsFeedback(isFeedback);
	}

	private void UpdateCanSendReport()
	{
		bool currentValue = m_PrivacyToggle.IsOn.CurrentValue;
		bool flag = IsEmailMatchRegexp() || !BuildModeUtility.IsDevelopment;
		m_CanSendReport.Value = currentValue && flag;
		m_CannotSendReportHint?.Dispose();
		m_CannotSendReportHint = null;
		bool num = !currentValue;
		bool flag2 = !flag;
		string text = (num ? ((!flag2) ? ((string)UIStrings.Instance.UIBugReport.CannotSendBecausePrivacy) : string.Concat(UIStrings.Instance.UIBugReport.CannotSendBecausePrivacy, "\n", UIStrings.Instance.UIBugReport.CannotSendBecauseEmail)) : ((!flag2) ? null : ((string)UIStrings.Instance.UIBugReport.CannotSendBecauseEmail)));
		string text2 = text;
		if (text2 != null)
		{
			m_CannotSendReportHint = m_BlinkButton.SetHint(text2).AddTo(this);
		}
	}

	public void OnContextDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectContext(m_ContextDropdown.Index.CurrentValue);
		UpdateAspectDropdown();
		if (BuildModeUtility.IsDevelopment)
		{
			try
			{
				m_AssigneeGO.gameObject.SetActive(value: true);
				m_AssigneeDropdown.Bind(base.ViewModel.GetAssigneeDropDownVM(m_ContextDropdown.Index.CurrentValue));
				SetupDropdowns();
				return;
			}
			catch
			{
				m_AssigneeGO.gameObject.SetActive(value: false);
				m_FixVersionGO.SetActive(value: false);
				return;
			}
		}
		m_AssigneeGO.gameObject.SetActive(value: false);
		if (m_FixVersionGO.activeSelf)
		{
			m_FixVersionGO.SetActive(value: false);
		}
		if (m_LabelsGroup.activeSelf)
		{
			m_LabelsGroup.SetActive(value: false);
		}
	}

	public void OnAspectDropdownValueChanged()
	{
		int currentValue = m_AspectDropdown.Index.CurrentValue;
		int currentValue2 = m_ContextDropdown.Index.CurrentValue;
		ReportingUtils.Instance.SelectAspect(currentValue);
		if (BuildModeUtility.IsDevelopment)
		{
			try
			{
				int assigneeIndex = base.ViewModel.GetAssigneeIndex(currentValue2, currentValue);
				m_AssigneeDropdown.SetIndex(assigneeIndex);
				OnAssigneeDropdownValueChanged();
				m_AssigneeGO.gameObject.SetActive(value: true);
				return;
			}
			catch
			{
				m_AssigneeGO.gameObject.SetActive(value: false);
				m_FixVersionGO.SetActive(value: false);
				return;
			}
		}
		m_AssigneeGO.gameObject.SetActive(value: false);
		if (m_FixVersionGO.activeSelf)
		{
			m_FixVersionGO.SetActive(value: false);
		}
		if (m_LabelsGroup.activeSelf)
		{
			m_LabelsGroup.SetActive(value: false);
		}
	}

	public void OnAssigneeDropdownValueChanged()
	{
		string text = m_AssigneeDropdown.GetCurrentTextValue();
		if (text.Contains("ui_designer"))
		{
			text = "ui_designer";
		}
		ReportingUtils.Instance.SelectAssignee(text);
	}

	public void OnFixVersionDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectFixVersion(m_FixVersionDropdown.Index.CurrentValue);
	}

	public void OnManualSaveDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectManualSave(m_ManualSaveDropdown.Index.CurrentValue - 1);
	}

	public void OnLinkClick(PointerEventData eventData, TMP_LinkInfo linkInfo)
	{
		if (linkInfo.GetLinkID() == "pp")
		{
			Application.OpenURL("https://owlcatgames.com/privacy");
		}
	}

	private string GetIssueType()
	{
		if (BuildModeUtility.IsDevelopment)
		{
			return m_devPriorityDropdown.GetCurrentTextValue();
		}
		OwlcatToggle key = m_IssueTypeToggleGroup.ActiveToggles().First();
		return m_IssueTypes[key];
	}

	private void SetTranslationText()
	{
		m_DescriptionTitleText.text = UIStrings.Instance.UIBugReport.DesctiptionHeader;
		m_MainTitleText.text = UIStrings.Instance.UIBugReport.Header;
		m_AspectTitleText.text = UIStrings.Instance.UIBugReport.AspectHeader;
		m_ContextTitleText.text = UIStrings.Instance.UIBugReport.ContextHeader;
		m_SendButtonText.text = UIStrings.Instance.UIBugReport.SendButton;
		m_HintText.text = UIStrings.Instance.UIBugReport.HintText;
		m_SuggestionToggleText.text = UIStrings.Instance.UIBugReport.SuggestionTogle;
		m_NormalToggleText.text = UIStrings.Instance.UIBugReport.NormalTogle;
		m_CriticalToggleText.text = UIStrings.Instance.UIBugReport.CriticalTogle;
		m_EmailTitleText.text = UIStrings.Instance.UIBugReport.EmailHeader;
		m_DiscordTitleText.text = UIStrings.Instance.UIBugReport.DiscordHeader;
		m_BottomDescriptionText.text = UIStrings.Instance.UIBugReport.ButtomDescription;
		m_PrivacyDescriptionText.text = UIStrings.Instance.UIBugReport.PrivacyCheckBoxDescription;
		m_EmailUpdatesDescriptionText.text = UIStrings.Instance.UIBugReport.EmailUpdatesCheckBoxDescription;
	}

	private void UpdateAspectDropdown()
	{
		m_AspectDropdown.SetIndex(ReportingUtils.Instance.GetInitialAspectIndex());
		m_AspectDropdown.Bind(base.ViewModel.GetAspectDropDownVM());
		m_AspectDropdownDisposable?.Dispose();
		m_AspectDropdownDisposable = m_AspectDropdown.Index.Subscribe(delegate
		{
			OnAspectDropdownValueChanged();
		});
	}

	private bool IsEmailMatchRegexp()
	{
		if (string.IsNullOrEmpty(m_EmailInputField.Text))
		{
			return false;
		}
		return new Regex("^\\w+[\\w-\\.]*\\@\\w+((-\\w+)|(\\w*))\\.[a-z]{2,9}$").Matches(m_EmailInputField.Text).Count == 1;
	}

	private void ToggleAdditionalContactsVisibility(bool toShow)
	{
		m_PrevEmail = m_EmailInputField.Text;
		if (toShow && !m_DiscordGameObject.activeSelf)
		{
			m_DiscordGameObject.SetActive(value: true);
		}
		else if (!toShow && m_DiscordGameObject.activeSelf)
		{
			m_DiscordGameObject.SetActive(value: false);
		}
	}

	private void HandleDiscordText()
	{
		if (m_PrevDiscord != m_DiscordInputField.Text)
		{
			try
			{
				m_DiscordInputField.Text = m_DiscordInputField.Text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
			}
			catch
			{
			}
		}
		m_PrevDiscord = m_DiscordInputField.Text;
	}

	private void ExpandDescriptionOverMarket()
	{
		bool item = ReportingUtils.Instance.PrivacyStuffGetEmailAgreements(m_EmailInputField.Text).promo;
		m_EmailUpdatesToggle.Set(item);
		m_EmailUpdatesGroup.SetActive(!item);
	}

	private void LateUpdate()
	{
		if (m_PrevEmail != m_EmailInputField.Text)
		{
			ToggleAdditionalContactsVisibility(IsEmailMatchRegexp());
			UpdateCanSendReport();
			(bool, bool) tuple = ReportingUtils.Instance.PrivacyStuffGetEmailAgreements(m_EmailInputField.Text);
			m_PrivacyToggle.Set(tuple.Item2);
			ExpandDescriptionOverMarket();
		}
		HandleDiscordText();
	}
}
