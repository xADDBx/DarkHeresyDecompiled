using Kingmaker.Code.Globalmap.Colonization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ProfitFactorModifierVM : ViewModel
{
	public readonly ProfitFactorModifierType Type;

	public readonly bool IsNegative;

	private readonly ReactiveProperty<float> m_ModifierValue = new ReactiveProperty<float>(0f);

	private readonly ProfitFactorModifier m_Modifier;

	public ReadOnlyReactiveProperty<float> ModifierValue => m_ModifierValue;

	public ProfitFactorModifier Modifier => m_Modifier;

	public ProfitFactorModifierVM(ProfitFactorModifier modifier)
	{
		m_Modifier = modifier;
		Type = modifier.ModifierType;
		IsNegative = modifier.IsNegative;
		m_ModifierValue.Value = modifier.Value;
	}
}
