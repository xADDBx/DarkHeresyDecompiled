using System;
using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BugDuplicateItemView : View<BugDuplicateItemVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	protected OwlcatMultiButton m_FocusButton;

	[SerializeField]
	protected TextMeshProUGUI m_TitleText;

	[SerializeField]
	protected TextMeshProUGUI m_StatusText;

	[SerializeField]
	protected TextMeshProUGUI m_BuildStatusText;

	[SerializeField]
	protected TextMeshProUGUI m_DistanceText;

	[SerializeField]
	protected TextMeshProUGUI m_CreatedText;

	[SerializeField]
	protected TextMeshProUGUI m_FixVersionText;

	[SerializeField]
	protected TextMeshProUGUI m_AssigneeText;

	[SerializeField]
	protected Image m_PriorityIcon;

	[SerializeField]
	private Image m_FixedImage;

	[Header("Colors")]
	[SerializeField]
	private Color m_NonFixedColor;

	[SerializeField]
	private Color m_FixedColor;

	[Space]
	[SerializeField]
	private Color m_CreatedTimeNewColor;

	[SerializeField]
	private Color m_CreatedTimeMiddleColor;

	[SerializeField]
	private Color m_CreatedTimeOldColor;

	[Header("Sprites")]
	[SerializeField]
	private List<Sprite> m_PriorityIcons;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_OpenButton;

	[SerializeField]
	private OwlcatButton m_MetButton;

	[SerializeField]
	private TextMeshProUGUI m_OpenButtonText;

	[SerializeField]
	private TextMeshProUGUI m_MetButtonText;

	protected override void OnBind()
	{
		m_Button.OnLeftClickAsObservable().Subscribe(OpenUrl).AddTo(this);
		m_Button.OnConfirmClickAsObservable().Subscribe(OpenUrl).AddTo(this);
		m_OpenButton.OnLeftClickAsObservable().Subscribe(OpenUrl).AddTo(this);
		m_MetButton.OnLeftClickAsObservable().Subscribe(OpenMet).AddTo(this);
		DateTime result;
		double num = (DateTime.TryParse(base.ViewModel.Created, out result) ? (DateTime.UtcNow - result).TotalDays : 0.0);
		TextMeshProUGUI createdText = m_CreatedText;
		Color color = ((num > 356.0) ? m_CreatedTimeOldColor : ((!(num > 60.0)) ? m_CreatedTimeNewColor : m_CreatedTimeMiddleColor));
		createdText.color = color;
		m_FixedImage.color = (base.ViewModel.IsFixed ? m_FixedColor : m_NonFixedColor);
		m_TitleText.text = base.ViewModel.Title;
		m_StatusText.text = base.ViewModel.Status;
		m_BuildStatusText.text = base.ViewModel.BuildStatus;
		TextMeshProUGUI distanceText = m_DistanceText;
		int distance = base.ViewModel.Distance;
		distanceText.text = distance.ToString();
		m_CreatedText.text = base.ViewModel.Created;
		m_FixVersionText.text = base.ViewModel.FixVersion;
		m_AssigneeText.text = base.ViewModel.Assignee ?? "";
		m_OpenButtonText.text = "Open";
		m_MetButtonText.text = "Met";
		m_PriorityIcon.sprite = m_PriorityIcons[base.ViewModel.PriorityType - 1];
	}

	public void OpenUrl()
	{
		Application.OpenURL(base.ViewModel.JiraTaskUrl);
	}

	public void OpenMet()
	{
		Application.OpenURL(base.ViewModel.MetUrl);
	}

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}
}
