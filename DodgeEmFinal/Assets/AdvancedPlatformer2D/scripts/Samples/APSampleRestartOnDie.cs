/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleRestartOnDie")]

// Sample for falling platform
public class APSampleRestartOnDie : MonoBehaviour
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_minHeight = -100f;   					// height at which player is considered dead

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	void Update()
	{
		if(transform.position.y <= m_minHeight)
		{
			// reset level
			Application.LoadLevel(Application.loadedLevel);
		}
	}
}
