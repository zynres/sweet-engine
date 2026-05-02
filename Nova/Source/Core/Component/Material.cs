using System.Numerics;

namespace Nova
{
    public struct Material
    {
        public Vector4 Color;
        public Texture2D BaseMap;
        public Texture2D NormalMap;
        public Texture2D MetallicMap;
        public Texture2D AOMap;
    }

}
