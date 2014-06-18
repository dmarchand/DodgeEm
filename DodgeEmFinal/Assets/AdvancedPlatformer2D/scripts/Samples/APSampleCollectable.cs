/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleCollectable")]

// Sample collectibles
public class APSampleCollectable : APHitable 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_lifePoints = 0f;   						// amount of life point given by this collectable
	public int m_ammoPoints = 0;							// amount of ammo for specified ranged attack
	public int m_rangedAttackIndex = 1;						// index of ranged attack to fill with ammo (from 1 to n)
	public int m_scorePoints = 0;							// amount of score points given by this collectable
	public bool m_bulletCanCatch = false;					// tells if a bullet can catch this collectible
	public bool m_meleeAttackCanCatch = true;				// this collectible can be catched with melee attack hit

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	bool m_catched;

	void Start () 
	{
		m_catched = false;
	}	


	// called when we have been hit by a melee attack
	override public bool OnMeleeAttackHit(APCharacterController character, APHitZone hitZone)
	{
		if(m_meleeAttackCanCatch)
		{
			HandleCatch(character);
		}

		// always ignore hit for now
		return false;
	}

	// called when we have been hit by a bullet
	override public bool OnBulletHit(APCharacterController character, APBullet bullet) 
	{
		if(m_bulletCanCatch)
		{
			HandleCatch(character);
			return true; // destroy bullet
		}
		else
		{
			return false; // keep bullet alive
		}
	}

	// called when character is entering this collectable
	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
		APCharacterController character = otherCollider.GetComponent<APCharacterController>();
		if(character)
		{
			HandleCatch(character);
		}
	}
	
	// Catch this collectible
	void HandleCatch(APCharacterController character)
	{
		if(!m_catched)
		{
			m_catched = true;

			// update player data
			APSamplePlayer player = character.GetComponent<APSamplePlayer>();
			if(player != null)
			{
				player.m_life += m_lifePoints;
				player.m_score += m_scorePoints;
				if(character.m_rangedAttacks.m_attacks.Length >= m_rangedAttackIndex)
				{
					character.m_rangedAttacks.m_attacks[m_rangedAttackIndex].m_ammo += m_ammoPoints;
				}

				// need to refresh GUI here
				player.RefreshGUI();
			}

			// destroy me
			Object.Destroy(gameObject);
		}
	}
}
