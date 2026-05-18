using System;
using Kingmaker.Localization.Enums;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class OwlcatInputField : MonoBehaviour, IConfirmClickHandler, IConsoleEntity, IScrollHandler, IEventSystemHandler, IConsolePointerLeftClickEvent, IConsoleNavigationEntity
{
	[SerializeField]
	private TMP_InputField m_InputField;

	[SerializeField]
	private HintView m_ConfirmHint;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	private TextMeshProUGUI m_PlaceholderTextLabel;

	private string m_TittleText;

	private string m_PlaceholderText;

	private uint m_MaxTextLength = 128u;

	private bool m_IsEnteredWithMouse;

	private CompositeDisposable m_Disposables = new CompositeDisposable();

	private IDisposable m_Disposable;

	public static readonly string InputLayerContextName = "InputField";

	private Locale m_KeyboardLanguage;

	private bool m_IsEditing;

	private IVirtualKeyboard m_VirtualKeyboard;

	public ReactiveCommand<Unit> PointerLeftClickCommand { get; } = new ReactiveCommand<Unit>();


	private TextMeshProUGUI PlaceholderTextLabel => m_PlaceholderTextLabel.Or(null) ?? (m_PlaceholderTextLabel = m_InputField.placeholder.GetComponent<TextMeshProUGUI>());

	private IVirtualKeyboard VirtualKeyboard
	{
		get
		{
			IVirtualKeyboard virtualKeyboard = m_VirtualKeyboard;
			if (virtualKeyboard == null)
			{
				IVirtualKeyboard virtualKeyboard3;
				IVirtualKeyboard virtualKeyboard2;
				if (!ApplicationHelper.IsRunOnSteamDeck)
				{
					virtualKeyboard2 = new PCVirtualKeyboard(m_InputField);
					virtualKeyboard3 = virtualKeyboard2;
				}
				else
				{
					virtualKeyboard2 = SteamDeckVirtualKeyboard.Create();
					virtualKeyboard3 = virtualKeyboard2;
				}
				virtualKeyboard2 = virtualKeyboard3;
				m_VirtualKeyboard = virtualKeyboard3;
				virtualKeyboard = virtualKeyboard2;
			}
			return virtualKeyboard;
		}
	}

	public string Text
	{
		get
		{
			return m_InputField.text;
		}
		set
		{
			m_InputField.text = value ?? "";
		}
	}

	private void OnEnable()
	{
		m_Disposable = ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			if (!m_IsEditing)
			{
				m_IsEnteredWithMouse = true;
				PointerLeftClickCommand.Execute(Unit.Default);
				SelectInputFiled();
			}
		});
	}

	private void OnDisable()
	{
		m_Disposable?.Dispose();
		Abort();
	}

	public void SetTittle(string text)
	{
		m_TittleText = text;
	}

	public void SetPlaceholderText(string text)
	{
		m_PlaceholderText = text;
		PlaceholderTextLabel.text = text ?? "";
	}

	public void SetLanguage(Locale language)
	{
		m_KeyboardLanguage = language;
	}

	public void SetMaxTextLength(uint maxTextLength)
	{
		m_MaxTextLength = maxTextLength;
	}

	public void SetFocus(bool value)
	{
		if (!m_IsEnteredWithMouse || !value)
		{
			m_Button.SetFocused(value);
		}
		if (!value)
		{
			Abort();
		}
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public void SelectInputFiled()
	{
		if (m_IsEditing)
		{
			Abort();
			return;
		}
		m_IsEditing = true;
		m_Button.SetActiveLayer("On");
		m_InputField.OnSelect(null);
		VirtualKeyboard.OpenKeyboard(OnKeyboardEditSucceeded, OnKeyboardEditDeclined, m_TittleText, Text, m_PlaceholderText, m_KeyboardLanguage, m_MaxTextLength);
	}

	public void OnScroll(PointerEventData eventData)
	{
		m_InputField.OnScroll(eventData);
	}

	private void OnConsoleScroll(float value)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, value * m_InputField.scrollSensitivity);
		m_InputField.OnScroll(pointerEventData);
	}

	public void Abort()
	{
		if (m_IsEditing)
		{
			m_IsEditing = false;
			m_Disposables.Clear();
			m_Button.SetActiveLayer("Off");
			m_InputField.OnDeselect(null);
			m_InputField.ReleaseSelection();
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			VirtualKeyboard.Abort();
			m_IsEnteredWithMouse = false;
		}
	}

	private void OnKeyboardEditSucceeded(string text)
	{
		Abort();
		Text = text;
	}

	private void OnKeyboardEditDeclined()
	{
		Abort();
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		if (!TrySubmitInputField())
		{
			SelectInputFiled();
		}
	}

	private bool TrySubmitInputField()
	{
		if (m_IsEditing)
		{
			bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			bool keyDown = Input.GetKeyDown(KeyCode.Return);
			if ((m_IsEnteredWithMouse || !m_InputField.multiLine || flag) && keyDown)
			{
				m_InputField.OnSubmit(null);
				return true;
			}
		}
		return false;
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
