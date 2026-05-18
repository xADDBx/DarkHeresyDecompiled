using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPortraitTabVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<bool> m_IsMainCharacter = new ReactiveProperty<bool>();

	public readonly CharGenPortraitTab Tab;

	public ReadOnlyReactiveProperty<bool> IsMainCharacter => m_IsMainCharacter;

	public CharGenPortraitTabVM(CharGenPortraitTab tab)
		: base(allowSwitchOff: false)
	{
		Tab = tab;
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DoSelectMe()
	{
	}
}
