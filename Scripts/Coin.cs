using Godot;

public partial class Coin : Area3D
{
	private float _rotationSpeed;
	private Vector3 _velocity;
	private Vector3 _position;
	private Vector3 _direction;
	private float _offset;
	private float _floatingSpeed;
	private float _floatingCurve;
	private bool _isUp;
	
	public override void _Ready()
	{
		_rotationSpeed = 4f;
		_position = Position;
		_isUp = false;
		_offset = 0.15f;
		_floatingSpeed = 0.08f;
		_floatingCurve = 2f;
	}
	
	public override void _Process(double delta)
	{
		RotateY(float.DegreesToRadians(_rotationSpeed));

		_direction = new Vector3(0, -0.01f, 0);

		if (Position.Y >= _position.Y + _offset)
			_isUp = false;
		else if (Position.Y <= _position.Y - _offset)
			_isUp = true;

		_direction = _isUp ? new Vector3(0, Mathf.Ease(_floatingSpeed, _floatingCurve), 0) 
			: new Vector3(0, -Mathf.Ease(_floatingSpeed, _floatingCurve), 0);
		
		Translate(_direction);
	}

	private void OnBodyEntered(Node3D body)
	{
		QueueFree();
	}
}
