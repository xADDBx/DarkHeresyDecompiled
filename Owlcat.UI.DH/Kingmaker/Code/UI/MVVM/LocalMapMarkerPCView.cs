using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Visual.LocalMap;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapMarkerPCView : View<LocalMapMarkerVM>
{
	[Header("Elements")]
	[SerializeField]
	private CanvasGroup m_Arrow;

	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	[NonSerialized]
	public Vector2 RealPosition;

	private Vector2 m_Size;

	private IDisposable m_PingDelay;

	public void Initialize(Vector2 size)
	{
		m_Size = size;
	}

	protected override void OnBind()
	{
		base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}).AddTo(this);
		base.ViewModel.Position.Subscribe(SetPosition).AddTo(this);
		if (m_Arrow != null)
		{
			m_Arrow.alpha = 0f;
		}
		if (m_TargetPingEntity != null)
		{
			if (m_TargetPingEntity.CanvasGroup != null)
			{
				m_TargetPingEntity.CanvasGroup.alpha = 0f;
			}
			m_TargetPingEntity.DisappearAnimation();
		}
		base.ViewModel.CoopPingEntity.Subscribe(delegate((NetPlayer player, Entity entity) value)
		{
			PingEntity(value.player, value.entity);
		}).AddTo(this);
		this.SetHint(base.ViewModel.Description.CurrentValue).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_PingDelay?.Dispose();
		m_PingDelay = null;
	}

	private void SetPosition(Vector3 value)
	{
		Vector3 vector = WarhammerLocalMapRenderer.Instance.WorldToViewportPoint(value);
		base.transform.localPosition = new Vector2(m_Size.x * vector.x, m_Size.y * vector.y);
		RealPosition = base.transform.position;
	}

	public void LocalMapMarkersAlwaysInside()
	{
		if (base.ViewModel != null)
		{
			SetPosition(base.ViewModel.Position.CurrentValue);
		}
	}

	public void ShowHideArrow(bool state, Vector2 targetPosition, Vector2 actualPosition)
	{
		if (!(m_Arrow == null))
		{
			if (!state)
			{
				m_Arrow.alpha = 0f;
				return;
			}
			m_Arrow.alpha = 1f;
			Vector2 vector = actualPosition - targetPosition;
			Quaternion rotation = Quaternion.AngleAxis(Mathf.Atan2(vector.x, vector.y) * 57.29578f, Vector3.forward);
			m_Arrow.gameObject.transform.rotation = Quaternion.Inverse(rotation);
		}
	}

	public Entity GetEntity()
	{
		return base.ViewModel.GetEntity();
	}

	private void PingEntity(NetPlayer player, Entity entity)
	{
		if (!(m_TargetPingEntity == null) && entity == base.ViewModel.GetEntity())
		{
			m_PingDelay?.Dispose();
			int index = player.Index - 1;
			Image component = m_TargetPingEntity.GetComponent<Image>();
			if (component != null)
			{
				component.color = ConfigRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
			}
			m_TargetPingEntity.AppearAnimation();
			m_PingDelay = ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(7.5)), delegate
			{
				m_TargetPingEntity.DisappearAnimation();
			}).AddTo(this);
		}
	}
}
