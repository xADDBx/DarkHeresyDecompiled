using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.UI.Pointer;
using Kingmaker.View;
using Kingmaker.Visual.LocalMap;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapConsoleView : LocalMapBaseView
{
	[SerializeField]
	private float m_StickCameraMoveFactor = 30f;

	[SerializeField]
	private float m_StickCursorFactor = 2f;

	[SerializeField]
	private float m_StickMapDragFactor = 1f;

	[SerializeField]
	private HintView m_OpenLegendHint;

	[SerializeField]
	private HintView m_HideLegendHint;

	private readonly ReactiveProperty<bool> m_LegendShowed = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CursorActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_RotateCameraMode = new ReactiveProperty<bool>();

	private Vector2 m_CameraFollowerVector;

	protected override void OnBind()
	{
		base.OnBind();
		m_CursorActive.Value = false;
		m_RotateCameraMode.Value = Game.Instance.Player.IsCameraRotateMode;
		AddInput();
	}

	public void AddInput()
	{
	}

	private static void RotateCamera(bool direction)
	{
		CameraRig instance = CameraRig.Instance;
		if (direction)
		{
			instance.RotateRight();
		}
		else
		{
			instance.RotateLeft();
		}
	}

	private void SwitchCursor(bool state)
	{
		m_CursorActive.Value = state;
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleHintRequest(null, shouldShow: true);
		});
	}

	private void SwitchCameraMode(bool state)
	{
		m_RotateCameraMode.Value = (Game.Instance.Player.IsCameraRotateMode = state);
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleHintRequest(null, shouldShow: true);
		});
	}

	private void InteractLocalMapHistory(bool state)
	{
		ShowLocalMapHistory(state);
		m_LegendShowed.Value = state;
	}

	private void OnLeftStickMove(Vector2 vec)
	{
		UpdateMapPosition(vec * m_CurrentZoom * (0f - m_StickMapDragFactor));
	}

	private void OnRightStickMove(Vector2 vec)
	{
		if (m_RotateCameraMode.Value)
		{
			if (vec.x < -0.5f)
			{
				RotateCamera(direction: true);
			}
			if (vec.x > 0.5f)
			{
				RotateCamera(direction: false);
			}
			float y = vec.y;
			if (y < -0.2f || y > 0.2f)
			{
				SetMapScale(vec.y / 5f);
			}
		}
		else
		{
			vec = base.ViewModel.LocalMapRotation switch
			{
				BlueprintAreaPart.LocalMapRotationDegree.Degree0 => new Vector2(vec.x, vec.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree90 => new Vector2(vec.y, 0f - vec.x), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree180 => new Vector2(0f - vec.x, 0f - vec.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree270 => new Vector2(0f - vec.y, vec.x), 
				_ => vec, 
			};
			Game.Instance.Controllers.CameraController.Follower.Release();
			Vector3 vector = WarhammerLocalMapRenderer.Instance.WorldToViewportPoint(CameraRig.Instance.transform.position);
			vector.x += vec.x / m_StickCameraMoveFactor;
			vector.y += vec.y / m_StickCameraMoveFactor;
			base.ViewModel.OnClick(vector, state: true, null, canPing: false);
			m_CameraFollowerVector = vector;
		}
	}

	private void ConfirmClick()
	{
		if (!m_CursorActive.Value)
		{
			base.ViewModel.OnClick(m_CameraFollowerVector, state: false);
			return;
		}
		Vector2 viewportPos = GetViewportPos(CursorController.CursorPosition);
		base.ViewModel.OnClick(viewportPos, state: false);
		base.ViewModel.OnClick(viewportPos, state: true);
		UIKitSoundManager.PlayButtonClickSound();
	}

	protected override void InteractableRightButtons()
	{
		base.InteractableRightButtons();
		Vector3 localScale = m_Image.rectTransform.localScale;
		m_MinZoom.Value = localScale.x > base.ZoomMin && localScale.y > base.ZoomMin;
		m_MaxZoom.Value = localScale.x < base.ZoomMax && localScale.y < base.ZoomMax;
	}

	private void Close()
	{
		SwitchCursor(state: false);
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}
}
