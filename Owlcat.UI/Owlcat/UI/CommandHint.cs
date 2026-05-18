using System.ComponentModel;
using Owlcat.UI.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class CommandHint : View<Command>
{
	[SerializeField]
	private CommandHintIconSet m_IconSet;

	[Header("Parts")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_LongPressProgress;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[Header("Animations")]
	[SerializeField]
	private PunchScaleAnimator m_PunchScale;

	private CanvasGroup m_CanvasGroup;

	private LayoutElement m_LayoutElement;

	private bool m_IgnoreLayoutOnBind;

	private void Awake()
	{
		m_CanvasGroup = GetComponent<CanvasGroup>();
		m_LayoutElement = GetComponent<LayoutElement>();
		m_IgnoreLayoutOnBind = m_LayoutElement.ignoreLayout;
		m_CanvasGroup.alpha = 0f;
		m_LayoutElement.ignoreLayout = true;
	}

	protected override void OnBind()
	{
		if ((bool)m_Icon)
		{
			m_Icon.sprite = m_IconSet.GetIcon(base.ViewModel.Binding);
		}
		if ((bool)m_Label)
		{
			m_Label.text = base.ViewModel.Label?.ToString();
		}
		if ((bool)m_LongPressProgress)
		{
			m_LongPressProgress.enabled = base.ViewModel.Binding.Contains("#longpress");
		}
		if ((bool)m_LongPressProgress)
		{
			m_LongPressProgress.fillAmount = 0f;
		}
		base.ViewModel.Triggered += OnTriggered;
		base.ViewModel.PropertyChanged += OnPropertyChanged;
		m_CanvasGroup.alpha = 1f;
		m_LayoutElement.ignoreLayout = m_IgnoreLayoutOnBind;
	}

	protected override void OnUnbind()
	{
		if ((bool)m_Icon)
		{
			m_Icon.sprite = null;
		}
		if ((bool)m_Label)
		{
			m_Label.text = string.Empty;
		}
		base.ViewModel.Triggered -= OnTriggered;
		base.ViewModel.PropertyChanged -= OnPropertyChanged;
		m_CanvasGroup.alpha = 0f;
		m_LayoutElement.ignoreLayout = true;
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		bool flag = base.ViewModel.Active && base.ViewModel.Enabled;
		m_CanvasGroup.alpha = (flag ? 1f : 0.15f);
		if (m_LongPressProgress != null && m_LongPressProgress.enabled)
		{
			m_LongPressProgress.fillAmount = base.ViewModel.Progress;
		}
	}

	private void OnTriggered()
	{
		m_PunchScale.Play(m_Icon);
	}
}
