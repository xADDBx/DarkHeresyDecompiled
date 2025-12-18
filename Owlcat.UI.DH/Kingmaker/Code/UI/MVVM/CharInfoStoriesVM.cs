using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStoriesVM : CharInfoComponentVM
{
	public readonly AutoDisposingList<CharInfoCompanionStoryVM> Stories = new AutoDisposingList<CharInfoCompanionStoryVM>();

	public CharInfoStoriesVM(ReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		Stories.Clear();
		foreach (BlueprintCompanionStory item2 in Game.Instance.Player.CompanionStories.Get(Unit.CurrentValue).ToList())
		{
			CharInfoCompanionStoryVM item = new CharInfoCompanionStoryVM(item2).AddTo(this);
			Stories.Add(item);
		}
	}
}
