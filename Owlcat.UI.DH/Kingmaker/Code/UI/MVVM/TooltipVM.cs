using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipVM : InfoBaseVM
{
	public readonly RectTransform OwnerTransform;

	public readonly TooltipBackground Background;

	public readonly InfoCallPCMethod InfoCallPCMethod;

	public readonly InfoCallConsoleMethod InfoCallConsoleMethod;

	public int MaxHeight;

	public readonly int PreferredHeight;

	public readonly int Width;

	public readonly bool IsGlossary;

	public readonly bool HasScroll;

	public readonly List<Vector2> PriorityPivots;

	public readonly ConsoleNavigationBehaviour OwnerNavigationBehaviour;

	public readonly bool IsComparative;

	public readonly bool ShouldNotHideLittleTooltip;

	public Vector2 LastPosition { get; private set; }

	protected override TooltipTemplateType TemplateType => TooltipTemplateType.Tooltip;

	public TooltipVM(TooltipData data, bool isComparative = false, bool shouldNotHideLittleTooltip = false, bool hasScroll = false)
		: base(data.MainTemplate)
	{
		PriorityPivots = data.Config.PriorityPivots;
		OwnerTransform = data.Config.TooltipPlace;
		Background = data.MainTemplate.Background;
		InfoCallPCMethod = data.Config.InfoCallPCMethod;
		InfoCallConsoleMethod = data.Config.InfoCallConsoleMethod;
		PreferredHeight = data.Config.PreferredHeight;
		MaxHeight = data.Config.MaxHeight;
		Width = data.Config.Width;
		IsComparative = isComparative;
		IsGlossary = data.Config.IsGlossary;
		ShouldNotHideLittleTooltip = shouldNotHideLittleTooltip;
		OwnerNavigationBehaviour = data.OwnerNavigationBehaviour;
		HasScroll = hasScroll;
		if (Game.Instance.IsControllerMouse && !data.Config.IsEncyclopedia)
		{
			SetupPCInteractionHint();
		}
		ObservableSubscribeExtensions.Subscribe(data.CloseCommand?, delegate
		{
			TooltipHelper.HideTooltip();
		}).AddTo(this);
	}

	public void SetLastWorldPosition(Vector2 position)
	{
		LastPosition = position;
	}

	private void SetupPCInteractionHint()
	{
		switch (InfoCallPCMethod)
		{
		case InfoCallPCMethod.LeftMouseButton:
			AddHintBrick(UIStrings.Instance.Tooltips.ShowInfo);
			break;
		case InfoCallPCMethod.RightMouseButton:
			AddHintBrick(UIStrings.Instance.Tooltips.ShowInfo);
			break;
		case InfoCallPCMethod.ShiftRightMouseButton:
			AddHintBrick(UIStrings.Instance.Tooltips.ShowInfo);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case InfoCallPCMethod.None:
			break;
		}
	}

	private void AddHintBrick(string hintString)
	{
		HintBricks.Add(new TooltipBrickHintVM(hintString));
	}

	public void OverrideMaxHeight(int height)
	{
		MaxHeight = height;
	}
}
