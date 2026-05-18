using Code.View.UI.MVVM;
using DG.Tweening;
using Dreamteck.Splines;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Components;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[ExecuteAlways]
public class TransitionMapBoardGlobalView : TransitionMapBoardBaseView, IFullScreenUIHandler, ISubscriber
{
	[Header("Ship")]
	[SerializeField]
	private Transform m_Ship;

	[SerializeField]
	private float m_FlySpeed = 500f;

	[SerializeField]
	private Ease m_FlyEase = Ease.InOutQuad;

	[Header("Ship Scale by Z")]
	[SerializeField]
	private float m_ShipZMin;

	[SerializeField]
	private float m_ShipZMax = 1000f;

	[SerializeField]
	private float m_ShipScaleMin = 0.5f;

	[SerializeField]
	private float m_ShipScaleMax = 1.5f;

	[Header("Ship Hide")]
	[SerializeField]
	private float m_HiddenShipZ = 2550f;

	private Tweener m_FlyTween;

	private SplineSample m_FlySample;

	private TransitionMapEntityVM m_FlyTarget;

	[Header("Zones")]
	[SerializeField]
	private EnumToObjectSelector<BlueprintMultiEntranceMap, GlobalMapZoneView> m_Zones;

	[Header("Path Hover")]
	[SerializeField]
	private float m_PathRevealDuration = 0.5f;

	[SerializeField]
	private Ease m_PathRevealEase = Ease.OutQuad;

	private GameObject m_HighlightedPath;

	private Tweener m_PathRevealTween;

	private new TransitionMapGlobalVM ViewModel => (TransitionMapGlobalVM)base.ViewModel;

	private void StartFlight(SplineComputer spline, TransitionMapEntityVM target)
	{
		m_FlyTarget = target;
		OnEntityHovered(target, hovered: true);
		float duration = spline.CalculateLength() / m_FlySpeed;
		SplineGraphic pathGraphic = spline.GetComponent<SplineGraphic>();
		if (pathGraphic != null)
		{
			spline.gameObject.SetActive(value: true);
			spline.Rebuild();
		}
		m_FlyTween = DOVirtual.Float(0f, 1f, duration, delegate(float t)
		{
			spline.Evaluate(t, ref m_FlySample);
			m_Ship.position = m_FlySample.position;
			UpdateShipScale();
			m_Ship.rotation = Quaternion.LookRotation(m_FlySample.forward, m_FlySample.up);
			if (pathGraphic != null)
			{
				pathGraphic.ClipFrom = t;
			}
		}).SetEase(m_FlyEase).SetUpdate(isIndependentUpdate: true)
			.OnKill(delegate
			{
				if (pathGraphic != null)
				{
					spline.gameObject.SetActive(value: false);
					pathGraphic.ResetClip();
				}
				m_FlyTween = null;
				m_FlyTarget = null;
				OnEntityHovered(target, hovered: false);
			})
			.OnComplete(delegate
			{
				if (pathGraphic != null)
				{
					spline.gameObject.SetActive(value: false);
					pathGraphic.ResetClip();
				}
				m_FlyTween = null;
				m_FlyTarget = null;
				OnEntityHovered(target, hovered: false);
				ViewModel.CurrentEntity.Value = target;
				ViewModel.CurrentEntity.Value.OnClick();
			});
	}

	private void PositionShipAtCurrentEntity()
	{
		TransitionMapEntityVM value = ViewModel.CurrentEntity.Value;
		if (value != null && value.Zone.HasValue)
		{
			Transform shipAnchor = m_Zones.GetEntity(value.Zone.Value).ShipAnchor;
			if (shipAnchor != null)
			{
				m_Ship.position = shipAnchor.position;
			}
			m_Ship.localRotation = Quaternion.Euler(0f, -90f, 90f);
		}
		UpdateShipScale();
	}

	private void UpdateShipScale()
	{
		float z = m_Ship.localPosition.z;
		float t = Mathf.InverseLerp(m_ShipZMin, m_ShipZMax, z);
		float num = Mathf.Lerp(m_ShipScaleMin, m_ShipScaleMax, t);
		m_Ship.localScale = Vector3.one * num;
	}

	private void HideShip()
	{
		Vector3 localPosition = m_Ship.localPosition;
		localPosition.z = m_HiddenShipZ;
		m_Ship.localPosition = localPosition;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Zones.EntitiesWithTypes.ForEach(delegate(EnumToObjectSelector<BlueprintMultiEntranceMap, GlobalMapZoneView>.Entity entry)
		{
			entry.Value.EntityView.gameObject.SetActive(value: false);
		});
		m_Ship.gameObject.SetActive(value: true);
		EventBus.Subscribe(this).AddTo(this);
		foreach (TransitionMapEntityVM entityVM in ViewModel.ScreenEntities)
		{
			if (entityVM == null || !entityVM.Zone.HasValue)
			{
				continue;
			}
			GlobalTransitionMapEntityView entityView = m_Zones.GetEntity(entityVM.Zone.Value).EntityView;
			if (!(entityView == null))
			{
				entityView.Bind(entityVM);
				ObservableSubscribeExtensions.Subscribe(entityView.LocationButton.OnLeftClickAsObservable(), delegate
				{
					OnEntityClicked(entityVM);
				}).AddTo(this);
				entityView.LocationButton.OnHoverAsObservable().Subscribe(delegate(bool hovered)
				{
					OnEntityHovered(entityVM, hovered);
				}).AddTo(this);
			}
		}
		ViewModel.CurrentEntity.Pairwise().Subscribe(delegate((TransitionMapEntityVM Previous, TransitionMapEntityVM Current) pair)
		{
			OnCurrentEntityChanged(pair.Previous, pair.Current);
		}).AddTo(this);
		UpdateCurrentEntityState(ViewModel.CurrentEntity.Value);
		PositionShipAtCurrentEntity();
	}

