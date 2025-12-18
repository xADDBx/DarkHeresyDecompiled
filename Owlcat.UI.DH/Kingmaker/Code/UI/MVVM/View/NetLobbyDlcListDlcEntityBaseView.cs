using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyDlcListDlcEntityBaseView : View<NetLobbyDlcListDlcEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_DlcName;

	[SerializeField]
	private List<Image> m_PlayersContainsDlcsList;

	[SerializeField]
	private Sprite m_ContainsDlcImage;

	[SerializeField]
	private Sprite m_HasNoDlcImage;

	[SerializeField]
	private Sprite m_SharingDlcImage;

	protected override void OnBind()
	{
		m_DlcName.text = base.ViewModel.DlcName;
		for (int i = 0; i < m_PlayersContainsDlcsList.Count; i++)
		{
			bool flag = i < base.ViewModel.PlayersHasDlcType.Count;
			m_PlayersContainsDlcsList[i].transform.parent.gameObject.SetActive(flag);
			if (flag)
			{
				NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum type = base.ViewModel.PlayersHasDlcType[i];
				m_PlayersContainsDlcsList[i].sprite = GetDlcContainsSprite(type);
				m_PlayersContainsDlcsList[i].SetHint(GetDlcContainsHint(type));
			}
		}
	}

	private Sprite GetDlcContainsSprite(NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum type)
	{
		return type switch
		{
			NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum.Contains => m_ContainsDlcImage, 
			NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum.Sharing => m_SharingDlcImage, 
			NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum.HasNo => m_HasNoDlcImage, 
			_ => null, 
		};
	}

	private string GetDlcContainsHint(NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum type)
	{
		return type switch
		{
			NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum.Contains => UIStrings.Instance.Overtips.HasResourceCount, 
			NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum.Sharing => UIStrings.Instance.NetLobbyTexts.DlcSharedByHost, 
			NetLobbyDlcListDlcEntityVM.DlcContainsTypeEnum.HasNo => LocalizedTexts.Instance.Reasons.UnavailableGeneric, 
			_ => null, 
		};
	}
}
