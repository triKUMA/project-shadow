using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using Range = Godot.Range;

public partial class PlayerController : CharacterBody3D {
  [Export] public float movementSpeed { get; private set; } = 8f;
  [Export] public float acceleration { get; private set; } = 20f;
  [Export] public float jumpImpulse { get; private set; } = 12f;
  [Export] public float gravity { get; private set; } = -30f;

  public Vector3 lastMovementDirection { get; private set; } = Vector3.Back;
  public bool isMoving => desiredVelocity.LengthSquared() > movingThreshold;

  private Vector3 desiredVelocity = Vector3.Zero;
  private Vector3 velocity = Vector3.Zero;
  private float movingThreshold = 0.2f;
  private bool isStartingJump = false;

  private Camera3D camera;

  public override void _Ready() {
    camera = GDExtensions.GetChildByType<Camera3D>(this);
  }

  public override void _Process(double delta) {
    var rawInput = Input.GetVector("move_left", "move_right", "move_backwards", "move_forwards");

    var forward = camera.GlobalBasis.Z;
    var right = camera.GlobalBasis.X;

    desiredVelocity = right * rawInput.X + forward * -rawInput.Y;
    desiredVelocity.Y = 0;
    desiredVelocity = desiredVelocity.Normalized();

    isStartingJump |= Input.IsActionJustPressed("jump") && IsOnFloor();
  }

  public override void _PhysicsProcess(double doubleDelta) {
    float delta = (float)doubleDelta;

    var yVelocity = Velocity.Y;
    Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
    Velocity = Velocity.MoveToward(desiredVelocity * movementSpeed, acceleration * delta);
    Velocity = new Vector3(Velocity.X, yVelocity + gravity * delta, Velocity.Z);

    if (isStartingJump) {
      isStartingJump = false;
      Velocity = new Vector3(Velocity.X, Velocity.Y + jumpImpulse, Velocity.Z);
    }

    MoveAndSlide();

    if (desiredVelocity.LengthSquared() > movingThreshold * movingThreshold) {
      lastMovementDirection = desiredVelocity;
    }
  }
}
