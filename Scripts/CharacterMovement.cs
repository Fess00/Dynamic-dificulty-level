using Godot;

public partial class CharacterMovement : CharacterBody3D
{
    private const float Speed = 4.0f;
    private const float JumpForce = 5.0f;
    private Vector3 _direction;
    private Vector2 _input;
    private Vector3 _velocity;
    private bool _isDoubleJump;
    private bool _isOrdinaryJump;
    private int _coinsCollected;
    
    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(double delta)
    {
        _velocity = Velocity;

        if (!IsOnFloor())
            _velocity += GetGravity() * (float)delta;

        if (IsOnFloor())
            _isOrdinaryJump = true;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            _isDoubleJump = true;
            _velocity.Y = JumpForce;
        }

        if (Input.IsActionJustPressed("jump") && _isDoubleJump && !IsOnFloor())
        {
            _velocity.Y = JumpForce;
            _isDoubleJump = false;
            _isOrdinaryJump = false;
        }
        else if (Input.IsActionJustPressed("jump") && !IsOnFloor() && _isOrdinaryJump)
        {
            _velocity.Y = JumpForce;
            _isOrdinaryJump = false;
        }
        
        _input = Input.GetVector("left", "right", "up", "down");
        _direction = (Transform.Basis * new Vector3(_input.X, 0, _input.Y)).Normalized();

        if (_direction != Vector3.Zero)
        {
            _velocity.X = _direction.X * Speed ;
            _velocity.Z = _direction.Z * Speed;
        }
        else
        {
            _velocity.X = Mathf.MoveToward(_velocity.X, 0, Speed);
            _velocity.Z = Mathf.MoveToward(_velocity.Z, 0, Speed);
        }

        Velocity = _velocity;
        MoveAndSlide();
    }
}
