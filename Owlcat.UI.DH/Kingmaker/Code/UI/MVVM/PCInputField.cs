using System;
using System.Collections.Generic;
using Kingmaker.UI.Workarounds;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PCInputField : MonoBehaviour, IDisposable
{
	[SerializeField]
	protected OwlcatMultiButton m_InputButton;

	[Header("Before focus")]
	[SerializeField]
	private TextMeshProUGUI m_EditLabel;

	[SerializeField]
	private TextMeshProUGUI m_FieldResult;

	[Header("When focus")]
	[SerializeField]
	protected TMP_InputField m_InputField;

	[SerializeField]
	private TextMeshProUGUI m_InputFieldWhenEmpty;

	[Header("Values")]
	[SerializeField]
	private bool m_CanSetEmptyText;

	private readonly ReactiveCommand<bool> m_OnStateChanged = new ReactiveCommand<bool>();

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>();

	private string m_InputFieldResultText = string.Empty;

	private Action<string> m_OnEndEditAction;

	public Observable<bool> OnStateChanged => m_OnStateChanged;

	public ReactiveProperty<string> CurrentText { get; } = new ReactiveProperty<string>(string.Empty);


	public void Initialize(string clickToEditText, string fieldPlaceHoldertext)
	{
		m_EditLabel.text = clickToEditText;
		m_InputFieldWhenEmpty.text = fieldPlaceHoldertext;
	}

	public IDisposable Bind(string defaultText, Action<string> onEndEditAction)
	{
		m_OnEndEditAction = onEndEditAction;
		m_InputField.text = (m_InputFieldResultText = defaultText);
		m_FieldResult.text = m_InputFieldResultText;
		m_InputField.OnEndEditAsObservable().Subscribe(OnEndEdit).AddTo(this);
		m_InputField.OnValueChangedAsObservable().Subscribe(delegate(string text)
		{
			CurrentText.Value = text;
		}).AddTo(this);
		UpdatePlaceholder();
		ObservableSubscribeExtensions.Subscribe(m_InputButton.OnLeftClickAsObservable(), delegate
		{
			OnEdit();
		}).AddTo(this);
		SetInputActive(state: false);
		return this;
	}

	public void SetText(string text)
	{
		m_InputField.text = text;
	}

	private void UpdatePlaceholder()
	{
		m_OnEndEditAction(m_InputFieldResultText);
		m_InputField.text = m_InputFieldResultText;
		m_FieldResult.text = m_InputFieldResultText;
	}

	public void OnEdit()
	{
		SetInputActive(state: true);
		m_InputField.Select();
		m_InputField.ActivateInputField();
	}

	private void OnEndEdit(string text)
	{
		SetInputActive(state: false);
		if (!string.IsNullOrEmpty(text) || m_CanSetEmptyText)
		{
			m_InputFieldResultText = text;
		}
		UpdatePlaceholder();
	}

	private void SetInputActive(bool state)
	{
		m_InputField.gameObject.SetActive(state);
		m_InputButton.gameObject.SetActive(!state);
		m_OnStateChanged?.Execute(state);
	}

	private void AddDisposable(IDisposable d)
	{
		m_Disposables.Add(d);
	}

	public void Dispose()
	{
		SetInputActive(state: false);
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
	}
}
