using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.ElementsSystem.ContextData;

namespace Owlcat.AI.Mechanics.BodyParts;

public class AiBodyPartsContextData : ContextData<AiBodyPartsContextData>
{
	private BlueprintBodyPart m_BodyPart;

	public static BlueprintBodyPart CurrentBodyPart => ContextData<AiBodyPartsContextData>.Current?.m_BodyPart;

	public AiBodyPartsContextData Setup(BlueprintBodyPart bodyPart)
	{
		m_BodyPart = bodyPart;
		return this;
	}

	protected override void Reset()
	{
	}
}
