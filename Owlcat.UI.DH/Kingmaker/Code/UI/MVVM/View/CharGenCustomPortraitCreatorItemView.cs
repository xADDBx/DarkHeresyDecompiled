using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCustomPortraitCreatorItemView : View<CharGenPortraitSelectorItemVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private OwlcatButton m_Button;

	public MonoBehaviour MonoBehaviour => this;

	public IConsoleEntity ConsoleEntityProxy => m_Button;

	protected override void OnBind()
	{
		m_Button.ConfirmClickHint = UIStrings.Instance.CharGen.AddPortrait;
		bool flag = UtilityNet.IsControlMainCharacter();
		m_Button.SetInteractable(flag);
		if (flag)
		{
			ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.OnCustomPortraitCreate();
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_Button.OnConfirmClickAsObservable(), delegate
			{
				base.ViewModel.OnCustomPortraitCreate();
			}).AddTo(this);
		}
	}
}
