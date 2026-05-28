using System;
using System.Threading;
using System.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Owlcat.UI.Navigation;

[RequireComponent(typeof(RectTransform))]
public class FocusFrame : UIBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Flags]
	private enum ModeFlags
	{
		None = 0,
		Hover = 1,
		Focus = 4,
		Selected = 8
	}

	private class AsyncSwitchHelper
	{
		private CancellationTokenSource m_Source;

		public async ValueTask Run(Func<CancellationToken, ValueTask> operation)
		{
			m_Source?.Cancel();
			m_Source = new CancellationTokenSource();
			await operation(m_Source.Token);
		}
	}

	[SerializeField]
	private ModeFlags m_Flags = (ModeFlags)(-1);

	[SerializeField]
	private RectTransform m_Prefab;

	[SerializeField]
	private RectOffset m_Padding;

	private RectTransform m_Instance;

	private ModeFlags m_Active;

	private readonly AsyncSwitchHelper m_Helper = new AsyncSwitchHelper();

	protected override void Awake()
	{
		if (TryGetComponent<Toggle>(out var component))
		{
			component.OnValueChangedAsObservable().Subscribe(OnToggleValueChanged).AddTo(base.gameObject);
		}
		if (TryGetComponent<OwlcatToggle>(out var component2))
		{
			component2.IsOn.Subscribe(OnToggleValueChanged).AddTo(base.gameObject);
		}
	}

	private void Show(ModeFlags mode)
	{
		Change(m_Active | mode);
	}

	private void Hide(ModeFlags mode)
	{
		Change(m_Active & ~mode);
	}

	private void Change(ModeFlags mode)
	{
		if (m_Active != mode)
		{
			bool num = (m_Active & m_Flags) != 0;
			m_Active = mode;
			bool flag = (m_Active & m_Flags) != 0;
			if (num != flag)
			{
				m_Helper.Run(flag ? new Func<CancellationToken, ValueTask>(ShowAsync) : new Func<CancellationToken, ValueTask>(HideAsync));
			}
		}
	}

	private async ValueTask ShowAsync(CancellationToken token)
	{
		if (!(m_Instance != null) && !token.IsCancellationRequested)
		{
			m_Instance = await WidgetPool.RetainAsync(m_Prefab, base.transform, token);
			m_Instance.anchorMin = Vector2.zero;
			m_Instance.anchorMax = Vector2.one;
			m_Instance.sizeDelta = Vector2.zero;
			m_Instance.offsetMin = new Vector2(m_Padding.left, m_Padding.bottom);
			m_Instance.offsetMax = new Vector2(-m_Padding.right, -m_Padding.top);
			if (m_Instance.TryGetComponent<ITransitable>(out var component))
			{
				Transition transition = component.Show();
				token.Register(transition.Complete);
				await transition;
			}
			if (m_Instance != null && token.IsCancellationRequested)
			{
				WidgetPool.Release(Interlocked.Exchange(ref m_Instance, null), reparent: false);
			}
		}
	}

	private async ValueTask HideAsync(CancellationToken token)
	{
		RectTransform instance = Interlocked.Exchange(ref m_Instance, null);
		if (!(instance == null))
		{
			if (instance.TryGetComponent<ITransitable>(out var component))
			{
				Transition transition = component.Hide();
				token.Register(transition.Complete);
				await transition;
			}
			WidgetPool.Release(instance, reparent: false);
		}
	}

	protected override void OnDisable()
	{
		Change(ModeFlags.None);
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		Show(ModeFlags.Focus);
	}

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		Hide(ModeFlags.Focus);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		Show(ModeFlags.Hover);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		Hide(ModeFlags.Hover);
	}

	private void OnToggleValueChanged(bool value)
	{
		if (value)
		{
			Show(ModeFlags.Selected);
		}
		else
		{
			Hide(ModeFlags.Selected);
		}
	}
}
