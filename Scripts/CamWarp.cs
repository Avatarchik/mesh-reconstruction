﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamWarp : MonoBehaviour
{
	protected struct TrackingData
	{
		public ulong timestamp;

		public Matrix4x4 m;

		public TrackingData(ulong _timestamp, Matrix4x4 _m)
		{
			timestamp = _timestamp;

			m = _m;
		}
	}

	/// <summary>
	/// Instance of the ZEDManager interface
	/// </summary>
	[SerializeField]
	protected ZEDManager zedManager = null;

	/// <summary>
	/// Instance of the OpenVRTracking
	/// </summary>
	[SerializeField]
	protected OpenVRTracking openVRTracking = null;

	[SerializeField]
	protected Vector3 hmdToZed = new Vector3(-0.0315f, 0, 0.115f);

	[SerializeField]
	[Range(0, 100)]
	[Tooltip("Estimated capture to RAM delay (ms)")]
	protected ulong communicationTime;

	protected Queue<TrackingData> headPoses = new Queue<TrackingData>();

	protected void OnEnable()
	{
		if(zedManager == null)
		{
			zedManager = FindObjectOfType<ZEDManager>();
			if(ZEDManager.GetInstances().Count > 1) //We chose a ZED arbitrarily, but there are multiple cams present. Warn the user. 
			{
				Debug.Log("Warning: " + gameObject.name + "'s zedManager was not specified, so the first available ZEDManager instance was " +
					"assigned. However, there are multiple ZEDManager's in the scene. It's recommended to specify which ZEDManager you want to " +
					"use to display a point cloud.");
			}
		}

		if(zedManager != null)
		{
			zedManager.OnGrab += OnGrab;
		}

		if(openVRTracking == null)
		{
			OpenVRTracking[] openVRTrackings = FindObjectsOfType<OpenVRTracking>();

			if(openVRTrackings.Length > 1)
			{
				Debug.Log("Warning: " + gameObject.name + "'s openVRTracking was not specified, so the first available OpenVRTracking instance was " +
					"assigned. However, there are multiple OpenVRTracking's in the scene. It's recommended to specify which OpenVRTracking you want to " +
					"use.");
			}

			openVRTracking = openVRTrackings[0];
		}

		if(openVRTracking != null)
		{
			openVRTracking.getHeadPose += OnNewHeadPose;
		}
	}

	protected void OnDisable()
	{
		if(zedManager != null)
		{
			zedManager.OnGrab -= OnGrab;
		}

		if(openVRTracking != null)
		{
			openVRTracking.getHeadPose -= OnNewHeadPose;
		}
	}

	protected void OnGrab()
	{
		ulong image_timestamp = zedManager.ImageTimeStamp;

		Matrix4x4 head_pose = GetHeadPose(image_timestamp - communicationTime * 1000000);

		Matrix4x4 offset = Matrix4x4.TRS(hmdToZed, Quaternion.identity, Vector3.one);

		Matrix4x4 world_matrix = head_pose * offset;

		transform.position = world_matrix.GetColumn(3);

		transform.rotation = world_matrix.rotation;
	}

	protected void OnNewHeadPose(ulong timestamp, Matrix4x4 m)
	{
		headPoses.Enqueue(new TrackingData(timestamp, m));

		// attention au thread ds lequel la callback est appelee
	}

	Matrix4x4 GetHeadPose(ulong timestamp)
	{
		TrackingData td = new TrackingData(0, Matrix4x4.identity);

		if(headPoses.Count > 0)
		{
			do
			{
				td = headPoses.Dequeue();
			}
			while(td.timestamp < timestamp && headPoses.Count > 0);
		}

		return td.m;
	}
}
