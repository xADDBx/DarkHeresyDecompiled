using System;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using TMPro;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

[Serializable]
public class EncyclopediaEntryBlock : IBlock
{
	public enum BlockType
	{
		Text,
		Image,
		Video,
		EntryReference
	}

	public BlockType blockType;

	[Tooltip("Если выключено, блок не будет показываться в тултипах.")]
	public bool IsVisibleInTooltip;

	[ShowIf("isText")]
	public LocalizedString Text;

	[ShowIf("isText")]
	public TextAlignmentOptions TextAlignment;

	[ShowIf("isText")]
	public bool HasConsoleDescription;

	[ShowIf("showConsoleDescription")]
	public LocalizedString ConsoleDescription;

	[ShowIf("showConsoleDescription")]
	public TextAlignmentOptions ConsoleTextAlignment;

	[ShowIf("isImage")]
	public SpriteLink ImageLink;

	[ShowIf("isVideo")]
	public VideoLink VideoLink;

	private bool isText => blockType == BlockType.Text;

	private bool showConsoleDescription
	{
		get
		{
			if (HasConsoleDescription)
			{
				return blockType == BlockType.Text;
			}
			return false;
		}
	}

	private bool isImage => blockType == BlockType.Image;

	private bool isVideo => blockType == BlockType.Video;

	public LocalizedString GetDescription()
	{
		if (Game.Instance.IsControllerMouse || !HasConsoleDescription || string.IsNullOrWhiteSpace(ConsoleDescription))
		{
			return Text;
		}
		return ConsoleDescription;
	}

	public Sprite GetImage()
	{
		if (!isImage)
		{
			return null;
		}
		return ImageLink.Loaded;
	}

	public VideoLink GetVideo()
	{
		if (!isVideo)
		{
			return null;
		}
		return VideoLink;
	}
}
