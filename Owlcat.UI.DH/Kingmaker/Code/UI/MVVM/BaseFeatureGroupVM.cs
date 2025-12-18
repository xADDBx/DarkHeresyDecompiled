using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BaseFeatureGroupVM<TFeatureVM> : ViewModel where TFeatureVM : CharInfoFeatureVM
{
	public readonly List<TFeatureVM> FeatureList;

	public readonly string Label;

	public readonly string TooltipKey;

	public bool IsEmpty
	{
		get
		{
			List<TFeatureVM> featureList = FeatureList;
			if (featureList == null)
			{
				return true;
			}
			return !featureList.Any();
		}
	}

	public BaseFeatureGroupVM([NotNull] List<TFeatureVM> featuresListGroup, string label = null, string tooltipKey = null)
	{
		FeatureList = featuresListGroup;
		Label = label;
		TooltipKey = tooltipKey;
	}

	protected override void OnDispose()
	{
		FeatureList?.ForEach(delegate(TFeatureVM f)
		{
			f.Dispose();
		});
		FeatureList?.Clear();
	}
}
