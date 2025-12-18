using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntryFeatureGroupVM : BaseFeatureGroupVM<BaseRankEntryFeatureVM>
{
	public BlueprintFeature Owner { get; }

	public RankEntryFeatureGroupVM([NotNull] List<BaseRankEntryFeatureVM> featuresListGroup, BlueprintFeature owner = null)
		: base(featuresListGroup, (string)null, (string)null)
	{
		Owner = owner;
	}

	public void UpdateState(LevelUpManager levelUpManager)
	{
		FeatureList.ForEach(delegate(BaseRankEntryFeatureVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
	}

	public void UpdateReadOnlyState()
	{
		FeatureList.ForEach(delegate(BaseRankEntryFeatureVM vm)
		{
			(vm as RankEntrySelectionFeatureVM)?.UpdateStateForReadOnly();
		});
	}

	public virtual List<VirtualListElementVMBase> GetAll()
	{
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		if (FeatureList.Empty())
		{
			return list;
		}
		List<BaseRankEntryFeatureVM> collection = FeatureList.ToList();
		list.AddRange(collection);
		return list;
	}

	public List<VirtualListElementVMBase> GetFiltered(FeatureFilterType? filter)
	{
		if (!filter.HasValue || filter.GetValueOrDefault() == FeatureFilterType.None)
		{
			return GetAll();
		}
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		List<BaseRankEntryFeatureVM> list2 = FeatureList.Where((BaseRankEntryFeatureVM f) => f.Feature.MeetsFilter(filter.Value)).ToList();
		switch (filter.Value)
		{
		case FeatureFilterType.RecommendedFilter:
			list2.RemoveAll((BaseRankEntryFeatureVM f) => !f.IsRecommended);
			break;
		case FeatureFilterType.FavoritesFilter:
			list2.RemoveAll((BaseRankEntryFeatureVM f) => !f.IsFavorite);
			break;
		}
		list.AddRange(list2);
		return list;
	}
}
