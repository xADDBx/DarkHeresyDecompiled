using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ItemSlotConsoleView : ItemSlotBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	public Observable<Unit> OnConfirmClickAsObservable => m_MainButton.OnConfirmClickAsObservable();

	public Observable<Unit> OnLongConfirmClickAsObservable => m_MainButton.OnLongConfirmClickAsObservable();

	public Observable<Unit> OnFunc01ClickAsObservable => m_MainButton.OnFunc01ClickAsObservable();

	public Observable<Unit> OnLongFunc01ClickAsObservable => m_MainButton.OnLongFunc01ClickAsObservable();

	public Observable<Unit> OnFunc02ClickAsObservable => m_MainButton.OnFunc02ClickAsObservable();

	public Observable<Unit> OnLongFunc02ClickAsObservable => m_MainButton.OnLongFunc02ClickAsObservable();

	protected override void OnBind()
	{
		base.OnBind();
		SubscribeInteractions();
	}

	private void SubscribeInteractions()
	{
	}

	public void SetSelected(bool value)
	{
		if (!(m_MainButton == null))
		{
			m_MainButton.SetFocus(value);
		}
	}

	public void SetWaitingForSlotState(bool state)
	{
		if (!(m_MainButton == null))
		{
			if (state)
			{
				m_MainButton.SetActiveLayer("WaitingForSlot");
			}
			else
			{
				m_MainButton.SetActiveLayer((base.ViewModel.ItemEntity == null) ? "Empty" : "Busy");
			}
		}
	}
}
