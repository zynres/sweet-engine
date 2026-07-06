using System.Runtime.InteropServices;
using Sweet.Engine.Scene.Components;
using System.Numerics;
using Silk.NET.GLFW;
using input;

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

        if (Input.GetKeyMouse(MouseButton.Right))
        {
            isMouseRight = true;

            Movement(deltaTime);
            Rotating(deltaTime);
        }

        Input.Update(isMouseRight);
    }

    private void Movement(float deltaTime)
    {
        Vector3 forward = Transform->GetForward();
        Vector3 right = Transform->GetRight();
        Vector3 up = Transform->GetUp();
        Vector3 vector = Vector3.Zero;

        if (Input.GetKey(Keys.W))
            vector += forward;
        if (Input.GetKey(Keys.S))
            vector -= forward;
        if (Input.GetKey(Keys.A))
            vector -= right;
        if (Input.GetKey(Keys.D))
            vector += right;

        if (Input.GetKey(Keys.Q))
            vector -= up;
        if (Input.GetKey(Keys.E))
            vector += up;

        if (Input.GetKey(Keys.ShiftLeft))
            speed += 0.05f;
        else
            speed = 25f;

        if (vector != Vector3.Zero)
            vector = Vector3.Normalize(vector);

        Transform->Position += vector * speed * deltaTime;
    }

    private void Rotating(float deltaTime)
    {
        Transform->Rotation.Y += sensitivity * Input.MouseDelta.X;// * deltaTime;
        Transform->Rotation.X += sensitivity * Input.MouseDelta.Y;// * deltaTime;

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