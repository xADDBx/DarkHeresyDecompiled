using System.Linq;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public struct PatternCellDataAccumulator
{
	private float ProbabilitiesSum { get; set; }

	private float[] InitialProbabilities { get; }

	private float DefenceProbability { get; set; }

	private float CoverProbability { get; set; }

	private int Lines { get; set; }

	private float InitialAverageProbability { get; set; }

	private bool AlwaysHit { get; }

	public PatternCellData Result => new PatternCellData(ProbabilitiesSum, InitialProbabilities, DefenceProbability, CoverProbability, Lines, InitialAverageProbability, AlwaysHit);

	public PatternCellDataAccumulator(float[] initialHitProbabilities, float defenceProbability, float coverProbability, float evasionProbability, bool mainCell)
	{
		AlwaysHit = false;
		InitialProbabilities = initialHitProbabilities;
		float num2 = (InitialAverageProbability = initialHitProbabilities.Average());
		ProbabilitiesSum = num2 * (1f - defenceProbability) * (1f - coverProbability);
		DefenceProbability = defenceProbability;
		CoverProbability = coverProbability;
		Lines = 1;
	}
}
