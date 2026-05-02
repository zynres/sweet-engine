using System.Numerics;

namespace Nova
{
    public unsafe struct Transform
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public Transform* Parent;
        public Transform* Child;

        public int ModelLoc;
        public int MvpLoc;

        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
        }

        public Matrix4x4 LocalToWorldMatrix
        {
            get
            {
                var scale = Matrix4x4.CreateScale(Scale);
                var rotation =
                    Matrix4x4.CreateRotationX(Rotation.X) *
                    Matrix4x4.CreateRotationY(Rotation.Y) *
                    Matrix4x4.CreateRotationZ(Rotation.Z);
                var translation = Matrix4x4.CreateTranslation(Position);

                var localMatrix = scale * rotation * translation;

                if (Parent != null)
                    return localMatrix * Parent->LocalToWorldMatrix;

                return localMatrix;
            }
        }

        public readonly Vector3 GetForward()
        {
            float cosY = MathF.Cos(Rotation.Y);
            float sinY = MathF.Sin(Rotation.Y);
            float cosX = MathF.Cos(Rotation.X);
            float sinX = MathF.Sin(Rotation.X);

            return new Vector3(
                sinY * cosX,
                -sinX,
                -cosY * cosX
            );
        }

        public void SetParent(Transform* parent)
        {
            Parent = parent;
        }

        public void SetChild(Transform* child)
        {
            Child = child;
        }
    }
}
