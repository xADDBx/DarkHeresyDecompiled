using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EndOfGameView : View<EndOfGameVM>
{
	[SerializeField]
	private EndOfGameObject m_EndOfGameObject;

	protected override void OnBind()
	{
		m_EndOfGameObject.Show();
	}

	protected override void OnUnbind()
	{
		m_EndOfGameObject.Hide();
	}
}
