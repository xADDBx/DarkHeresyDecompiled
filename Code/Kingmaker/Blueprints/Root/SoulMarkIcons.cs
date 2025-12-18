using System;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SoulMarkIcons
{
	public Sprite Monodominance;

	public Sprite Torian;

	public Sprite Xanthite;

	public Sprite Xenophilia;

	public Sprite GetIconByDirection(AlignmentAxis direction)
	{
		return direction switch
		{
			AlignmentAxis.None => null, 
			AlignmentAxis.Monodominance => Monodominance, 
			AlignmentAxis.Torian => Torian, 
			AlignmentAxis.Xanthite => Xanthite, 
			AlignmentAxis.Xenophilia => Xenophilia, 
			_ => null, 
		};
	}
}
