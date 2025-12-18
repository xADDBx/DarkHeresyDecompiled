using R3;
using R3.Triggers;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeaturesPCView : CharInfoFeaturesBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		m_ActiveAbilities.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(state: true);
		}).AddTo(this);
		m_PassiveAbilities.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(state: false);
		}).AddTo(this);
	}
}