	protected override void OnUnbind()
	{
		m_FlyTween?.Kill();
		m_FlyTween = null;
		m_PathRevealTween?.Kill();
		m_PathRevealTween = null;
		foreach (EnumToObjectSelector<BlueprintMultiEntranceMap, GlobalMapZoneView>.Entity entitiesWithType in m_Zones.EntitiesWithTypes)
		{
			entitiesWithType.Value.EntityView?.Unbind();
		}
		m_Ship.gameObject.SetActive(value: false);
		base.OnUnbind();
	}

	private void OnEntityClicked(TransitionMapEntityVM entityVM)
	{
		if (m_FlyTween != null)
		{
			return;
		}
		if (entityVM == ViewModel.CurrentEntity.Value)
		{
			entityVM.OnClick();
			return;
		}
		BlueprintMultiEntranceMap? zone = ViewModel.CurrentEntity.Value.Zone;
		BlueprintMultiEntranceMap? zone2 = entityVM.Zone;
		if (!zone.HasValue || !zone2.HasValue)
		{
			return;
		}
		SplineComputer spline = GetPath(zone.Value, zone2.Value);
		if (spline != null)
		{
			HideShip();
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.Transition.ConfirmTravelText, DialogMessageBoxType.Dialog, OnConfirmTravel);
			});
		}
		else
		{
			entityVM.OnClick();
		}
		void OnConfirmTravel(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes && m_FlyTween == null && entityVM != ViewModel.CurrentEntity.Value)
			{
				StartFlight(spline, entityVM);
			}
		}
	}

	private void OnEntityHovered(TransitionMapEntityVM entityVM, bool hovered)
	{
		if (m_FlyTween != null && (entityVM != m_FlyTarget || !hovered))
		{
			return;
		}
		EventBus.RaiseEvent(delegate(ITransitionMapHighlight h)
		{
			h.HandleHighlightEntry(entityVM.Name, hovered);
		});
		if (entityVM == ViewModel.CurrentEntity.Value)
		{
			return;
		}
		BlueprintMultiEntranceMap? zone = ViewModel.CurrentEntity.Value.Zone;
		BlueprintMultiEntranceMap? zone2 = entityVM.Zone;
		if (!zone.HasValue || !zone2.HasValue)
		{
			return;
		}
		if (hovered)
		{
			SplineComputer path = GetPath(zone.Value, zone2.Value);
			if (path == null)
			{
				return;
			}
			path.Rebuild();
			m_HighlightedPath = path.gameObject;
			m_HighlightedPath.SetActive(value: true);
			path.Rebuild();
			SplineGraphic pathGraphic = path.GetComponent<SplineGraphic>();
			if (pathGraphic != null)
			{
				m_PathRevealTween?.Kill();
				pathGraphic.ClipFrom = 0f;
				pathGraphic.ClipTo = 0f;
				m_PathRevealTween = DOVirtual.Float(0f, 1f, m_PathRevealDuration, delegate(float t)
				{
					pathGraphic.ClipTo = t;
				}).SetEase(m_PathRevealEase).SetUpdate(isIndependentUpdate: true)
					.OnKill(delegate
					{
						m_PathRevealTween = null;
					})
					.OnComplete(delegate
					{
						m_PathRevealTween = null;
					});
			}
		}
		else
		{
			m_PathRevealTween?.Kill();
			m_PathRevealTween = null;
			if (m_HighlightedPath != null)
			{
				m_HighlightedPath.GetComponent<SplineGraphic>()?.ResetClip();
				m_HighlightedPath.SetActive(value: false);
				m_HighlightedPath = null;
			}
		}
	}

	private void OnCurrentEntityChanged(TransitionMapEntityVM previous, TransitionMapEntityVM current)
	{
		RestoreEntityState(previous);
		UpdateCurrentEntityState(current);
	}

	private void UpdateCurrentEntityState(TransitionMapEntityVM entityVM)
	{
		FindViewForEntity(entityVM)?.SetKindLayer("Area");
	}

	private void RestoreEntityState(TransitionMapEntityVM entityVM)
	{
		if (entityVM != null)
		{
			FindViewForEntity(entityVM)?.SetKindLayer(entityVM.Kind.ToString());
		}
	}

	private GlobalTransitionMapEntityView FindViewForEntity(TransitionMapEntityVM entityVM)
	{
		if (entityVM == null || !entityVM.Zone.HasValue)
		{
			return null;
		}
		return m_Zones.GetEntity(entityVM.Zone.Value).EntityView;
	}

	private SplineComputer GetPath(BlueprintMultiEntranceMap from, BlueprintMultiEntranceMap to)
	{
		return m_Zones.GetEntity(from).Paths?.GetEntity(to);
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.GroupChanger && state)
		{
			HideShip();
		}
	}
}
