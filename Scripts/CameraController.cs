using Godot;

public partial class CameraController : SpringArm3D
{
	private float _mouseSensitivity;
	private float _yaw;
	private float _pitch;

	private Vector2 _inputRotation;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;

		_mouseSensitivity = 0.1f;

		_inputRotation = Vector2.Zero;
	}

	public override void _Process(double delta)
	{
		float deltaF = (float)delta;

		_yaw -= _inputRotation.X;
		_pitch -= _inputRotation.Y;
		_pitch = Mathf.Clamp(_pitch, -45, 60);
		_inputRotation = Vector2.Zero;

		RotationDegrees = new Vector3(_pitch, _yaw, 0);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_inputRotation = mouseMotion.Relative * _mouseSensitivity;
		}
	}
}
