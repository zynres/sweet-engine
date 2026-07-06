using System.Runtime.InteropServices;
using Sweet.Engine.Scene.Components;
using System.Numerics;
using Silk.NET.GLFW;
using Sweet.Intents;

namespace Sweet.Engine.Editor.Controllers;

public unsafe struct CameraController : IDisposable
{
    public Transform* Transform;

    public float Aspect;
    private float sensitivity;
    private float speed;

    public CameraController()
    {
        Transform = (Transform*)NativeMemory.Alloc((nuint)sizeof(Transform));

        *Transform = new Transform()
        {
            Position = new(0, 0, 20),
            Rotation = Vector3.Zero
        };

        sensitivity = 0.002f;
    }

    public void Handle(float deltaTime)
    {
        bool isMouseRight = false;

        if (Intent.GetKeyMouse(MouseButton.Right))
        {
            isMouseRight = true;

            Movement(deltaTime);
            Rotating(deltaTime);
        }

        Intent.Update(isMouseRight);
    }

    private void Movement(float deltaTime)
    {
        Vector3 forward = Transform->GetForward();
        Vector3 right = Transform->GetRight();
        Vector3 up = Transform->GetUp();
        Vector3 vector = Vector3.Zero;

        if (Intent.GetKey(Keys.W))
            vector += forward;
        if (Intent.GetKey(Keys.S))
            vector -= forward;
        if (Intent.GetKey(Keys.A))
            vector -= right;
        if (Intent.GetKey(Keys.D))
            vector += right;

        if (Intent.GetKey(Keys.Q))
            vector -= up;
        if (Intent.GetKey(Keys.E))
            vector += up;

        if (Intent.GetKey(Keys.ShiftLeft))
            speed += 0.05f;
        else
            speed = 25f;

        if (vector != Vector3.Zero)
            vector = Vector3.Normalize(vector);

        Transform->Position += vector * speed * deltaTime;
    }

    private void Rotating(float deltaTime)
    {
        Transform->Rotation.Y += sensitivity * Intent.MouseDelta.X;// * deltaTime;
        Transform->Rotation.X += sensitivity * Intent.MouseDelta.Y;// * deltaTime;

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