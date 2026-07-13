using SweetEngine.MDI.Mixers;
using SweetEngine.Resources;
using System.Numerics;
using ImGuiNET;

namespace SweetEngine.Editor.Windows;

public static unsafe class SceneWindow
{
    private static FrameBuffer* frameBuffer;
    private static CameraMixer* CameraMixer;

    public static void Depends(FrameBuffer* frameBuffer, CameraMixer* CameraMixer)
    {
        SceneWindow.CameraMixer = CameraMixer;
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
            
                CameraMixer->Aspect = w / h;
            }

        CameraMixer->Aspect = w / h;
        CameraMixer->Aspect = (float)width / height;
    }

    ImGui.Image((nint) frameBuffer->Color.Id, size, new Vector2(0, 1), new Vector2(1, 0));

        ImGui.End();
    }
}