using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(WidgetList))]
[RequireComponent(typeof(OwlcatToggleGroup))]
public class OwlcatDropdown : View<OwlcatDropdownVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private OwlcatMultiButton m_MainMultiButton;

	[SerializeField]
	private DropdownItemView m_DropdownItemView;

	[Space]
	[SerializeField]
	private DropdownItemView m_DropdownItemViewPrefab;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private OwlcatToggleGroup m_ToggleGroup;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private float m_ItemHeight = 21.6f;

	private readonly ReactiveProperty<bool> m_IsOn = new ReactiveProperty<bool>();

	private const string OnLayer = "On";

	private const string OffLayer = "Off";

	private CompositeDisposable m_Subscriptions;

	private CompositeDisposable m_PanelSubscriptions;

	private readonly List<DropdownItemView> m_Items = new List<DropdownItemView>();

	public static readonly string InputLayerContextName = "OwlcatDropdownInput";

	private GameObject m_Blocker;

	private bool m_IsEnteredWithMouse;

	private static List<Canvas> s_ListPool;

	public ReadOnlyReactiveProperty<bool> IsOn => m_IsOn;

	public IConsoleEntity ConsoleEntityProxy => m_MainMultiButton;

	public ReadOnlyReactiveProperty<int> Index => base.ViewModel.Index;

	public int VMCollectionCount => base.ViewModel.VMCollection.Count;

	protected override void OnBind()
	{
		m_ScrollRect.gameObject.SetActive(value: false);
		base.ViewModel.SelectedVM.Subscribe(ChangeValue).AddTo(this);
		CreateInput();
	}

	protected override void OnUnbind()
	{
		Clear();
	}

	private void CreateInput()
	{
		m_Subscriptions = new CompositeDisposable();
		m_Subscriptions.Add(ObservableSubscribeExtensions.Subscribe(m_MainMultiButton.OnLeftClickAsObservable(), delegate
		{
			m_IsEnteredWithMouse = true;
			SetState(value: true);
		}));
		m_Subscriptions.Add(ObservableSubscribeExtensions.Subscribe(m_MainMultiButton.OnConfirmClickAsObservable(), delegate
		{
			SetState(value: true);
		}));
	}

	private void ChangeValue(DropdownItemVM viewModel)
	{
		m_DropdownItemView.Bind(viewModel);
	}

	public string GetCurrentTextValue()
	{
		return m_DropdownItemView.TextValue;
	}

	public void SetInteractable(bool state)
	{
		m_MainMultiButton.SetInteractable(state);
	}

	public void SetIndex(int index)
	{
		base.ViewModel.SetIndex(index);
	}

	public void SetState(bool value, bool immediately = false)
	{
		if (base.ViewModel?.VMCollection != null && base.ViewModel.VMCollection.Count != 0 && m_IsOn.Value != value)
		{
			m_IsOn.Value = value;
			if (value)
			{
				Show();
			}
			else
			{
				Hide(immediately);
			}
		}
	}

	private void Show()
	{
		m_Subscriptions?.Dispose();
		m_MainMultiButton.SetFocus(value: false);
		m_MainMultiButton.SetActiveLayer("On");
		m_FadeAnimator.AppearAnimation();
		m_Blocker = CreateBlocker();
		m_ScrollRect.gameObject.SetActive(value: true);
		m_PanelSubscriptions = new CompositeDisposable();
		m_PanelSubscriptions.Add(m_WidgetList.DrawMultiEntries(base.ViewModel.VMCollection, new List<MonoBehaviour> { m_DropdownItemViewPrefab }));
		m_Items.Clear();
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is DropdownItemView dropdownItemView)
			{
				m_Items.Add(dropdownItemView);
				dropdownItemView.SetToggleGroup(m_ToggleGroup);
				if (dropdownItemView.ViewModel == base.ViewModel.SelectedVM.CurrentValue)
				{
					dropdownItemView.Toggle.Set(value: true);
				}
				dropdownItemView.SetItemHeight(m_ItemHeight);
			}
		}
		m_PanelSubscriptions.Add((from v in m_ToggleGroup.ActiveToggle.Skip(1)
			where v != null
			select v).Subscribe(delegate(OwlcatToggle toggle)
		{
			int index = m_Items.FindIndex((DropdownItemView item) => item.Toggle == toggle);
			SetIndex(index);
			SetState(value: false);
		}));
		SystemSounds.Instance.DropdownMenu.Show.Play();
	}

	private void Hide(bool immediately = false)
	{
		m_MainMultiButton.SetActiveLayer("Off");
		m_MainMultiButton.SetFocus(!m_IsEnteredWithMouse);
		if (immediately)
		{
			Clear();
			CreateInput();
			m_ScrollRect.gameObject.SetActive(value: false);
			SystemSounds.Instance.DropdownMenu.Hide.Play();
			return;
		}
		m_FadeAnimator.DisappearAnimation(delegate
		{
			Clear();
			CreateInput();
			m_ScrollRect.gameObject.SetActive(value: false);
			SystemSounds.Instance.DropdownMenu.Hide.Play();
		});
	}

	private void Clear()
	{
		m_IsEnteredWithMouse = false;
		m_Items.Clear();
		m_Subscriptions?.Dispose();
		m_Subscriptions = null;
		m_PanelSubscriptions?.Dispose();
		m_PanelSubscriptions = null;
		m_WidgetList.Clear();
		if (m_Blocker != null)
		{
			UnityEngine.Object.Destroy(m_Blocker);
		}
	}

	private GameObject CreateBlocker()
	{
		s_ListPool = CollectionPool<List<Canvas>, Canvas>.Get();
		base.gameObject.GetComponentsInParent(includeInactive: false, s_ListPool);
		if (s_ListPool.Count == 0)
		{
			return null;
		}
		List<Canvas> list = s_ListPool;
		Canvas canvas = list[list.Count - 1];
		using (IEnumerator<Canvas> enumerator = s_ListPool.Where((Canvas t) => t.isRootCanvas).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				canvas = enumerator.Current;
			}
		}
		CollectionPool<List<Canvas>, Canvas>.Release(s_ListPool);
		GameObject gameObject = new GameObject("Blocker");
		RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
		rectTransform.SetParent(canvas.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector3.zero;
		rectTransform.anchorMax = Vector3.one;
		rectTransform.sizeDelta = Vector2.zero;
		GameObject gameObject2 = m_ScrollRect.gameObject;
		Canvas canvas2 = gameObject.AddComponent<Canvas>();
		canvas2.overrideSorting = true;
		Canvas component = gameObject2.GetComponent<Canvas>();
		canvas2.sortingLayerID = component.sortingLayerID;
		canvas2.sortingOrder = component.sortingOrder - 1;
		Canvas canvas3 = null;
		Transform parent = gameObject2.transform.parent;
		while (parent != null)
		{
			canvas3 = parent.GetComponent<Canvas>();
			if (canvas3 != null)
			{
				break;
			}
			parent = parent.parent;
		}
		if (canvas3 != null)
		{
			Component[] components = canvas3.GetComponents<BaseRaycaster>();
			components = components;
			for (int i = 0; i < components.Length; i++)
			{
				Type type = components[i].GetType();
				if (gameObject.GetComponent(type) == null)
				{
					gameObject.AddComponent(type);
				}
			}
		}
		else
		{
			GetOrAddComponent<GraphicRaycaster>(gameObject);
		}
		gameObject.AddComponent<Image>().color = Color.clear;
		gameObject.AddComponent<Button>().onClick.AddListener(delegate
		{
			SetState(value: false);
		});
		return gameObject;
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!val)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void Scroll(float value)
	{
		m_ScrollRect.Scroll(value, smooth: true);
	}

	public void OnDisable()
	{
		SetState(value: false, immediately: true);
	}
}
