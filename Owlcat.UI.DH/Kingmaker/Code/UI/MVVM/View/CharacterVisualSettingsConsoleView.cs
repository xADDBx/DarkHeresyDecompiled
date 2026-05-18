using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public sealed class CharacterVisualSettingsConsoleView : CharacterVisualSettingsView<CharacterVisualSettingsEntityConsoleView>
{
	[SerializeField]
	private HintView m_CloseHint;

	private DollRoomTargetController m_RoomTargetController;

	private float m_RotateFactor = 1f;

	private float m_ZoomFactor = 1f;

	private float m_ZoomThresholdValue = 0.01f;

	public void SetDollRoomController(DollRoomTargetController roomTargetController, float rotateFactor, float zoomFactor, float zoomThresholdValue)
	{
		m_RoomTargetController = roomTargetController;
		m_RotateFactor = rotateFactor;
		m_ZoomFactor = zoomFactor;
		m_ZoomThresholdValue = zoomThresholdValue;
	}

	protected override void OnBind()
	{
		base.OnBind();
	}

	private void RotateDoll(float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_RoomTargetController.Or(null)?.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_RoomTargetController.Or(null)?.Zoom(x * m_ZoomFactor);
		}
	}
}
