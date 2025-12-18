using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ActionBarSlotConsumableView : ActionBarBaseSlotView
{
	[Header("Resource Block")]
	[SerializeField]
	private TextMeshProUGUI m_ResourceCount;

	private VisibilityController m_ResourceCountVisibility;

	protected override void Awake()
	{
		base.Awake();
		m_ResourceCountVisibility = VisibilityController.Control(m_ResourceCount);
	}

	protected override void OnBind()
	{
		base.OnBind();
		if (m_ResourceCount != null)
		{
			base.ViewModel.ResourceCount.Subscribe(delegate
			{
				SetResourceCount();
			}).AddTo(this);
		}
	}

	private void SetResourceCount()
	{
		int currentValue = base.ViewModel.ResourceCount.CurrentValue;
		bool flag = currentValue > 0;
		m_ResourceCountVisibility.SetVisible(flag);
		if (flag)
		{
			m_ResourceCount.text = currentValue.ToString();
		}
	}
}
