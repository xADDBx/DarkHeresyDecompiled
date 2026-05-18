using Owlcat.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationView : View<FactionReputationVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_FactionsWidgetList;

	[Header("Screen")]
	[SerializeField]
	private UIServiceWindowPostProcessView m_PostProcessView;

	[Header("Views")]
	[FormerlySerializedAs("factionWidgetPrefab")]
	[SerializeField]
	private FactionReputationWidgetView m_FactionWidgetPrefab;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_PostProcessView.Initialize();
	}

	protected override void OnBind()
	{
		m_FactionsWidgetList.DrawEntries(base.ViewModel.FactionWidgetVMs, m_FactionWidgetPrefab);
		base.gameObject.SetActive(value: true);
		m_PostProcessView.ShowFrom(RootVM.Instance.ServiceWindowsContext.HasPrevWindow ? UIPostEffectState.Default : UIPostEffectState.Off);
	}

	protected override void OnUnbind()
	{
		m_FactionsWidgetList.Clear();
		m_PostProcessView.Hide(base.ViewModel.SwitchedFromServiceWindow.CurrentValue, delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
