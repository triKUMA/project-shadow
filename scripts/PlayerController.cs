using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using Range = Godot.Range;

public partial class PlayerController : CharacterBody3D {
  [Export]
  public float movementSpeed { get; private set; } = 1f;

  [Export, Range(0f, 1f)]
  public float movementSmoothing { get; private set; } = 0f;

  private Vector3 desiredVelocity = Vector3.Zero;
  private Vector3 velocity = Vector3.Zero;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
  }

  public override void _Process(double delta) {
    desiredVelocity = new Vector3(
      (Input.IsActionPressed("MoveRight") ? 1f : 0f) - (Input.IsActionPressed("MoveLeft") ? 1f : 0f),
      0f,
      -((Input.IsActionPressed("MoveForward") ? 1f : 0f) - (Input.IsActionPressed("MoveBackward") ? 1f : 0f))
    ).Normalized();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(double delta) {
    float floatDelta = (float)delta;
    velocity = velocity.Lerp(desiredVelocity, 1f - movementSmoothing);

    Velocity = velocity * movementSpeed * floatDelta;

    MoveAndSlide();

    desiredVelocity = Vector3.Zero;
  }
}
