using Owlcat.UI;
using Owlcat.UI.Commands;
using Owlcat.UI.Navigation;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[RequireComponent(typeof(FocusLayer))]
public class VariativeInteractionConsoleView : VariativeInteractionView
{
	protected override void OnBind()
	{
		base.OnBind();
		this.AddNavigation().AddTo(this);
		this.AddCommand("gamepad:buttonEast;keyboard:esc", delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
	}
}
