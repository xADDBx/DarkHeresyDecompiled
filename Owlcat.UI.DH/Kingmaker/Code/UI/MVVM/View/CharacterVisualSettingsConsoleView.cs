using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsConsoleView : CharacterVisualSettingsView<CharacterVisualSettingsEntityConsoleView>
{
	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	protected ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

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
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		List<IConsoleEntity> entities = new List<IConsoleEntity> { m_OutfitMainColorSelectorView, m_ClothEntityView, m_HelmetEntityView, m_BackpackEntityView, m_HelmetAboveAllEntityView };
		m_NavigationBehaviour.AddColumn(entities);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharacterVisualSettings"
		});
		m_ClothEntityView.Or(null)?.AddInput(m_InputLayer);
		m_HelmetEntityView.AddInput(m_InputLayer);
		m_HelmetAboveAllEntityView.AddInput(m_InputLayer);
		m_BackpackEntityView.AddInput(m_InputLayer);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_InputLayer.AddAxis(RotateDoll, 2).AddTo(this);
		m_InputLayer.AddAxis(ZoomDoll, 3).AddTo(this);
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 16);
		if (m_CloseHint != null)
		{
			m_CloseHint.Bind(inputBindStruct).AddTo(this);
			m_CloseHint.SetLabel(UIStrings.Instance.CharGen.HideVisualSettings);
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_RoomTargetController.Or(null)?.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_RoomTargetController.Or(null)?.Zoom(x * m_ZoomFactor);
		}
	}
}
