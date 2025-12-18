using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuCollectionEntity : IContextMenuCollectionEntity
{
	public readonly bool IsHeader;

	public Sprite Icon;

	public LocalizedString Title { get; }

	public string TitleText { get; private set; }

	public string SubTitle { get; private set; }

	public Action Command { get; }

	public Func<bool> Condition { get; private set; }

	public bool IsInteractable { get; private set; } = true;


	public ButtonSoundsEnum ClickSoundType { get; }

	public ButtonSoundsEnum HoverSoundType { get; }

	public bool IsEmpty => Command == null;

	public bool IsEnabled => Condition?.Invoke() ?? true;

	public bool IsValid
	{
		get
		{
			if (IsEmpty || !IsEnabled)
			{
				return IsHeader;
			}
			return true;
		}
	}

	public ContextMenuCollectionEntity(ButtonSoundsEnum clickSoundType = ButtonSoundsEnum.NormalSound, ButtonSoundsEnum hoverSoundType = ButtonSoundsEnum.NormalSound)
	{
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(LocalizedString title, Action command, Func<bool> condition = null, ButtonSoundsEnum clickSoundType = ButtonSoundsEnum.NormalSound, ButtonSoundsEnum hoverSoundType = ButtonSoundsEnum.NormalSound)
	{
		Title = title;
		Command = command;
		Condition = condition;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(string title, string subTitle, bool isHeader = false, ButtonSoundsEnum clickSoundType = ButtonSoundsEnum.NormalSound, ButtonSoundsEnum hoverSoundType = ButtonSoundsEnum.NormalSound)
	{
		TitleText = title;
		IsHeader = isHeader;
		SubTitle = subTitle;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(LocalizedString title, Action command, bool condition, bool isInteractable = true, ButtonSoundsEnum clickSoundType = ButtonSoundsEnum.NormalSound, ButtonSoundsEnum hoverSoundType = ButtonSoundsEnum.NormalSound)
	{
		Title = title;
		Command = command;
		Condition = () => condition;
		IsInteractable = isInteractable;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(string title, Action command, bool condition, bool isInteractable = true, Sprite icon = null, ButtonSoundsEnum clickSoundType = ButtonSoundsEnum.NormalSound, ButtonSoundsEnum hoverSoundType = ButtonSoundsEnum.NormalSound)
	{
		TitleText = title;
		Command = command;
		Condition = () => condition;
		IsInteractable = isInteractable;
		Icon = icon;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public void SetNewTitleText(string text)
	{
		TitleText = text;
	}

	public void SetNewIcon(Sprite icon)
	{
		Icon = icon;
	}

	public void SetSubtitleText(string text)
	{
		SubTitle = text;
	}

	public void ForceUpdateEnabling(bool value)
	{
		Condition = () => value;
	}

	public void ForceUpdateInteractive(bool value)
	{
		IsInteractable = value;
	}

	public void Execute()
	{
		Command?.Invoke();
	}

	public ButtonSoundsEnum GetClickSoundType()
	{
		return ClickSoundType;
	}

	public ButtonSoundsEnum GetHoverSoundType()
	{
		return HoverSoundType;
	}

	bool IContextMenuCollectionEntity.IsValid()
	{
		return IsValid;
	}

	bool IContextMenuCollectionEntity.IsHeader()
	{
		return IsHeader;
	}

	bool IContextMenuCollectionEntity.IsInteractable()
	{
		return IsInteractable;
	}

	bool IContextMenuCollectionEntity.IsEmpty()
	{
		return IsEmpty;
	}

	bool IContextMenuCollectionEntity.IsEnabled()
	{
		return IsEnabled;
	}

	public Sprite GetIcon()
	{
		return Icon;
	}

	public LocalizedString GetTitleLocalized()
	{
		return Title;
	}

	public string GetTitle()
	{
		return TitleText;
	}

	public string GetSubTitle()
	{
		return SubTitle;
	}
}
