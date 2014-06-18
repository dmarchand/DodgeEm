/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Camera")]

public class APCamera : MonoBehaviour 
{ 
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_marginX = 1f;				// Max distance between camera and player on X axis
	public float m_marginY = 2f;				// Max distance between camera and player on Y axis
	public Vector2 m_offset = Vector2.zero;		// static offset
	public float m_faceLead = 0f;				// face leading distance

	// speed curve in fonction of distance
	public AnimationCurve m_speed = new AnimationCurve(new Keyframe(0, 15), new Keyframe(15, 50));

	public bool m_clampEnabled = false;			// Enable position clamping
	public Vector2 m_minPos = Vector2.zero;		// Camera min position
	public Vector2 m_maxPos = Vector2.zero;		// Camera max position

	public Transform player;								// Reference to the player's transform.
	public APParallaxScrolling[] m_parallaxScrollings;		// list of object to handle for parallax scrolling

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	APCharacterController m_character;
	float m_dynOffset;

	void Awake ()
	{
		m_dynOffset = 0f;
		m_character = player.GetComponent<APCharacterController>();
		for(int i = 0; i < m_parallaxScrollings.Length; i++)
		{
			m_parallaxScrollings[i].Init(this);
		}
	}

	void FixedUpdate()
	{
		if(APSettings.m_fixedUpdate)
		{
			UpdateCamera();
		}
	}

	void LateUpdate ()
	{
		if(!APSettings.m_fixedUpdate)
		{
			UpdateCamera();
		}
		else
		{
			// add carrier velocity if needed
			if(m_character && m_character.IsCarried())
			{
				Vector2 offset = m_character.GetCarrierOffset();
				SetCameraPosition(transform.position.x + offset.x, transform.position.y + offset.y);
			}
		}
	}

	void UpdateCamera ()
	{
		float camX = transform.position.x;
		float camY = transform.position.y;
		float fOffsetX = 0f;
		float fOffsetY = 0f;
		Vector2 v2Diff = player.position - transform.position;
		v2Diff += m_offset;
		v2Diff.x += m_dynOffset;


		/*
		Vector3 camPosOffset = transform.position - new Vector3(m_offset.x + m_dynOffset, m_offset.y, 0f);
		Debug.DrawLine(camPosOffset + new Vector3(-m_marginX, -m_marginY, 0f), 
		               camPosOffset + new Vector3(-m_marginX, m_marginY, 0f));

		Debug.DrawLine(camPosOffset + new Vector3(-m_marginX, m_marginY, 0f), 
		               camPosOffset + new Vector3(m_marginX, m_marginY, 0f));

		Debug.DrawLine(camPosOffset + new Vector3(m_marginX, m_marginY, 0f), 
		               camPosOffset + new Vector3(m_marginX, -m_marginY, 0f));

		Debug.DrawLine(camPosOffset + new Vector3(m_marginX, -m_marginY, 0f), 
		               camPosOffset + new Vector3(-m_marginX, -m_marginY, 0f));
		*/

		// handle x margin
		if(v2Diff.x > m_marginX)
		{
			fOffsetX = (v2Diff.x - m_marginX);

			if(m_character && (m_character.IsControlled || m_character.IsFacingRight()))
			{
				m_dynOffset = m_faceLead;
			}
		}
		else if(v2Diff.x < -m_marginX)
		{
			fOffsetX = (v2Diff.x + m_marginX);

			if(m_character && (m_character.IsControlled || !m_character.IsFacingRight()))
			{
				m_dynOffset = -m_faceLead;
			}
		}

		// handle y margin
		if(v2Diff.y > m_marginY)
		{
			fOffsetY = (v2Diff.y - m_marginY);
		}
		else if(v2Diff.y < -m_marginY)
		{
			fOffsetY = (v2Diff.y + m_marginY);
		}

		// update dynamic
		float fAbsOffsetX = Mathf.Abs(fOffsetX);
		float fSpeedX = m_speed.Evaluate(fAbsOffsetX);
		camX = Move(camX, camX + fOffsetX, fSpeedX);

		float fAbsOffsetY = Mathf.Abs(fOffsetY);
		float fSpeedY = m_speed.Evaluate(fAbsOffsetY);
		camY = Move(camY, camY + fOffsetY, fSpeedY);

		// clamp if needed
		if(m_clampEnabled)
		{
			camX = Mathf.Clamp(camX, m_minPos.x, m_maxPos.x);
			camY = Mathf.Clamp(camY, m_minPos.y, m_maxPos.y);
		}

		// Final update
		SetCameraPosition(camX, camY);
	}

	void SetCameraPosition(float fPosX, float fPosY)
	{
		transform.position = new Vector3(fPosX, fPosY, transform.position.z);
		
		// Update parallax scrollings
		for(int i = 0; i < m_parallaxScrollings.Length; i++)
		{
			m_parallaxScrollings[i].Update();
		}
	}

	float Move(float fFrom, float fTo, float fSpeed)
	{
		float fDiff = fTo - fFrom;
		float fOffset = fSpeed * Time.deltaTime;
		if(fOffset > Mathf.Abs(fDiff))
		{
			fOffset = fDiff;
		}
		else
		{
			fOffset *= Mathf.Sign(fDiff);
		}

		return fFrom + fOffset;
	}
}
