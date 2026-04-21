using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;


public partial class PlayerController
{
	[Header("Camera")]
	[SerializeField]
	CinemachineThirdPersonFollow CameraFollow;

	[SerializeField]
	float CameraAimSmoothness = 5f;

	[Header("Camera Regular Mode")]
	[SerializeField]
	private Vector3 CamShoulderOffset = Vector3.zero;

	[SerializeField]
	private float CamVerticalArmLength = 0.5f;

	[SerializeField]
	private float CamDistance = 3.5f;



	[Header("Camera Aiming Mode")]
	[SerializeField]
	private float CamAimVerticalArmLength = 0.4f;

	[SerializeField]
	private float CamAimDistance = 1.3f;

	[SerializeField]
	private Vector3 CamAimShoulderOffset = new Vector3(0.75f, 0f, 0f);


	bool aiming = false;


	public bool Aiming
	{
		get
		{
			return aiming;
		}
	}


	void UpdateAiming()
	{
		if (aiming)
		{
			npc.LookAt(CameraFollow.transform.position + CameraFollow.transform.forward * 30f);
		}

		Vector3 targetShoulderOffset = aiming ? CamAimShoulderOffset : CamShoulderOffset;
		float targetCamDistance = aiming ? CamAimDistance : CamDistance;
		float targetVerticalArmLength = aiming ? CamAimVerticalArmLength : CamVerticalArmLength;

		float t = CameraAimSmoothness * Time.deltaTime;

		if (Mathf.Abs(CameraFollow.CameraDistance - targetCamDistance) <= 0.05f)
		{
			CameraFollow.ShoulderOffset = targetShoulderOffset;
			CameraFollow.CameraDistance = targetCamDistance;
			CameraFollow.VerticalArmLength = targetVerticalArmLength;
		}
		else
		{
			CameraFollow.ShoulderOffset = Vector3.Lerp(CameraFollow.ShoulderOffset, targetShoulderOffset, t);
			CameraFollow.CameraDistance = Mathf.Lerp(CameraFollow.CameraDistance, targetCamDistance, t);
			CameraFollow.VerticalArmLength = Mathf.Lerp(CameraFollow.VerticalArmLength, targetVerticalArmLength, t);
		}

	}

}
