using System.Collections.Generic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntryEmptyFeaturesGroupVM : RankEntryFeatureGroupVM
{
	private readonly string m_Description;

	public RankEntryEmptyFeaturesGroupVM(string description)
		: base(new List<BaseRankEntryFeatureVM>())
	{
		m_Description = description;
	}

	public override List<VirtualListElementVMBase> GetAll()
	{
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		if (string.IsNullOrEmpty(m_Description))
		{
			return list;
		}
		RankEntryDescriptionVM item = new RankEntryDescriptionVM(m_Description).AddTo(this);
		list.Add(item);
		return list;
	}
}
