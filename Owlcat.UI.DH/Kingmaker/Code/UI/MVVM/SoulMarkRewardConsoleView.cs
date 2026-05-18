using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SoulMarkRewardConsoleView : SoulMarkRewardBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private Vector2 m_TooltipPivot = new Vector2(0f, 0.5f);

	[SerializeField]
	private RectTransform m_TooltipPlace;

	private readonly ReactiveProperty<bool> m_TooltipShown = new ReactiveProperty<bool>();

	private TooltipConfig m_TooltipConfig;

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			PriorityPivots = new List<Vector2> { m_TooltipPivot },
			TooltipPlace = m_TooltipPlace
		};
		CreateInput();
		EventBus.Subscribe(this).AddTo(this);
	}

	private void CreateInput()
	{
	}

	private void OnAccept()
	{
		base.ViewModel.OnDeclinePressed();
	}
}
