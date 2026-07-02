using System.Numerics;
using ImGuiNET;

namespace Nova;

public static unsafe class SceneWindow
{
    private static Vector2 lastSceneSize;
    private static FrameBuffer frameBuffer;
    private static CameraController camera;

    public static void DrawImpl()
    {
        ImGui.Begin("Scene");

        /*Vector2 size = ImGui.GetContentRegionAvail();

        if (size != lastSceneSize)
        {
            frameBuffer.Resize((int)size.X, (int)size.Y);

            camera.Aspect = size.X / size.Y;

            lastSceneSize = size;
        }

        ImGui.Image((nint)frameBuffer.Texture.Id, size);*/

        ImGui.End();
    }
}