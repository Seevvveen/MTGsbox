using System;
using System.Collections.Generic;
using System.Text;
using Sandbox;

public static class ScreenTraceExtensions
{
	/// <summary>
	/// Build a ray from a screen-space pixel position.
	/// </summary>
	public static Ray ScreenPointToWorldRay(this CameraComponent camera, Vector2 screenPos )
	{
		return camera.ScreenPixelToRay( screenPos );
	}

	/// <summary>
	/// Raycast from a screen-space pixel into the scene.
	/// </summary>
	public static SceneTraceResult TraceFromScreenPoint(
		this CameraComponent camera,
		Scene scene,
		Vector2 screenPos,
		float distance = 250f )
	{
		var ray = camera.ScreenPixelToRay( screenPos );

		var start = ray.Position;
		var end = start + ray.Forward * distance;

		return scene.Trace.Ray( start, end ).Run();
	}

	/// <summary>
	/// Raycast from the current mouse position into the scene.
	/// </summary>
	public static SceneTraceResult TraceFromMouse(
		this CameraComponent camera,
		Scene scene,
		float distance = 250f )
	{
		return camera.TraceFromScreenPoint( scene, Mouse.Position, distance );
	}
}
