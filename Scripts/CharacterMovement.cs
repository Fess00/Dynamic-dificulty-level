using Godot;

public partial class CharacterMovement : CharacterBody3D
{
    [Export]
    private Node3D _model;
    [Export]
    private SpringArm3D _springArm;

    private AnimationPlayer _animation;

    private const float Speed = 4.0f;
    private const float JumpForce = 5.0f;
    private const float Gravity = 2.0f;

    private Vector3 _direction;
    private Vector2 _input;
    private Vector3 _velocity;
    private Vector2 _lookDirection;
    private Vector3 _dashDirection;
    private bool _isDoubleJump;
    private bool _isOrdinaryJump;
    private int _coinsCollected;

    private bool _wasOnFloor;
    private bool _justTouchedGround;
    private bool _isCurrentlyOnFloor;
    private bool _justJumped;
    private bool _isDashing;
    private float _dashSpeed;
    private float _dashDuration;
    private float _dashTimer;
    private bool _canAirDash;
    private bool _canDash;
    private float _dashCooldown;
    private float _dashCooldownTimer;
    private int _health;

    public bool IsInvulnerable { get; private set; }

    public override void _Ready()
    {
        _animation = _model.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (_animation == null)
            GD.PrintErr("AnimationPlayer не найден!");

        _justTouchedGround = false;
        _wasOnFloor = true;
        _justJumped = false;
        _isDashing = false;
        _canAirDash = true;

        _dashDuration = 0.3f;
        _dashTimer = 0f;
        _dashSpeed = 10f;
        _dashCooldown = 0.7f;
        _dashCooldownTimer = 0f;
        _health = 3;

        IsInvulnerable = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        _velocity = Velocity;
        _isCurrentlyOnFloor = IsOnFloor();
        _justTouchedGround = !_wasOnFloor && _isCurrentlyOnFloor;

        if (!_isCurrentlyOnFloor)
            _velocity += GetGravity() * (float)delta;

        if (_isCurrentlyOnFloor)
        {
            _isOrdinaryJump = true;
            _canAirDash = true;
        }

        if (Input.IsActionJustPressed("jump") && _isCurrentlyOnFloor)
        {
            _isDoubleJump = true;
            _velocity.Y = JumpForce;
            _justJumped = true;
            PlayAnim("Jump");
        }

        if (Input.IsActionJustPressed("jump") && _isDoubleJump &&  !_isCurrentlyOnFloor)
        {
            _velocity.Y = JumpForce;
            _isDoubleJump = false;
            _isOrdinaryJump = false;
            _justJumped = true;
            PlayAnim("Jump");
        }
        else if (Input.IsActionJustPressed("jump") && !_isCurrentlyOnFloor && _isOrdinaryJump)
        {
            _velocity.Y = JumpForce;
            _isOrdinaryJump = false;
            _justJumped = true;
            PlayAnim("Jump");
        }

        if (Input.IsActionJustPressed("dash") && !_isDashing)
        {
            bool canDash = _canDash && (_isCurrentlyOnFloor || _canAirDash);

            if (canDash)
            {
                _isDashing = true;
                _dashTimer = _dashDuration;
                _dashDirection = _direction != Vector3.Zero ? _direction : _model.GlobalTransform.Basis.Z;

                _canDash = false;
                _dashCooldownTimer = _dashCooldown;

                if (!_isCurrentlyOnFloor)
                    _canAirDash = false;
                
                SetInvulnerable(true);
                PlayAnim("Dash");
            }
        }

        _input = Input.GetVector("left", "right", "up", "down");
        _direction = (Transform.Basis * new Vector3(_input.X, 0, _input.Y)).Normalized();
        _direction = _direction.Rotated(Vector3.Up, _springArm.Rotation.Y).Normalized();

        if (_direction != Vector3.Zero)
        {
            _velocity.X = _direction.X * Speed;
            _velocity.Z = _direction.Z * Speed;
        }
        else
        {
            _velocity.X = Mathf.MoveToward(_velocity.X, 0, Speed);
            _velocity.Z = Mathf.MoveToward(_velocity.Z, 0, Speed);
        }

        if (_velocity.Length() > 0.2f && (_direction != Vector3.Zero || _isCurrentlyOnFloor))
        {
            _lookDirection = new Vector2(_velocity.Z, _velocity.X);
            float targetAngle = _lookDirection.Angle();
            float currentAngle = _model.Rotation.Y;

            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, (float)delta * 10.0f);
            _model.Rotation = new Vector3(0, smoothAngle, 0);
        }

        if (_isDashing)
        {
            _dashTimer -= (float)delta;

            _velocity.X = _dashDirection.X * _dashSpeed;
            _velocity.Z = _dashDirection.Z * _dashSpeed;
            _velocity.Y = 0;

            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                SetInvulnerable(false);
            }

            Velocity = _velocity;
            MoveAndSlide();
            return;
        }

        ManageAnimation();

        if (!_canDash)
        {
            _dashCooldownTimer -= (float)delta;
            if (_dashCooldownTimer <= 0f)
            {
                _canDash = true;
                _canAirDash = true;
            }
        }

        _wasOnFloor = _isCurrentlyOnFloor;

        Velocity = _velocity;
        MoveAndSlide();
    }

    private void PlayAnim(string name)
    {
        if (_animation.CurrentAnimation == name)
            return;

        if (name == "Jump")
            _animation.Play("Jump", customSpeed: 2.5f);
        else
            _animation.Play(name);
    }

    private void ManageAnimation()
    {
        if (_animation != null && !_isDashing)
        {
            if (_justTouchedGround)
            {
                PlayAnim("TouchGround");
                _justJumped = false;
            }
            else if (!_isCurrentlyOnFloor && _velocity.Y < -0.001f)
            {
                PlayAnim("Fall");
            }
            else if (_isCurrentlyOnFloor && !_justJumped)
            {
                string moveAnim = _velocity.Length() > 0.2f ? "Run" : "Idle";
                if (!_animation.IsPlaying() || _animation.CurrentAnimation != moveAnim)
                    PlayAnim(moveAnim);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        _health -= amount;

        if (_health <= 0)
        {
            RestartGame();
        }
    }

    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
    }

    private void RestartGame()
    {
        var currentScene = GetTree().CurrentScene.SceneFilePath;
        GetTree().ChangeSceneToFile(currentScene);
    }
}