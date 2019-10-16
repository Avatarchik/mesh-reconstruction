﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePointCloudRenderer : PointCloudRenderer
{
	[Range(0.001f, 0.3f)]
	[SerializeField]
	float size = 0.05f;

	override protected void Update()
	{
		vertexBufferGenerator.SetFloat("size", size);

		base.Update();
	}

	override protected int ComputeVertexBufferMaxSize()
	{
		return width * height * 6;
	}
}