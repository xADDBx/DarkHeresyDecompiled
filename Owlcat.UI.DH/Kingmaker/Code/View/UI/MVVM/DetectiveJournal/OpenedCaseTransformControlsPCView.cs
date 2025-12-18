using System;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseTransformControlsPCView : OpenedCaseTransformControlsBaseView
{
	[Header("Controls")]
	[SerializeField]
	private float m_KeyboardSpeed = 30f;

	[SerializeField]
	private OwlcatMultiButton m_ZoomInButton;

	[SerializeField]
	private OwlcatMultiButton m_ZoomOutButton;

	[SerializeField]
	private OwlcatMultiButton m_ResetButton;

	private const float InteractableZoomThreshold = 0.01f;

	private const float InteractablePositionThreshold = 0.005f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Transform.EnsureComponent<Image>().OnDragAsObservable().Subscribe(base.OnDrag)
			.AddTo(this);
		m_ZoomInButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.ZoomInClick).AddTo(this);
		m_ZoomOutButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.ZoomOutClick).AddTo(this);
		m_ResetButton.OnLeftClickAsObservable().Subscribe(ResetPositionButtonClick).AddTo(this);
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(UpdateButtonsInteractableState).AddTo(this);
		ApplyBindAction(delegate(string s, Action action)
		{
			Game.Instance.Keyboard.Bind(s, action);
		});
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		ApplyBindAction(Game.Instance.Keyboard.Unbind);
	}

	private void UpdateButtonsInteractableState()
	{
		float currentZoom = base.ViewModel.CurrentZoom01;
		bool interactable = currentZoom < 0.99f;
		m_ZoomInButton.Interactable = interactable;
		bool interactable2 = currentZoom > 0.01f;
		m_ZoomOutButton.Interactable = interactable2;
		bool interactable3 = Mathf.Abs(currentZoom - 0.5f) > 0.01f || Mathf.Abs(m_Transform.pivot.x - 0.5f) > 0.005f || Mathf.Abs(m_Transform.pivot.y - 0.5f) > 0.005f;
		m_ResetButton.Interactable = interactable3;
	}

	private void ResetPositionButtonClick()
	{
		ResetPosition();
		UISounds.Instance.Play(UISounds.Instance.Sounds.Common.ResetZoomAndPositionButton, isButton: true);
	}

	private void ApplyBindAction(Action<string, Action> bindAction)
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		bindAction(uIKeybindGeneralSettings.ScrollLeft.name, delegate
		{
			DragToDirection(Vector2.left);
		});
		bindAction(uIKeybindGeneralSettings.ScrollRight.name, delegate
		{
			DragToDirection(Vector2.right);
		});
		bindAction(uIKeybindGeneralSettings.ScrollUp.name, delegate
		{
			DragToDirection(Vector2.up);
		});
		bindAction(uIKeybindGeneralSettings.ScrollDown.name, delegate
		{
			DragToDirection(Vector2.down);
		});
	}

	private void DragToDirection(Vector2 direction)
	{
		Vector2 delta = -direction.normalized * m_KeyboardSpeed;
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			delta = delta
		};
		OnDrag(eventData);
	}
}
