using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.Components.Camera;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Networking;
using Kingmaker.UI;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Visual.LocalMap;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapBaseView : View<LocalMapVM>
{
	private class PingData
	{
		public IDisposable PingDelay { get; set; }
	}

	private static readonly int FowTex = Shader.PropertyToID("_FowTex");

	[Header("Common Block")]
	[SerializeField]
	private ScrambledTMP m_Title;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private TMP_Text m_NoSignalTitle;

	[Header("Map Block")]
	[SerializeField]
	protected RawImage m_Image;

	[SerializeField]
	private RawImage m_TestRawImage;

	[SerializeField]
	protected RectTransform m_FrameBlock;

	[SerializeField]
	private RectTransform m_Frame;

	[SerializeField]
	private RectTransform m_LittleSkullCamera;

	public Vector2 MaxSize = new Vector2(1640f, 677f);

	private Vector2 m_ChangedMapSize;

	[Header("Markers Block")]
	[SerializeField]
	private List<LocalMapMarkerSet> m_MarkerSets = new List<LocalMapMarkerSet>();

	[Header("Right Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_MapHistoryButton;

	[SerializeField]
	private LocalMapLegendBlockView m_LegendBlockView;

	[SerializeField]
	private RectTransform m_MapHistoryLittleSquare;

	[SerializeField]
	private RectTransform m_MapBlock;

	[SerializeField]
	private RectTransform[] m_MarkersNeedToShowAlways;

	[FormerlySerializedAs("m_TargetPingEntitys")]
	[SerializeField]
	private List<FadeAnimator> m_TargetPingEntities = new List<FadeAnimator>();

	[Header("Debug")]
	[SerializeField]
	private bool m_UsePostprocess;

	[Header("Values")]
	[SerializeField]
	protected float m_ZoomStep;

	[Header("Values")]
	[SerializeField]
	protected float m_ZoomMaxSize;

	[Header("Values")]
	[SerializeField]
	protected float m_ZoomMinSize;

	[SerializeField]
	private float m_MoveMapSpeed;

	[SerializeField]
	private float m_MoveMapFrame;

	[SerializeField]
	private Vector2 m_CorrectTargetPositionPoint;

	[SerializeField]
	private float m_CorrectBiggerX = 185f;

	[SerializeField]
	private float m_CorrectBiggerMinusX = 185f;

	[SerializeField]
	private float m_CorrectBiggerY = 125f;

	[SerializeField]
	private float m_CorrectBiggerMinusY = 90f;

	[Header("Screen")]
	[SerializeField]
	[FormerlySerializedAs("UIPostProcessMember")]
	private UIPostProcessMember m_UIPostProcessMember;

	[SerializeField]
	private FadeAnimator m_ScreenContentFadeAnimator;

	[SerializeField]
	private GameObject m_ShatteredGlass;

	private bool m_SaveMarkerCoords;

	private Vector2 m_Size;

	protected readonly ReactiveProperty<bool> MaxZoom = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> MinZoom = new ReactiveProperty<bool>();

	protected float CurrentZoom;

	private readonly List<LocalMapMarkerPCView> m_MarkerViews = new List<LocalMapMarkerPCView>();

	private readonly Dictionary<NetPlayer, PingData> m_PlayerPingData = new Dictionary<NetPlayer, PingData>();

	private Vector2 MaxPos
	{
		get
		{
			Vector2 vector = m_Image.rectTransform.localScale;
			Vector2 vector2 = vector - Vector2.one * m_ZoomMinSize;
			return m_Size / 5f * vector * vector2 * m_MoveMapFrame;
		}
	}

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		ShowWindow();
		if (m_UsePostprocess)
		{
			m_UIPostProcessMember.Bind();
		}
		m_NoSignalTitle.text = UIStrings.Instance.LocalMapTexts.NoSignalTitle.Text;
		foreach (FadeAnimator item in m_TargetPingEntities.Where((FadeAnimator entity) => entity != null))
		{
			if (item.CanvasGroup != null)
			{
				item.CanvasGroup.alpha = 0f;
			}
			item.DisappearAnimation();
		}
		for (int i = 0; i < m_TargetPingEntities.Count; i++)
		{
			Image component = m_TargetPingEntities[i].GetComponent<Image>();
			if (!(component == null))
			{
				if (ConfigRoot.Instance.UIConfig.CoopPlayersPingsColors.Count < i)
				{
					break;
				}
				component.color = ConfigRoot.Instance.UIConfig.CoopPlayersPingsColors[i];
			}
		}
		SetMaxSize();
		base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_Title.SetText(string.Empty, value);
		}).AddTo(this);
		base.ViewModel.DrawResult.Subscribe(SetDrawResult).AddTo(this);
		base.ViewModel.CompassAngle.Subscribe(SetFrameAngle).AddTo(this);
		base.ViewModel.CanOpenMap.Subscribe(delegate(bool value)
		{
			m_StateSelectable.SetActiveLayer(value ? "NoSignal" : "Default");
		}).AddTo(this);
		m_MapHistoryButton.OnHoverAsObservable().Subscribe(ShowLocalMapHistory).AddTo(this);
		base.ViewModel.CoopPingPosition.Subscribe(delegate((NetPlayer, Vector3) value)
		{
			PingPosition(value.Item1, value.Item2);
		}).AddTo(this);
		OpenLocalMapFirstZoomSettings();
		SetMarkersVM();
		m_LegendBlockView.Bind(base.ViewModel.LocalMapLegendBlockVM);
		SetMapRotation((float)base.ViewModel.LocalMapRotation);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			HandleUpdate();
		}).AddTo(this);
		if (RootVM.Instance.WindowsPanelVM.Value?.FoWTextureTemp != null)
		{
			m_TestRawImage.material.SetTexture(FowTex, RootVM.Instance.WindowsPanelVM.Value?.FoWTextureTemp);
		}
	}

	protected override void OnUnbind()
	{
		HideWindow();
		m_MarkerViews.ForEach(WidgetFactory.DisposeWidget);
		m_MarkerViews.Clear();
		SetMapRotation(0f);
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_ShatteredGlass.SetActive(value: true);
		UIPostProcessingAnimator.Instance.PlayState(UIPostEffectState.Default);
		m_ScreenContentFadeAnimator.AppearAnimation();
	}

	private void HideWindow()
	{
		m_ShatteredGlass.SetActive(value: false);
		if (m_UsePostprocess)
		{
			if (base.ViewModel.SwitchedFromServiceWindow.Value)
			{
				base.gameObject.SetActive(value: false);
				m_UIPostProcessMember?.Dispose();
				return;
			}
			m_ScreenContentFadeAnimator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
				{
					m_UIPostProcessMember?.Dispose();
				});
			});
			if ((bool)UIPostProcessingAnimator.Instance)
			{
				UIPostProcessingAnimator.Instance.PlayState(UIPostEffectState.Off);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void HandleUpdate()
	{
		ShowMarkersAlways();
	}

	private void ShowMarkersAlways()
	{
		foreach (RectTransform item in m_Image.transform)
		{
			if (!m_MarkersNeedToShowAlways.Contains(item))
			{
				continue;
			}
			foreach (RectTransform item2 in item)
			{
				LocalMapMarkerPCView component = item2.GetComponent<LocalMapMarkerPCView>();
				if (!(component == null))
				{
					component.LocalMapMarkersAlwaysInside();
					component.ShowHideArrow(state: false, component.RealPosition, item2.position);
					bool flag = component.RealPosition.x > m_MapBlock.sizeDelta.x / 2f - m_CorrectBiggerX;
					bool flag2 = component.RealPosition.x < (0f - m_MapBlock.sizeDelta.x) / 2f + m_CorrectBiggerMinusX;
					bool flag3 = component.RealPosition.y > m_MapBlock.sizeDelta.y / 2f - m_CorrectBiggerY;
					bool flag4 = component.RealPosition.y < (0f - m_MapBlock.sizeDelta.y) / 2f + m_CorrectBiggerMinusY;
					if (flag || flag2 || flag3 || flag4)
					{
						float x = (flag ? (m_MapBlock.sizeDelta.x / 2f - m_CorrectBiggerX) : (flag2 ? ((0f - m_MapBlock.sizeDelta.x) / 2f + m_CorrectBiggerMinusX) : component.RealPosition.x));
						float y = (flag3 ? (m_MapBlock.sizeDelta.y / 2f - m_CorrectBiggerY) : (flag4 ? ((0f - m_MapBlock.sizeDelta.y) / 2f + m_CorrectBiggerMinusY) : component.RealPosition.y));
						item2.position = new Vector3(x, y);
						component.ShowHideArrow(state: true, component.RealPosition, item2.position);
					}
				}
			}
		}
	}

	private void SetMarkersVM()
	{
		foreach (LocalMapMarkerVM item in base.ViewModel.MarkersVm)
		{
			AddLocalMapMarker(item);
		}
		base.ViewModel.MarkersVm.ObserveAdd().Subscribe(delegate(CollectionAddEvent<LocalMapMarkerVM> value)
		{
			AddLocalMapMarker(value.Value);
		}).AddTo(this);
		m_FrameBlock.localScale = Vector3.one;
		foreach (RectTransform item2 in m_Image.transform)
		{
			if (item2 == m_FrameBlock)
			{
				continue;
			}
			foreach (RectTransform item3 in item2)
			{
				item3.localScale = Vector3.one;
			}
		}
	}

	private void OpenLocalMapFirstZoomSettings()
	{
		SetMapSize();
		m_Image.rectTransform.sizeDelta = m_Size;
		m_Image.rectTransform.localScale = Vector2.one * m_ZoomMinSize;
		UpdateMapPosition(Vector2.zero);
		InteractableRightButtons();
	}

	private void SetMapRotation(float z)
	{
		m_Image.rectTransform.eulerAngles = new Vector3(0f, 0f, z);
		foreach (RectTransform item in m_Image.transform)
		{
			if (item == m_FrameBlock)
			{
				continue;
			}
			foreach (RectTransform item2 in item)
			{
				item2.localRotation = Quaternion.Inverse(m_Image.rectTransform.rotation);
			}
		}
		SetFrameAngle(base.ViewModel.CompassAngle.CurrentValue);
	}

	protected void ShowLocalMapHistory(bool state)
	{
		UISounds.Instance.Sounds.LocalMap.ShowHideLocalMapLegend.Play();
		m_LegendBlockView.ShowHide(state);
		m_MapHistoryLittleSquare.DOAnchorPosX(state ? 31f : (-3f), 0.1f).SetEase(Ease.Linear).SetUpdate(isIndependentUpdate: true);
	}

	private void SetFrameAngle(float z)
	{
		m_Frame.eulerAngles = new Vector3(0f, 0f, 0f - z);
	}

	private void AddLocalMapMarker(LocalMapMarkerVM localMapMarkerVM)
	{
		LocalMapMarkerSet localMapMarkerSet = m_MarkerSets.FirstOrDefault((LocalMapMarkerSet s) => s.Type == localMapMarkerVM.MarkerType);
		if (localMapMarkerSet != null)
		{
			LocalMapMarkerPCView widget = WidgetFactory.GetWidget(localMapMarkerSet.View);
			widget.transform.SetParent(localMapMarkerSet.Container, worldPositionStays: false);
			widget.Initialize(m_Image.rectTransform.sizeDelta);
			widget.Bind(localMapMarkerVM);
			m_MarkerViews.Add(widget);
		}
	}

	protected void FindRogueTrader(bool smooth)
	{
		LocalMapMarkerSet localMapMarkerSet = m_MarkerSets.FirstOrDefault((LocalMapMarkerSet s) => s.Type == LocalMapMarkType.PlayerCharacter);
		if (localMapMarkerSet == null || localMapMarkerSet.Container.transform.childCount <= 0)
		{
			return;
		}
		RectTransform rectTransform = localMapMarkerSet.Container.transform.Cast<RectTransform>().FirstOrDefault((RectTransform character) => character.GetComponent<LocalMapCharacterMarkerPCView>()?.CharacterName == Game.Instance.Player.MainCharacter.Entity?.CharacterName);
		if ((bool)rectTransform)
		{
			Vector2 maxPos = MaxPos;
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			Vector2 vector = UIUtilityRect.Clamp(base.ViewModel.LocalMapRotation switch
			{
				BlueprintAreaPart.LocalMapRotationDegree.Degree0 => new Vector2(0f - anchoredPosition.x, 0f - anchoredPosition.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree90 => new Vector2(anchoredPosition.y, 0f - anchoredPosition.x), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree180 => new Vector2(anchoredPosition.x, anchoredPosition.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree270 => new Vector2(0f - anchoredPosition.y, anchoredPosition.x), 
				_ => default(Vector2), 
			} * m_Image.rectTransform.localScale, -maxPos, maxPos);
			if (smooth)
			{
				m_Image.rectTransform.DOAnchorPos(vector, 1f).SetUpdate(isIndependentUpdate: true);
			}
			else
			{
				m_Image.rectTransform.anchoredPosition = vector;
			}
			base.ViewModel.ScrollCameraToRogueTrader();
		}
	}

	private void SetDrawResult(WarhammerLocalMapRenderer.DrawResults dr)
	{
		m_Size = new Vector2(dr.ColorRT.width, dr.ColorRT.height);
		SetMapSize();
		Vector2 vector = dr.ScreenRect.size * m_Size;
		m_FrameBlock.sizeDelta = new Vector2(Mathf.Max(vector.x, vector.y), Mathf.Min(vector.x, vector.y));
		m_FrameBlock.anchoredPosition = dr.ScreenRect.min * m_Size;
	}

	private void SetMaxSize()
	{
		BlueprintAreaPart.LocalMapRotationDegree localMapRotation = base.ViewModel.LocalMapRotation;
		m_ChangedMapSize = ((localMapRotation == BlueprintAreaPart.LocalMapRotationDegree.Degree0 || localMapRotation == BlueprintAreaPart.LocalMapRotationDegree.Degree180) ? new Vector2(MaxSize.x, MaxSize.y) : new Vector2(MaxSize.y, MaxSize.x));
	}

	private void SetMapSize()
	{
		Vector2 vector = m_Size / m_ChangedMapSize;
		float num = Mathf.Max(vector.x, vector.y);
		bool flag = num >= 1f;
		float num2 = (flag ? Mathf.Max(num, 1f) : (1f / num));
		m_Size = (flag ? (m_Size / num2) : (m_Size * num2));
	}

	protected void SetMapScale(float zoomDelta)
	{
		float num = m_ZoomStep * zoomDelta;
		Vector3 vector = new Vector3(num, num, 0f);
		Vector3 localScale = m_Image.rectTransform.localScale;
		Vector3 vector2 = UIUtilityRect.Clamp(localScale + vector, new Vector3(m_ZoomMinSize, m_ZoomMinSize, float.MinValue), new Vector3(m_ZoomMaxSize, m_ZoomMaxSize, float.MaxValue));
		if (vector2 == localScale)
		{
			return;
		}
		m_Image.rectTransform.localScale = vector2;
		CurrentZoom = vector2.x;
		foreach (RectTransform item in m_Image.transform)
		{
			if (item == m_FrameBlock)
			{
				continue;
			}
			foreach (RectTransform item2 in item)
			{
				item2.localScale -= vector / 2f;
			}
		}
		UpdateMapPosition(Vector2.zero);
		InteractableRightButtons();
	}

	protected virtual void InteractableRightButtons()
	{
	}

	protected void UpdateMapPosition(Vector2 scrollDelta)
	{
		if (MinZoom.Value)
		{
			Vector2 anchoredPosition = m_Image.rectTransform.anchoredPosition;
			Vector2 maxPos = MaxPos;
			Vector2 vector = UIUtilityRect.Clamp(anchoredPosition + scrollDelta, -maxPos, maxPos);
			if (vector != anchoredPosition)
			{
				m_Image.rectTransform.anchoredPosition = vector;
			}
		}
	}

	protected Vector2 GetViewportPos(Vector2 position)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Image.rectTransform, position, UICamera.Instance, out var localPoint);
		localPoint += Vector2.Scale(m_Size, m_Image.rectTransform.pivot);
		return localPoint / m_Size;
	}

	protected Vector2 GetViewportPos(PointerEventData eventData)
	{
		return GetViewportPos(eventData.position);
	}

	private void PingPosition(NetPlayer player, Vector3 position)
	{
		int playerIndex = player.Index - 1;
		if (m_PlayerPingData.TryGetValue(player, out var value))
		{
			value.PingDelay?.Dispose();
		}
		else
		{
			m_PlayerPingData[player] = new PingData();
		}
		Vector3 vector = WarhammerLocalMapRenderer.Instance.WorldToViewportPoint(position);
		PingData pingData = m_PlayerPingData[player];
		RectTransform rectTransform = m_TargetPingEntities[playerIndex].transform as RectTransform;
		if (rectTransform != null)
		{
			rectTransform.anchoredPosition = new Vector2(m_Image.rectTransform.sizeDelta.x * vector.x, m_Image.rectTransform.sizeDelta.y * vector.y) - m_CorrectTargetPositionPoint;
		}
		m_TargetPingEntities[playerIndex].AppearAnimation();
		pingData.PingDelay = ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(7.5)), delegate
		{
			m_TargetPingEntities[playerIndex].DisappearAnimation();
		}).AddTo(this);
	}
}
