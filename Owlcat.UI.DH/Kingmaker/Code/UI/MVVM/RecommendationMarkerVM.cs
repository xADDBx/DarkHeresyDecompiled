using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class RecommendationMarkerVM : ViewModel
{
	public RecommendationType Recommendation { get; }

	public RecommendationMarkerVM(RecommendationType recommendation)
	{
		Recommendation = recommendation;
	}

	public RecommendationMarkerVM(int recommendation)
	{
		Recommendation = (RecommendationType)recommendation;
	}

	public RecommendationMarkerVM(bool recommend)
	{
		Recommendation = (recommend ? RecommendationType.Recommended : RecommendationType.Neutral);
	}
}
