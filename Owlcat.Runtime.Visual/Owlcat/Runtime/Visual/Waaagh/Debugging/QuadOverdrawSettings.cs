using System;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class QuadOverdrawSettings
{
	public int MaxQuadCost = 10;

	public QuadOverdrawObjectFilter ObjectFilter;
}
