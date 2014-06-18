/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Carrier")]

public class APCarrier : MonoBehaviour
{ 
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL

	// Return last computed velocity, for now this must be called in your LateUpdate after this script execution
	public Vector2 GetVelocity() 
	{
		return m_velocity;
	}

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	void Start ()
	{
		m_prevPos = transform.position;
		m_velocity = Vector2.zero;
	}

	void LateUpdate ()
	{
		// NB : animation is updated between Update & LateUpdate call
		m_velocity = ((Vector2)transform.position - m_prevPos) / Time.deltaTime;
		m_prevPos = transform.position;
	}

	Vector2 m_prevPos;
	Vector2 m_velocity;
}

