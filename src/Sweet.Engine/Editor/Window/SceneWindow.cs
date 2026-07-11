using Sweet.Engine.Editor.Controllers;
using Sweet.Engine.Scene.Components;
using System.Numerics;
using ImGuiNET;
using Sweet.Devices;

namespace Sweet.Engine.Editor.Window;

public static unsafe class SceneWindow
{
    private static FrameBuffer* frameBuffer;
    private static CameraController* cameraController;

    public static void Depends(FrameBuffer* frameBuffer, CameraController* cameraController)
    {
        SceneWindow.cameraController = cameraController;
        SceneWindow.frameBuffer = frameBuffer;
    }

    public static void DrawImpl()
    {
        ImGui.Begin("Scene");

        Vector2 size = ImGui.GetContentRegionAvail();

        uint width = (uint)size.X;
        uint height = (uint)size.Y;

        if (width != frameBuffer->Width || height != frameBuffer->Height)
        {
            bool isAspect = true;

            float w = 1920f;
            float h = 1080f;


            if (isAspect)
            {
                frameBuffer->Resize(0, 0, width, height);
            }
            else
            {
                float targetAspect = w / h;
                float windowAspect = (float)width / height;

                uint vpX, vpY, vpW, vpH;

                if (windowAspect > targetAspect)
                {
                    vpH = height;
                    vpW = (uint)(height * targetAspect);
                    vpX = (width - vpW) / 2;
                    vpY = 0;
                }
                else
                {
                    vpW = width;
                    vpH = (uint)(width / targetAspect);
                    vpX = 0;
                    vpY = (height - vpH) / 2;
                }

                frameBuffer->Resize(vpX, vpY, vpW, vpH);
            
                cameraController->Aspect = w / h;
            }

        cameraController->Aspect = w / h;
        cameraController->Aspect = (float)width / height;
    }

    ImGui.Image((nint) frameBuffer->ColorTexture.Id, size, new Vector2(0, 1), new Vector2(1, 0));

        ImGui.End();
    }
}