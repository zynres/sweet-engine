using System.Runtime.InteropServices;
using Sweet.Engine.Scene.Components;
using Sweet.Intents.Generated;
using System.Numerics;
using Silk.NET.GLFW;
using Sweet.Intents;
using Sweet.Devices;

namespace Sweet.Engine.Editor.Controllers;

public unsafe struct CameraController : IDisposable
{
    public Transform* Transform;

    private readonly float speedMultiplier;
    private readonly float sensitivity;
    private readonly float speed;

    public float Aspect;

    public CameraController()
    {
        Transform = (Transform*)NativeMemory.Alloc((nuint)sizeof(Transform));

        *Transform = new Transform()
        {
            Position = new(0, 0, 20),
            Rotation = Vector3.Zero
        };

        speedMultiplier = 1.01f;
        sensitivity = 0.002f;
        speed = 25f;
    }

    public void Handle(float deltaTime)
    {
        if (Intent.IsHeld(EditorCameraIntents.MoveState))
        {
            Movement(deltaTime);
            Rotating(deltaTime);
        }
    }

    private void Movement(float deltaTime)
    {
        Vector3 direction =
            Transform->GetForward() * Intent.GetAxis(EditorCameraIntents.MoveForward) +
            Transform->GetRight() * Intent.GetAxis(EditorCameraIntents.MoveRight) +
            Transform->GetUp() * Intent.GetAxis(EditorCameraIntents.MoveUp);

        float currentSpeed = Intent.IsHeld(EditorCameraIntents.Sprint) ?
            speed * speedMultiplier :
            speed;

        if (direction != Vector3.Zero)
            direction = Vector3.Normalize(direction);

        Transform->Position += direction * currentSpeed * deltaTime;
    }

    private void Rotating(float deltaTime)
    {
        Transform->Rotation.Y += sensitivity * Device.Mouse->Delta.X;
        Transform->Rotation.X += sensitivity * Device.Mouse->Delta.Y;

        Transform->Rotation.X = Math.Clamp(
            Transform->Rotation.X,
            -MathF.PI / 2 + 0.01f,
            MathF.PI / 2 - 0.01f);
    }

    public void Dispose()
    {
        if (Transform != null)
        {
            NativeMemory.Free(Transform);
            Transform = null;
        }
    }
}