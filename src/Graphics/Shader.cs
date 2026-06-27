
namespace Nova
{
    public struct Shader()
    {
        public string vertexSrc = @"
        #version 330 core

        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        layout (location = 2) in vec2 aTexCoords;
        layout (location = 3) in vec3 aTangent;

        out vec2 TexCoords;
        out vec3 FragPos;
        out mat3 TBN;

        uniform mat4 uMVP;
        uniform mat4 uModel;

        void main()
        {
            gl_Position = uMVP * vec4(aPos, 1.0);
            TexCoords = aTexCoords;
            FragPos = vec3(uModel * vec4(aPos, 1.0));

            vec3 T = normalize(mat3(uModel) * aTangent);
            vec3 N = normalize(mat3(uModel) * aNormal);
            vec3 B = cross(N, T);
            TBN = mat3(T, B, N);
        }";
        public string fragmentSrc = @"
        #version 330 core

        in vec2 TexCoords;
        in vec3 FragPos;
        in mat3 TBN; 

        out vec4 FragColor;

        uniform sampler2D uBaseMap;
        uniform sampler2D uNormalMap;
        uniform sampler2D uMetallicMap;
        uniform vec4 uColor;
        uniform vec3 uLightDir;
        uniform vec3 uViewPos;
        uniform float uLightIntensity;

        void main()
        {
            vec2 uv = vec2(TexCoords.x, 1.0 - TexCoords.y);

            vec4 baseColor = texture(uBaseMap, uv) * uColor;

            vec3 normalMap = texture(uNormalMap, uv).rgb;
            normalMap = normalize(normalMap * 2.0 - 1.0);
            vec3 normal = normalize(TBN * normalMap);

            vec3 mrSample = texture(uMetallicMap, uv).rgb;
            float metallic = mrSample.r;
            float roughness = clamp(mrSample.g, 0.05, 1.0);

            vec3 lightDir = normalize(uLightDir);
            vec3 viewDir = normalize(uViewPos - FragPos);

            float diff = max(dot(normal, lightDir), 0.0);
            vec3 diffuse = baseColor.rgb * diff;

            vec3 halfwayDir = normalize(lightDir + viewDir);
            float spec = pow(max(dot(normal, halfwayDir), 0.0), mix(4.0, 64.0, 1.0 - roughness));
            vec3 specular = spec * mix(vec3(0.04), baseColor.rgb, metallic) / 2;

            vec3 finalColor = (diffuse + specular) * uLightIntensity;
            finalColor = pow(finalColor, vec3(1.0/2.2));

            FragColor = vec4(finalColor, baseColor.a);
        }";
    }
}