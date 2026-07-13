namespace SweetEngine.Resources;

public readonly struct VertexKey
{
    public readonly int PosIndex;
    public readonly int UVIndex;
    public readonly int NormalIndex;

    public VertexKey(int v, int uv, int n)
    {
        PosIndex = v;
        UVIndex = uv;
        NormalIndex = n;
    }

    public bool Equals(VertexKey other) =>
        PosIndex == other.PosIndex &&
        UVIndex == other.UVIndex &&
        NormalIndex == other.NormalIndex;

    public override bool Equals(object obj) =>
        obj is VertexKey other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(PosIndex, UVIndex, NormalIndex);

    public static bool operator ==(VertexKey left, VertexKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexKey left, VertexKey right)
    {
        return !(left == right);
    }
}