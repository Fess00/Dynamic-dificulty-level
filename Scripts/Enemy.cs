using Godot;
using System;

public partial class Enemy : Node3D
{
     [Export]
    public NodePath PlayerPath;
    [Export]
    public PackedScene ProjectileScene;

    [Export]
    public float ShootInterval = 1.0f;

    private Node3D _player;
    private Area3D _detectionArea;
    private Marker3D _projectileSpawnPoint;
    private float _shootTimer = 0f;
    private bool _playerInRange = false;

    public override void _Ready()
    {
        _player = GetNode<CharacterBody3D>(PlayerPath);
        _detectionArea = GetNode<Area3D>("DetectionArea");
        _projectileSpawnPoint = GetNode<Marker3D>("ProjectileSpawnPoint");

        _detectionArea.BodyEntered += OnBodyEntered;
        _detectionArea.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerInRange && IsInstanceValid(_player))
        {
            LookAt(new Vector3(_player.GlobalTransform.Origin.X, GlobalTransform.Origin.Y, _player.GlobalTransform.Origin.Z), Vector3.Up);

            _shootTimer -= (float)delta;
            if (_shootTimer <= 0f)
            {
                Shoot();
                _shootTimer = ShootInterval;
            }
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body == _player)
        {
            _playerInRange = true;
            _shootTimer = 0f; // сразу стреляем
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body == _player)
            _playerInRange = false;
    }

    private void Shoot()
    {
        if (ProjectileScene == null)
            return;

        var projectile = ProjectileScene.Instantiate<Node3D>();
        GetParent().AddChild(projectile);

        projectile.GlobalTransform = _projectileSpawnPoint.GlobalTransform;
        var trans = _player.GlobalTransform.Origin;
        trans.Y += 0.8f;
        var dir = (trans - projectile.GlobalTransform.Origin).Normalized();
        (projectile as EnemyProjectile).Direction = dir;

        GetTree().CurrentScene.AddChild(projectile);
    }
}
