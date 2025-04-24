using Godot;

public partial class EnemyProjectile : Node3D
{
    [Export]
    public float Speed = 10.0f;

    [Export]
    public float LifeTime = 3.0f;
    [Export]
    private Area3D _area;

    public Vector3 Direction = Vector3.Forward;
    private float _timer = 0f;

    public override void _Ready()
    {
        _timer = LifeTime;
        _area.BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalTranslate(Direction.Normalized() * Speed * (float)delta);

        _timer -= (float)delta;
        if (_timer <= 0f)
            CallDeferred("queue_free");
    }

    private void OnBodyEntered(Node body)
    {
        if (body is CharacterMovement player)
        {
            if (!player.IsInvulnerable)
            {
                player.TakeDamage(1);
                CallDeferred("queue_free");
            }
        }
    }
}
