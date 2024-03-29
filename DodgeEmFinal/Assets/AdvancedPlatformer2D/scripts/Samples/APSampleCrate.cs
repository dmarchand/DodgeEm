/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleCrate")]

// Sample for specific explodable Crate behavior
// first hit = damage
// second hit = explode
public class APSampleCrate : APHitable 
{
	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	int m_hitCount;
	bool m_shouldDisable;
	Animator m_anim;

	void Start () 
	{
		// init start position
		m_hitCount = 2;
		m_anim = GetComponent<Animator>();
		m_shouldDisable = false;
	}
	
	void LateUpdate()
	{
		if(m_shouldDisable)
		{
			gameObject.SetActive(false);
		}
	}

	// return true if object is dead
	bool IsDead() 
	{
		return m_hitCount <= 0;
	}
	
	// called when we have been hit by a melee attack
	override public bool OnMeleeAttackHit(APCharacterController character, APHitZone hitZone)
	{
		return HandleHit();
	}

	// called when we have been hit by a bullet
	override public bool OnBulletHit(APCharacterController launcher, APBullet bullet) 
	{
		return HandleHit();
	}

	// call to handle one hit
	bool HandleHit()
	{
		// do nothing if already dead
		if (IsDead())
			return false;

		// reduce hit count
		m_hitCount -= 1;
		
		// handle death & callbacks
		if (m_hitCount <= 0)
		{
			m_hitCount = 0;
			
			// launch die animation
			if(m_anim)
			{
				m_anim.Play("explode", 0, 0f);
			}
		}
		else
		{
			// launch hit animation
			if(m_anim)
			{
				m_anim.Play("hit", 0, 0f);
			}
		}

		return true;
	}
	
	// Disable object at next frame
	void Disable()
	{
		m_shouldDisable = true;
	}
}
