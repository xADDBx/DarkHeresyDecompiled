using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[RequireComponent(typeof(RectTransform))]
public class PointMarkersPCView : View<PointMarkersVM>
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_MarkersContainer;

	[SerializeField]
	private RectTransform[] m_InfluencingContainers;

	[Header("Views")]
	[SerializeField]
	private PointMarkerPCView m_MarkerView;

	private readonly List<PointMarkerPCView> m_Markers = new List<PointMarkerPCView>();

	private RectTransform m_RectTransform;

	private float m_XMin;

	private float m_XMax;

	private float m_YMin;

	private float m_YMax;

	private readonly List<LineSegment2> m_ParsedBorders = new List<LineSegment2>();

	public List<LineSegment2> Borders { get; } = new List<LineSegment2>();


	public RectTransform RectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

	private Camera UICamera => Kingmaker.UI.UICamera.Instance;

	public float ScreenScale => Mathf.Min((float)Screen.width / 1920f, (float)Screen.height / 1080f);

	protected override void OnBind()
	{
		base.ViewModel.PointMarkers.ObserveAdd().DebounceFrame(1).Subscribe(delegate
		{
			DrawMarkers();
		})
			.AddTo(this);
		base.ViewModel.PointMarkers.ObserveRemove().DebounceFrame(1).Subscribe(delegate
		{
			DrawMarkers();
		})
			.AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.PointMarkers.ObserveReset().DebounceFrame(1), delegate
		{
			ClearMarkers();
		}).AddTo(this);
		DrawMarkers();
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			UpdateHandler();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		ClearMarkers();
	}

	private void UpdateHandler()
	{
		if (base.ViewModel.VisibleMarkers.Any())
		{
			UpdateBorders();
		}
	}

	private void DrawMarkers()
	{
		ClearMarkers();
		foreach (PointMarkerVM pointMarker in base.ViewModel.PointMarkers)
		{
			PointMarkerPCView widget = WidgetFactory.GetWidget(m_MarkerView);
			widget.Initialize(this);
			widget.Bind(pointMarker);
			widget.transform.SetParent(m_MarkersContainer, worldPositionStays: false);
			m_Markers.Add(widget);
		}
	}

	private void ClearMarkers()
	{
		m_Markers.ForEach(delegate(PointMarkerPCView marker)
		{
			marker.Bind(null);
			WidgetFactory.DisposeWidget(marker);
		});
		m_Markers.Clear();
	}

	private void UpdateBorders()
	{
		Borders.Clear();
		RectTransform[] influencingContainers = m_InfluencingContainers;
		foreach (RectTransform rectTransform in influencingContainers)
		{
			if (rectTransform != null && rectTransform.gameObject.activeInHierarchy)
			{
				Borders.AddRange(ParseBorders(rectTransform));
			}
		}
	}

	private List<LineSegment2> ParseBorders(RectTransform container)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, UICamera.WorldToScreenPoint(container.position), UICamera, out var localPoint);
		Vector2 vector = new Vector2((localPoint.x + RectTransform.rect.width * RectTransform.pivot.x) * ScreenScale, (localPoint.y + RectTransform.rect.height * RectTransform.pivot.y) * ScreenScale);
		m_XMin = vector.x - container.rect.width * container.localScale.x * ScreenScale * container.pivot.x;
		m_XMax = vector.x - container.rect.width * container.localScale.x * ScreenScale * (container.pivot.x - 1f);
		m_YMin = vector.y - container.rect.height * container.localScale.y * ScreenScale * container.pivot.y;
		m_YMax = vector.y - container.rect.height * container.localScale.y * ScreenScale * (container.pivot.y - 1f);
		m_ParsedBorders.Clear();
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMin, m_YMin),
			PointB = new Vector2(m_XMin, m_YMax)
		});
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMin, m_YMin),
			PointB = new Vector2(m_XMax, m_YMin)
		});
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMax, m_YMin),
			PointB = new Vector2(m_XMax, m_YMax)
		});
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMin, m_YMax),
			PointB = new Vector2(m_XMax, m_YMax)
		});
		return m_ParsedBorders;
	}
}
