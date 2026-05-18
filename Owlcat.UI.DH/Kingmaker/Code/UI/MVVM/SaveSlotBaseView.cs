using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using Owlcat.UI.Commands;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotBaseView : SelectionGroupEntityView<SaveSlotVM>, IConfirmClickHandler, IConsoleEntity, ISelectHandler, IEventSystemHandler, ICommandProvider
{
	[SerializeField]
	protected bool m_IsDetailedView;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_LocationLabel;

	[SerializeField]
	private TextMeshProUGUI m_TimeInGameLabel;

	[SerializeField]
	private TextMeshProUGUI m_TimeOfSave;

	[SerializeField]
	private GameObject m_DlcRequiredBlock;

	[SerializeField]
	private TextMeshProUGUI m_DlcRequiredLabel;

	[SerializeField]
	private TextMeshProUGUI m_DlcRequiredDescription;

	[Header("Decoration")]
	[SerializeField]
	protected RawImage m_ScreenshotImage;

	[SerializeField]
	private GameObject m_AutoSaveMark;

	[SerializeField]
	private GameObject m_QuickSaveMark;

	[SerializeField]
	private Image m_IronManIcon;

	[Header("Portraits")]
	[SerializeField]
	private SaveLoadPortraitBaseView m_PortraitPrefab;

	[SerializeField]
	private WidgetList m_WidgetListMvvm;

	public IReadOnlyCollection<Command> Commands { get; } = new List<Command>();


	protected UISaveLoadTexts Texts => ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.SaveLoadTexts;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		this.AddCommand("keyboard:backspace", base.ViewModel.Delete);
		this.AddCommand("keyboard:space", base.ViewModel.ShowScreenshot);
		this.AddCommand("keyboard:space#release", base.ViewModel.HideScreenshot);
		base.gameObject.SetActive(value: true);
		if (!(this is NewSaveSlotPCView) && !(this is NewSaveSlotConsoleView))
		{
			base.ViewModel.SetAvailable(state: true);
		}
		if (m_DlcRequiredLabel != null)
		{
			m_DlcRequiredLabel.text = Texts.DlcRequired;
			AddDisposable(base.ViewModel.ShowDlcRequiredLabel.Subscribe(UpdateDLCState));
		}
		UpdateDLCView();
		if (m_PortraitPrefab != null && m_WidgetListMvvm != null)
		{
			AddDisposable(base.ViewModel.PartyPortraits.Subscribe(SetPortraits));
		}
		AddDisposable(base.ViewModel.SaveName.Subscribe(SetSaveName));
		AddDisposable(base.ViewModel.LocationName.Subscribe(SetLocationName));
		AddDisposable(base.ViewModel.TimeInGame.Subscribe(SetTimeInGame));
		base.ViewModel.SystemSaveTime.Subscribe(delegate
		{
			TimeOfSave();
		});
		AddDisposable(base.ViewModel.ScreenShot.Subscribe(SetScreenshot));
		AddDisposable(base.ViewModel.ShowAutoSaveMark.Subscribe(ShowAutoSaveMark));
		AddDisposable(base.ViewModel.ShowQuickSaveMark.Subscribe(ShowQuickSaveMark));
		base.ViewModel.UpdateScreenshot();
		bool flag = base.ViewModel.Reference.Type == SaveInfo.SaveType.IronMan;
		m_IronManIcon.Or(null)?.gameObject.SetActive(flag);
		if (flag && m_IronManIcon != null)
		{
			m_IronManIcon.SetHint(UIStrings.Instance.SaveLoadTexts.SaveHasIronManMode);
		}
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(OnAvailableChanged));
		OnAvailableChanged(base.ViewModel.IsAvailable.CurrentValue);
	}

	protected override void DestroyViewImplementation()
	{
		if (!m_IsDetailedView)
		{
			base.ViewModel.SetAvailable(state: false);
		}
		base.gameObject.SetActive(value: false);
		base.DestroyViewImplementation();
	}

	private void UpdateDLCView()
	{
		if (m_DlcRequiredDescription == null || base.ViewModel.DlcRequiredMap == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (List<string> item in base.ViewModel.DlcRequiredMap)
		{
			bool flag = false;
			foreach (string item2 in item)
			{
				stringBuilder.Append(flag ? (" " + UIStrings.Instance.Tooltips.or.Text + " ") : "- ");
				stringBuilder.Append(item2 ?? "");
				flag = true;
			}
			stringBuilder.Append("\n");
		}
		m_DlcRequiredDescription.text = stringBuilder.ToString();
	}

	private void SetSaveName(string s)
	{
		m_NameLabel.text = s;
	}

	private void TimeOfSave()
	{
		string text = base.ViewModel.SystemSaveTime.CurrentValue.ToString("d").Replace("/", "-");
		string text2 = base.ViewModel.SystemSaveTime.CurrentValue.ToString("HH:mm");
		m_TimeOfSave.text = text + "\n" + text2;
	}

	private void SetLocationName(string s)
	{
		m_LocationLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(s));
		m_LocationLabel.text = s;
	}

	private void SetTimeInGame(string s)
	{
		m_TimeInGameLabel.text = s;
	}

	private void SetScreenshot(Texture2D screenshot)
	{
		m_ScreenshotImage.gameObject.SetActive(screenshot != null);
		if (!(screenshot == null))
		{
			m_ScreenshotImage.texture = screenshot;
			m_ScreenshotImage.GetComponent<AspectRatioFitter>().aspectRatio = screenshot.GetAspect();
		}
	}

	private void SetPortraits(List<SaveLoadPortraitVM> sprites)
	{
		m_WidgetListMvvm.DrawEntries(base.ViewModel.PartyPortraits.CurrentValue?.ToArray(), m_PortraitPrefab);
	}

	private void ShowAutoSaveMark(bool b)
	{
		m_AutoSaveMark.SetActive(b);
	}

	private void ShowQuickSaveMark(bool b)
	{
		m_QuickSaveMark.SetActive(b);
	}

	protected virtual void UpdateDLCState(bool b)
	{
		m_DlcRequiredLabel.gameObject.SetActive(b);
		if (m_DlcRequiredBlock != null)
		{
			m_DlcRequiredBlock.SetActive(b);
		}
		UpdateDLCView();
	}

	public new bool CanConfirmClick()
	{
		return true;
	}

	public new void OnConfirmClick()
	{
		base.ViewModel.SaveOrLoad();
	}

	private void OnAvailableChanged(bool isAvailable)
	{
		base.gameObject.SetActive(isAvailable);
	}

	public void OnSelect(BaseEventData eventData)
	{
		base.ViewModel.SetSelectedFromView(state: true);
	}
}
