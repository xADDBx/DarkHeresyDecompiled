using System.Collections.Generic;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Feedback;

public class FeedbackConsumptionTracker
{
	private readonly Queue<float> m_ConsumptionMemory;

	private readonly float m_MipBiasIncrement;

	private readonly float m_MipBiasDecrement;

	private readonly float m_OversubscribedThreshold;

	private readonly float m_UndersubscribedThreshold;

	private float m_MipBias;

	public float MipBias
	{
		get
		{
			return m_MipBias;
		}
		set
		{
			m_MipBias = math.clamp(value, 0f, 10f);
		}
	}

	public FeedbackConsumptionTracker(FeedbackMipBiasSettings settings)
	{
		m_ConsumptionMemory = new Queue<float>(settings.MemoryLength);
		for (int i = 0; i < settings.MemoryLength; i++)
		{
			m_ConsumptionMemory.Enqueue(0f);
		}
		m_MipBiasIncrement = settings.BiasIncrement;
		m_MipBiasDecrement = settings.BiasDecrement;
		m_UndersubscribedThreshold = settings.UndersubscribedThreshold;
		m_OversubscribedThreshold = settings.OversubscribedThreshold;
	}

	public void Update(float consumption)
	{
		m_ConsumptionMemory.Dequeue();
		m_ConsumptionMemory.Enqueue(consumption);
		float num = FindMaxOfConsumptionMemory();
		if (num >= m_OversubscribedThreshold)
		{
			MipBias += m_MipBiasIncrement;
		}
		else if (num <= m_UndersubscribedThreshold)
		{
			MipBias -= m_MipBiasDecrement;
		}
	}

	public float FindMaxOfConsumptionMemory()
	{
		float num = 0f;
		foreach (float item in m_ConsumptionMemory)
		{
			num = math.max(num, item);
		}
		return num;
	}
}
