/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(APCharacterController))]
[AddComponentMenu("Advanced Platformer 2D/Samples/APSamplePlayer")]

// Sample for handling Player behavior when being hit + simple life system
public class APSamplePlayer : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_life = 10f;   							// total life of player
	public int m_score = 0;   								// initial score of player
	public float m_jumpDamage = 1f;							// ammount of damage done when jumping on Blob
	public float m_godModeTimeWhenHit = 3f;					// time of god mode (i.e invincible mode when beeing hit)
	public LayerMask m_godModeLayerMask;					// collision layer while in god mode
	public Vector2 m_hitImpulse = new Vector2(8f, 12f);		// impulse when beeing hit by Blob
	public float m_waitTimeAfterDie = 3f;					// time to wait before reseting level after dying
	public string m_animDie;								// state of animation die

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	APCharacterController m_player;
	Animator m_anim;
	bool m_godMode;
	float m_godModeTime;
	LayerMask m_prevCollisionLayer;
	APSampleGUI m_gui;


	// Use this for initialization
	void Start () 
	{
		// some initializations variables
		m_player = GetComponent<APCharacterController>();
		m_anim = GetComponent<Animator>();

		m_godMode = false;
		m_godModeTime = 0f;

		// save ref to our sample GUI here
		if(m_player.m_GUI)
		{
			m_gui = m_player.m_GUI.GetComponent<APSampleGUI>();
		}

		// initialize GUI
		RefreshGUI();
	}

	// refresh GUI
	public void RefreshGUI()
	{
		if(m_gui)
		{
			if(m_player.m_rangedAttacks.m_attacks.Length > 0)
			{
				m_gui.SetAmmoCount(m_player.m_rangedAttacks.m_attacks[0].m_ammo);
			}
			
			m_gui.SetLife(Mathf.RoundToInt(m_life));
			m_gui.SetScore(m_score);
		}
	}

	void FixedUpdate () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		// handle god mode here
		if(m_godMode && Time.time > m_godModeTime + m_godModeTimeWhenHit)
		{
			if(m_anim)
			{
				m_anim.SetBool("GodMode", false);
			}
			m_godMode = false;

			// restore previous layer mask
			m_player.GetMotor().m_rayLayer = m_prevCollisionLayer;
		}
	}

	// return true if character is dead
	public bool IsDead() 
	{
		return m_life <= 0f;
	}
	
	// called when hit by NPC object
	public void OnHit(float fDamage, Vector3 hitPos)
	{
		// do nothing if already dead or no damage
		if (IsDead() || (fDamage <= 0f))
			return;

		// handle death & callbacks
		if (fDamage >= m_life)
		{
			// die !
			m_life = 0f;
			m_player.LeaveAnyState();

			// play animation if exists
			if(!string.IsNullOrEmpty(m_animDie))
			{
				m_player.PlayAnim(m_animDie);
			}

			// Request restart of level
			StartCoroutine("RestartLevel");
		}
		else
		{
			m_life -= fDamage;

			// hit!
			// enable god mode if not already done
			if(!m_godMode)
			{
				m_godMode = true;
				m_godModeTime = Time.time;
				if(m_anim)
				{
					m_anim.SetBool("GodMode", true);
				}
				
				// disable collisions with npcs for a while
				APCharacterMotor motor = m_player.GetMotor();
				m_prevCollisionLayer = motor.m_rayLayer;
				motor.m_rayLayer = m_godModeLayerMask;
				
				// add small impulse in opposite direction
				Vector2 v2Dir = motor.transform.position - hitPos;
				m_player.SetVelocity(new Vector2((v2Dir.x > 0f ? 1f : -1f) * m_hitImpulse.x, m_hitImpulse.y));
			}
		}

		// update GUI
		if(m_gui)
		{
			m_gui.SetLife(Mathf.RoundToInt(m_life));
		}
	}

	IEnumerator RestartLevel () 
	{
		yield return new WaitForSeconds (m_waitTimeAfterDie);
		Application.LoadLevel(Application.loadedLevel);
	}
}
