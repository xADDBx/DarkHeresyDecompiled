using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("c89779b02bc24f8d8717507c1f3b7939")]
public class BlueprintEncyclopediaEntry : BlueprintScriptableObject, IPage, INode
{
	public LocalizedString Title;

	public bool HideInEncyclopedia;

	public List<EncyclopediaEntryBlock> Blocks;

	public string Key => name;

	public BlueprintEncyclopediaNode Parent => null;

	public bool FirstExpanded => false;

	public List<EncyclopediaEntryBlock> GetTooltipInfo()
	{
		return Blocks.Where((EncyclopediaEntryBlock b) => b.IsVisibleInTooltip).ToList();
	}

	public List<EncyclopediaEntryBlock> GetFullInfo()
	{
		return Blocks;
	}

	public List<INode> GetRootBranch()
	{
		return new List<INode>();
	}

	public List<IBlock> GetBlocks()
	{
		return ((IEnumerable<EncyclopediaEntryBlock>)GetFullInfo()).Select((Func<EncyclopediaEntryBlock, IBlock>)((EncyclopediaEntryBlock b) => b)).ToList();
	}

	public List<SpriteLink> GetImages()
	{
		return new List<SpriteLink>();
	}

	public string GetTitle()
	{
		return Title.Text;
	}

	public bool IsChilds()
	{
		return false;
	}

	public List<IPage> GetChilds()
	{
		return new List<IPage>();
	}
}
