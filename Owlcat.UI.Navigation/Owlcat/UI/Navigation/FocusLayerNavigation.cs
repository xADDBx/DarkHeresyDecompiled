using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Owlcat.UI.Navigation;

internal class FocusLayerNavigation : IDisposable
{
	internal interface INavigationMode
	{
		void OnEnter()
		{
		}

		void OnUpdate()
		{
		}

		void OnPostUpdate()
		{
		}

		void OnExit()
		{
		}
	}

	private static List<FocusLayerNavigation> s_Stack = new List<FocusLayerNavigation>();

	private readonly CompositeDisposable m_Disposables = new CompositeDisposable();

	private readonly PointerMode m_PointerMode;

	private readonly OrthogonalMode m_OrthogonalMode;

	private readonly NavigationSettings m_Settings;

	private INavigationMode m_Mode;

	public FocusLayerNavigation(FocusLayer focusLayer, NavigationSettings settings)
	{
		s_Stack.Add(this);
		m_Settings = settings;
		ref readonly IPointerProvider pointer = ref m_Settings.Pointer;
		if (pointer == null)
		{
			pointer = NavigationSettings.DefaultPointer;
		}
		EventSystem current = EventSystem.current;
		current.sendNavigationEvents = false;
		FocusCommandProviderCollection focusCommandProviderCollection = new FocusCommandProviderCollection(focusLayer);
		focusCommandProviderCollection.AddTo(m_Disposables);
		m_PointerMode = new PointerMode(current, focusLayer, focusCommandProviderCollection, m_Settings.Pointer);
		m_OrthogonalMode = new OrthogonalMode(current, focusLayer, focusCommandProviderCollection, m_Settings.Pointer);
		Observable.EveryUpdate(UnityFrameProvider.PreUpdate).Subscribe(OnPreUpdate).AddTo(m_Disposables);
		Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(OnUpdate).AddTo(m_Disposables);
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(OnLateUpdate).AddTo(m_Disposables);
	}

	public void Dispose()
	{
		m_Mode?.OnExit();
		s_Stack.Remove(this);
		m_Disposables.Dispose();
	}

	private void OnPreUpdate()
	{
		INavigationMode activeMode = GetActiveMode();
		if (m_Mode != activeMode)
		{
			m_Mode?.OnExit();
			m_Mode = activeMode;
			m_Mode?.OnEnter();
		}
	}

	private void OnUpdate(Unit _)
	{
		m_Mode?.OnUpdate();
	}

	private void OnLateUpdate(Unit _)
	{
		m_Mode?.OnPostUpdate();
	}

	private INavigationMode GetActiveMode()
	{
		List<FocusLayerNavigation> list = s_Stack;
		if (list[list.Count - 1] != this)
		{
			return null;
		}
		bool flag = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.mousePositionDelta.magnitude >= 0.1f || Input.mouseScrollDelta.magnitude > 0.1f;
		bool flag2 = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
		TMP_InputField component;
		bool num = EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out component);
		bool flag3 = true;
		bool flag4 = true;
		if (num)
		{
			return m_PointerMode;
		}
		if (flag && flag3)
		{
			return m_PointerMode;
		}
		if (flag2 && flag4)
		{
			return m_OrthogonalMode;
		}
		if (m_Mode != null)
		{
			return m_Mode;
		}
		if (flag3 && m_Settings.Pointer.Enabled)
		{
			return m_PointerMode;
		}
		if (flag4)
		{
			return m_OrthogonalMode;
		}
		return null;
	}
}
