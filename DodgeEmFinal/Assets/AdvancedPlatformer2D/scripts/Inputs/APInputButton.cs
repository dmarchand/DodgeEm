/* Copyright (c) 2014 Advanced Platformer 2D */

// Uncomment to enable EasyTouch support (you must have EasyTouch project in your Solution)
//#define APEASYTOUCHSUPPORT

using UnityEngine;

[System.Serializable]
public class APInputButton
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL	
	public string m_name;						// name of button in project input settings
	public APInputButtonPlugin m_plugin;		// use custom plugin instead of classic Unity input


	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL

	public APInputButton(string sName) 
	{
		m_name = sName; 
	}

	public bool IsSpecified()
	{
		return !string.IsNullOrEmpty(m_name) || (m_plugin != null);
	}

	public bool GetButton()
	{
		if(m_plugin != null)
		{
			return m_plugin.GetButton();
		}
		else
		{
			return Input.GetButton(m_name);
		}
	}

	public bool GetButtonDown()
	{
		if(m_plugin != null)
		{
			return m_plugin.GetButtonDown();
		}
		else
		{
			return Input.GetButtonDown(m_name);
		}
	}

	public bool GetButtonUp()
	{
		if(m_plugin != null)
		{
			return m_plugin.GetButtonUp();
		}
		else
		{
			return Input.GetButtonUp(m_name);
		}
	}

}

