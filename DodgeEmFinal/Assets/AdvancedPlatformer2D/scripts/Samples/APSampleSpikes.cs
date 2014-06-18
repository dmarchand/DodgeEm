/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleSpikes")]

// Sample for Spikes object
public class APSampleSpikes : APHitable 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_touchDamage = 1f;						// damage done when touching player

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	float m_minTimeBetweenTwoReceivedHits = 0.1f;
	float m_hitPenetrationTolerance = 0f;
	float m_lastHitTime;

	void Start () 
	{
		m_lastHitTime = 0f;
	}

	// called when character motor ray is touching us
	override public bool OnCharacterTouch(APCharacterController character, APCharacterMotor.RayType rayType, RaycastHit2D hit, float penetration)
	{
		// Make sure character is close enough
		if(penetration >= m_hitPenetrationTolerance)
			return false;

		// prevent checking hits too often
		if(Time.time < m_lastHitTime + m_minTimeBetweenTwoReceivedHits)
			return false;
		
		// save current hit time
		m_lastHitTime = Time.time;

		// add hit to character
		APSamplePlayer samplePlayer = character.GetComponent<APSamplePlayer>();
		samplePlayer.OnHit(m_touchDamage, transform.position);

		// always ignore contact
		return false;
	}	
}
