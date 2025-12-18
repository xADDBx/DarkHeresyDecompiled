using System;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class FeatureFiltersIcons
{
	public Sprite NoneIcon;

	public Sprite RecommendedFilterIcon;

	public Sprite FavoritesFilterIcon;

	public Sprite OffenseFilterIcon;

	public Sprite DefenseFilterIcon;

	public Sprite SupportFilterIcon;

	public Sprite UniversalFilterIcon;

	public Sprite ArchetypeFilterIcon;

	public Sprite OriginFilterIcon;

	public Sprite WarpFilterIcon;

	public Sprite GetIconFor(FeatureFilterType filter)
	{
		return filter switch
		{
			FeatureFilterType.None => NoneIcon, 
			FeatureFilterType.RecommendedFilter => RecommendedFilterIcon, 
			FeatureFilterType.FavoritesFilter => FavoritesFilterIcon, 
			FeatureFilterType.OffenseFilter => OffenseFilterIcon, 
			FeatureFilterType.DefenseFilter => DefenseFilterIcon, 
			FeatureFilterType.SupportFilter => SupportFilterIcon, 
			FeatureFilterType.UniversalFilter => UniversalFilterIcon, 
			FeatureFilterType.ArchetypeFilter => ArchetypeFilterIcon, 
			FeatureFilterType.OriginFilter => OriginFilterIcon, 
			FeatureFilterType.WarpFilter => WarpFilterIcon, 
			_ => throw new ArgumentOutOfRangeException("filter", filter, null), 
		};
	}
}
