using System;
using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ModalWindowButton : MonoBehaviour, IDisposable
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>();

	public OwlcatMultiButton MultiButton => m_Button;

	public void Initialize(ModalWindowAction action)
	{
		m_Text.text = action.Name;
		m_Disposables.Add(ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			action.Action?.Invoke();
		}));
		m_Disposables.Add(m_Button.OnConfirmClickAsObservable().Subscribe(action.Action));
	}

	public void Dispose()
	{
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_Disposables.Clear();
	}
}
