using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InfoWindowVM : InfoBaseVM, IInfoWindowHandler, ISubscriber, IDialogStartHandler
{
	private readonly Action m_OnClose;

	public readonly bool IsStartPos;

	private readonly IEnumerable<TooltipBaseTemplate> m_TooltipTemplates;

	private readonly bool m_ShouldNotHideLittleTooltip;

	private readonly ReactiveCommand<Unit> m_ForceClose = new ReactiveCommand<Unit>();

	public readonly Vector2? LastTooltipPosition;

	protected override TooltipTemplateType TemplateType => TooltipTemplateType.Info;

	public Observable<Unit> ForceClose => m_ForceClose;

	public InfoWindowVM(TooltipBaseTemplate template, Action onClose, bool shouldNotHideLittleTooltip = false, Vector2? lastTooltipPosition = null)
		: base(template)
	{
		EventBus.Subscribe(this).AddTo(this);
		m_TooltipTemplates = new TooltipBaseTemplate[1] { template };
		m_OnClose = onClose;
		m_ShouldNotHideLittleTooltip = shouldNotHideLittleTooltip;
		LastTooltipPosition = lastTooltipPosition;
		if (template is TooltipTemplateUnitInspect)
		{
			IsStartPos = true;
		}
	}

	public InfoWindowVM(IEnumerable<TooltipBaseTemplate> templates, Action onClose, bool shouldNotHideLittleTooltip = false)
		: base(templates)
	{
		EventBus.Subscribe(this).AddTo(this);
		m_TooltipTemplates = templates;
		m_OnClose = onClose;
		m_ShouldNotHideLittleTooltip = shouldNotHideLittleTooltip;
	}

	public void OnClose()
	{
		m_OnClose();
		if (!m_ShouldNotHideLittleTooltip)
		{
			TooltipHelper.HideTooltip();
		}
	}

	[ItemCanBeNull]
	public IEnumerable<TooltipBaseTemplate> GetTooltipTemplates()
	{
		return m_TooltipTemplates;
	}

	public void HandleCloseTooltipInfoWindow()
	{
		OnClose();
	}

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		m_ForceClose.Execute(Unit.Default);
	}
}
