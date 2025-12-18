using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

[Obsolete]
[TypeId("946176d009764382a8ee5a482332470d")]
public class BlueprintEncyclopediaAstropathBriefPage : BlueprintEncyclopediaPage, IEncyclopediaPageWithAvailability
{
	public class AstropathBriefBlock : IBlock
	{
		public string MessageLocation;

		public string MessageDate;

		public string MessageSender;

		public string MessageBody;

		public bool IsMessageRead;

		public BlueprintEncyclopediaAstropathBriefPage Entry;

		public AstropathBriefBlock(BlueprintEncyclopediaAstropathBriefPage entry)
		{
			Entry = entry;
		}
	}

	[SerializeField]
	private BlueprintAstropathBrief.Reference m_AstropathBrief;

	public BlueprintAstropathBrief AstropathBrief => m_AstropathBrief?.Get();

	public bool IsAvailable => false;

	public override List<IBlock> GetBlocks()
	{
		return base.GetBlocks();
	}
}
