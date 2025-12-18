namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public readonly struct PatternCellData
{
	public static readonly PatternCellData Empty = new PatternCellData(1f, new float[1] { 1f }, 0f, 0f, 1, 1f, alwaysHit: true);

	public float ProbabilitiesSum { get; }

	public float[] InitialProbabilities { get; }

	public float DefenceProbability { get; }

	public float CoverProbability { get; }

	public int Lines { get; }

	public float InitialAverageProbability { get; }

	public bool AlwaysHit { get; }

	public PatternCellData(float probabilitiesSum, float[] initialProbabilities, float defenceProbability, float coverProbability, int lines, float initialAverageProbability, bool alwaysHit)
	{
		this = default(PatternCellData);
		ProbabilitiesSum = probabilitiesSum;
		InitialProbabilities = initialProbabilities;
		DefenceProbability = defenceProbability;
		CoverProbability = coverProbability;
		Lines = lines;
		InitialAverageProbability = initialAverageProbability;
		AlwaysHit = alwaysHit;
	}
}
