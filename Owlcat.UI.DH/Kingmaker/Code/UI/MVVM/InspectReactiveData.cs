using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InspectReactiveData
{
	public readonly ReactiveProperty<string> WoundsValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> DurabilityValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> DefenceValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> DamageReductionValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> MovementPointsValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<(int min, int max, int current)> MoraleValue = new ReactiveProperty<(int, int, int)>();

	public readonly ObservableList<ITooltipBrick> TooltipBrickBuffs = new ObservableList<ITooltipBrick>();
}
