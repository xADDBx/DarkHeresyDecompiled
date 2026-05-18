using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BugReportDrawingView : View<BugReportDrawingVM>
{
	[SerializeField]
	private AspectRatioFitter m_ScreenshotFitter;

	[SerializeField]
	private RawImage m_ScreenshotRawImage;

	[SerializeField]
	private RawImage m_ScreenshotDrawingRawImage;

	[Header("Localizations")]
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[SerializeField]
	private TextMeshProUGUI m_ClearButtonText;

	[SerializeField]
	private TextMeshProUGUI m_SaveButtonText;

	[Header("PC Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_ClearButton;

	[SerializeField]
	private OwlcatMultiButton m_SaveButton;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	private readonly BugReportDrawing m_Drawing = new BugReportDrawing();

	private string m_ReportNumber;

	private string m_LastReportNumber;

	private Vector2 m_LeftStickVector;

	public Graphic DrawingImage => m_ScreenshotDrawingRawImage;

	public Texture2D ScreenTexture { get; private set; }

	public Texture2D DrawingTexture { get; private set; }

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_TitleText.text = UIStrings.Instance.UIBugReport.EditScreenShotTitleText;
		m_ClearButtonText.text = UIStrings.Instance.UIBugReport.ClearButtonText;
		m_SaveButtonText.text = UIStrings.Instance.UIBugReport.SaveButtonText;
		CreateInput();
		LoadScreenshot();
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	private void CreateInput()
	{
		ObservableSubscribeExtensions.Subscribe(m_ClearButton.OnLeftClickAsObservable(), delegate
		{
			ClearCanvas();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SaveButton.OnLeftClickAsObservable(), delegate
		{
			SaveToDirectory();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ClearButton.OnConfirmClickAsObservable(), delegate
		{
			ClearCanvas();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SaveButton.OnConfirmClickAsObservable(), delegate
		{
			SaveToDirectory();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
	}

	protected virtual void UpdateCursorMovement()
	{
	}

	private void Update()
	{
		m_Drawing.Update();
		if (m_LeftStickVector != Vector2.zero)
		{
			UpdateCursorMovement();
		}
		m_LeftStickVector = Vector2.zero;
	}

	private void LoadScreenshot()
	{
		Texture2D texture2D = null;
		string path = ReportingUtils.Instance.CurrentReportFolder + "/screen.png";
		m_ReportNumber = ReportingUtils.Instance.CurrentReportFolder.Split('/', '\\').Last();
		if (m_ReportNumber == m_LastReportNumber)
		{
			return;
		}
		m_Drawing.BugreportDrawingView = this;
		if (File.Exists(path))
		{
			byte[] data = File.ReadAllBytes(path);
			texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
			texture2D.LoadImage(data);
			if (QualitySettings.globalTextureMipmapLimit > 0)
			{
				texture2D.requestedMipmapLevel = 0;
				texture2D.LoadImage(data);
			}
		}
		if (!(texture2D == null))
		{
			Texture2D texture2D2 = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, mipChain: false);
			texture2D2.Apply();
			SetActive(texture2D, texture2D2);
		}
	}

	private void SetActive(Texture2D screenshot, Texture2D drawing)
	{
		ScreenTexture = screenshot;
		DrawingTexture = drawing;
		m_Drawing.BugreportDrawingView = this;
		m_Drawing.Awake();
		m_ScreenshotRawImage.texture = screenshot;
		m_ScreenshotDrawingRawImage.texture = drawing;
		m_ScreenshotFitter.aspectRatio = (float)screenshot.width / (float)screenshot.height;
		m_LastReportNumber = m_ReportNumber;
	}

	public void ClearCanvas()
	{
		m_Drawing.ResetCanvas();
	}

	public void SaveToDirectory()
	{
		BugreportScreenCapture.SaveImage(ScreenTexture, DrawingTexture, null);
		base.ViewModel.Close();
	}
}
