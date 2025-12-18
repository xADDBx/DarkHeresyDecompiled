using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewCluesMarkersView : View<ObservableList<INewIconTarget>>
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_MarkersContainer;

	[SerializeField]
	private RectTransform m_MovableTransform;

	[Header("Views")]
	[SerializeField]
	private NewCluePointerMarkerView m_MarkerPrefab;

	private readonly List<NewCluePointerMarkerView> m_Markers = new List<NewCluePointerMarkerView>();

	private readonly Dictionary<LineDirection, (Vector3, Vector3)> m_DirectionToBroderPts = new Dictionary<LineDirection, (Vector3, Vector3)>();

	protected override void OnBind()
	{
		DelayedBind();
	}

	private void DelayedBind()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(0.7.Seconds(), UnityTimeProvider.TimeUpdateIgnoreTimeScale), delegate
		{
			Vector3[] array = new Vector3[4];
			m_MarkersContainer.GetWorldCorners(array);
			m_DirectionToBroderPts.Clear();
			m_DirectionToBroderPts.Add(LineDirection.Left, (array[0], array[1]));
			m_DirectionToBroderPts.Add(LineDirection.Up, (array[1], array[2]));
			m_DirectionToBroderPts.Add(LineDirection.Right, (array[2], array[3]));
			m_DirectionToBroderPts.Add(LineDirection.Down, (array[3], array[0]));
			base.ViewModel.ObserveAdd().Subscribe(delegate(CollectionAddEvent<INewIconTarget> value)
			{
				CreateMarker(value.Value);
			}).AddTo(this);
			base.ViewModel.ForEach(CreateMarker);
			base.ViewModel.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<INewIconTarget> value)
			{
				NewCluePointerMarkerView newCluePointerMarkerView = m_Markers.FirstOrDefault((NewCluePointerMarkerView m) => m.ViewModel == value.Value);
				WidgetFactory.DisposeWidget(newCluePointerMarkerView);
				m_Markers.Remove(newCluePointerMarkerView);
			}).AddTo(this);
		}).AddTo(this);
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(UpdatePositions).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Markers.ForEach(WidgetFactory.DisposeWidget);
		m_Markers.Clear();
	}

	private void CreateMarker(INewIconTarget target)
	{
		NewCluePointerMarkerView widget = WidgetFactory.GetWidget(m_MarkerPrefab);
		widget.transform.SetParent(m_MarkersContainer, worldPositionStays: false);
		widget.Bind(target);
		m_Markers.Add(widget);
	}

	private void UpdatePositions()
	{
		foreach (NewCluePointerMarkerView marker in m_Markers)
		{
			Vector3? position = null;
			foreach (KeyValuePair<LineDirection, (Vector3, Vector3)> directionToBroderPt in m_DirectionToBroderPts)
			{
				if (!(marker.NewClueMarker == null))
				{
					position = IntersectSegments(directionToBroderPt.Value.Item1, directionToBroderPt.Value.Item2, m_MovableTransform.position, marker.NewClueMarker.position);
					if (position.HasValue)
					{
						break;
					}
				}
			}
			marker.SetTransformValues(position, m_MovableTransform.localScale.x);
		}
	}

	private static Vector3? IntersectSegments(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		Vector3 vector = p2 - p1;
		Vector3 b = p4 - p3;
		float num = Cross(vector, b);
		if (Mathf.Abs(num) < float.Epsilon)
		{
			return null;
		}
		float num2 = Cross(p3 - p1, b) / num;
		float num3 = Cross(p3 - p1, vector) / num;
		if (num2 < -1E-45f || num2 > 1f || num3 < -1E-45f || num3 > 1f)
		{
			return null;
		}
		return p1 + num2 * vector;
	}

	private static float Cross(Vector3 a, Vector3 b)
	{
		return a.x * b.y - a.y * b.x;
	}
}
