/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleGUI")]

// Sample GUI handler
public class APSampleGUI : APGUI 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public bool m_showLife = true;
	public bool m_showAmmo = true;
	public bool m_showScore = true;

	public GUIStyle m_text;
	public GUIStyle m_icon;
	public Texture m_textureLife;
	public Texture m_textureAmmo;
	public Texture m_textureScore;

	public void SetAmmoCount(int ammo) { m_ammoCount = ammo; }
	public void SetLife(int life) { m_lifeCount = life; }
	public void SetScore(int score) { m_scoreCount = score; }

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	int m_ammoCount = 0;
	int m_lifeCount = 0;
	int m_scoreCount = 0;

	// Drawing
	void OnGUI () 
	{
		GUILayout.BeginHorizontal();

		if(m_showLife)
		{
			GUILayout.Box ( m_textureLife, m_icon );
			GUILayout.Label ( m_lifeCount.ToString(), m_text);
		}

		if(m_showAmmo)
		{
			GUILayout.Box ( m_textureAmmo, m_icon );
			GUILayout.Label ( m_ammoCount.ToString(), m_text);
		}

		if(m_showScore)
		{
			GUILayout.Box ( m_textureScore, m_icon );
			GUILayout.Label ( m_scoreCount.ToString(), m_text);
		}

		GUILayout.EndHorizontal();
	}

	// Refresh GUI
	public override void OnBulletLaunched(APCharacterController launcher, APRangedAttack attack)
	{
		m_ammoCount = attack.m_ammo;
	}
}
