using System.Runtime.InteropServices;
using SweetEngine.Scene.Components;
using SweetLib.Intents.Generated;
using System.Numerics;
using SweetLib.Intents;
using SweetLib.Devices;

namespace SweetEngine.Controllers;

public unsafe struct CameraController
{
    public Transform Transform;

    private readonly float speedMultiplier;
    private readonly float sensitivity;
    private readonly float speed;

    private readonly Mouse* mouse;
    private readonly Time* time;

    public float Aspect;

    public CameraController(Mouse* mouse, Time* time)
    {
        this.mouse = mouse;
        this.time = time;

        Transform = new Transform()
        {
            Position = new(0, 0, 20),
            Rotation = Vector3.Zero
        };

        speedMultiplier = 1.01f;
        sensitivity = 0.002f;
        speed = 25f;
    }

    public void Update(in Intent intent)
    {
        if (intent.IsHeld(EditorCameraIntents.MoveState))
        {
            Movement(in intent);
            Rotating();
        }
    }

    private void Movement(in Intent intent)
    {
        Vector3 direction =
            Transform.GetForward() * intent.GetAxis(EditorCameraIntents.MoveForward) +
            Transform.GetRight() * intent.GetAxis(EditorCameraIntents.MoveRight) +
            Transform.GetUp() * intent.GetAxis(EditorCameraIntents.MoveUp);

        float currentSpeed = intent.IsHeld(EditorCameraIntents.Sprint) ?
            speed * speedMultiplier :
            speed;

        if (direction != Vector3.Zero)
            direction = Vector3.Normalize(direction);

        Transform.Position += direction * currentSpeed * time->Delta;
    }

    private void Rotating()
    {
        Transform.Rotation.Y += sensitivity * mouse->Delta.X;
        Transform.Rotation.X += sensitivity * mouse->Delta.Y;

        Transform.Rotation.X = Math.Clamp(
            Transform.Rotation.X,
            -MathF.PI / 2 + 0.01f,
            MathF.PI / 2 - 0.01f);
    }
}