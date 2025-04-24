using Godot;
using System;

public partial class Enemy : Node3D
{
     [Export]
    public NodePath PlayerPath;
    [Export]
    public PackedScene ProjectileScene;
    [Export]
    public float shootInterval;

    private Node3D _player;
    private Area3D _shootArea;
    private Area3D _disableArea;
    private Marker3D _projectileSpawnPoint;
    private float _shootTimer;
    private bool _playerInRange;
    private bool _shootingDisabled;

    public override void _Ready()
    {
        _playerInRange = false;
        _shootingDisabled = false;
        _shootTimer = 0f;
        shootInterval = 1.0f;

        _player = GetNode<CharacterBody3D>(PlayerPath);
        _shootArea = GetNode<Area3D>("ShootArea");
        _disableArea = GetNode<Area3D>("DisableArea");
        _projectileSpawnPoint = GetNode<Marker3D>("ProjectileSpawnPoint");

        _shootArea.BodyEntered += OnBodyEntered;
        _shootArea.BodyExited += OnBodyExited;

        _disableArea.BodyEntered += OnDisableBodyEnter;
        _disableArea.BodyExited += OnDisableBodyExit;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerInRange && IsInstanceValid(_player))
        {
            LookAt(new Vector3(_player.GlobalTransform.Origin.X, GlobalTransform.Origin.Y, _player.GlobalTransform.Origin.Z), Vector3.Up);

            _shootTimer -= (float)delta;
            if (_shootTimer <= 0f && !_shootingDisabled)
            {
                Shoot();
                _shootTimer = shootInterval;
            }
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body == _player)
        {
            _playerInRange = true;
            _shootTimer = 0f;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body == _player)
            _playerInRange = false;
    }

    private void OnDisableBodyEnter(Node body)
    {
        if (body == _player)
            _shootingDisabled = true;
    }

    private void OnDisableBodyExit(Node body)
    {
        if (body == _player)
            _shootingDisabled = false;
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
    }
}
