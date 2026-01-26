using System.Threading.Tasks;

namespace Sandbox.Seating;

/// <summary>
/// 
/// </summary>
public sealed class SeatAnchor : Component
{
	[Property, Range(0, 255)] public byte Order { get; private set; } = 0;
	[Property, Range(0, 255)] public byte Team { get; private set; } = 0;
	[Property, ReadOnly] public Guid Occupent { get; set; } = Guid.Empty;
	[Property, ReadOnly] public bool IsOccupied => Occupent != Guid.Empty;
	
	
	protected override Task OnLoad(LoadingContext context)
	{
		GameObject.NetworkMode = NetworkMode.Snapshot;
		return Task.CompletedTask;
	}
	
	
	[Rpc.Host]
	public void SetOccupant(Guid id)
	{
		Occupent = id;
	}
	
	[Rpc.Host]
	public void Clear()
	{
		Occupent = Guid.Empty;
	}
	
	
	

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = IsOccupied ? Color.Red : Color.Green;
		Gizmo.Draw.Arrow(Vector3.Zero,Vector3.Forward, 50f);
		Gizmo.Transform = Scene.Transform.World;
		Gizmo.Draw.LineSphere(WorldTransform.Position,8);
		Gizmo.Draw.Text($"Seat {Order}",WorldTransform);
	}
}
