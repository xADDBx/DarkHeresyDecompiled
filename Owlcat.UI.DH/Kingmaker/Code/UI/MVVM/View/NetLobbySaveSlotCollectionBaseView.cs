namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbySaveSlotCollectionBaseView : SaveSlotCollectionVirtualBaseView
{
	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		base.gameObject.SetActive(value: false);
	}
}
