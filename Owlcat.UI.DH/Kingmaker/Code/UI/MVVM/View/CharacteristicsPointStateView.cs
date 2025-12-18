using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacteristicsPointStateView : MonoBehaviour
{
	public enum CharacteristicsPointState
	{
		Available,
		NotAvailable,
		JustSpent
	}

	[SerializeField]
	private Image m_StateImage;

	private readonly Color AVAILABLE_COLOR = new Color(0.16f, 0.216f, 0.192f, 0.5f);

	private readonly Color JUST_SPENT_COLOR = new Color(0.796f, 0.631f, 0.435f);

	private readonly Color NOT_AVAILABLE_COLOR = new Color(0.663f, 0.808f, 0.706f);

	public void SetState(CharacteristicsPointState state)
	{
		Image stateImage = m_StateImage;
		stateImage.color = state switch
		{
			CharacteristicsPointState.Available => AVAILABLE_COLOR, 
			CharacteristicsPointState.NotAvailable => NOT_AVAILABLE_COLOR, 
			CharacteristicsPointState.JustSpent => JUST_SPENT_COLOR, 
			_ => m_StateImage.color, 
		};
	}
}
