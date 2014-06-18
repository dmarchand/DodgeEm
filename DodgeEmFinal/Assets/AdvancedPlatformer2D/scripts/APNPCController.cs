/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(APCharacterMotor))]
[AddComponentMenu("Advanced Platformer 2D/CharacterController")]

public class APNPCController : APCharacterController
{
	protected override void HandleInputFilter()
	{
		m_inputs.m_axisX.SetForcedValue(true, -1f);
	}
}