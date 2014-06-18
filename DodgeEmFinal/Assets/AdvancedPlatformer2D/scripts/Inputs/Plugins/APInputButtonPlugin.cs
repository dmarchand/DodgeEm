/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public abstract class APInputButtonPlugin : MonoBehaviour
{
	abstract public bool GetButton();
	abstract public bool GetButtonDown();
	abstract public bool GetButtonUp();
}

