using System.Numerics;
using Silk.NET.GLFW;
using static Silk.NET.GLFW.GlfwCallbacks;

namespace input;

public unsafe static class Input
{
    private static WindowHandle* window;
    private static Glfw glfw;

    public static Vector2 MousePosition;
    public static Vector2 MouseDelta;

    private static Vector2 _lastMousePosition;

    private static double lastX;
    private static double lastY;

    private static int width;
    private static int height;

    public static void Init(WindowHandle* _window, Glfw _glfw)
    {
        window = _window;
        glfw = _glfw;

        glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);

        glfw.GetWindowSize(window, out int _width, out int _height);

        SetMousePosition(new Vector2(width / 2, height / 2));

        width = _width;
        height = _height;
    }

    public static void Update(bool isMouseRight)
    {
        glfw.GetCursorPos(window, out double x, out double y);

        if (isMouseRight && (x < 0 || x > width || y < 0 || y > height))
        {
            if (x < 0)
                x = width;
            if (x > width)
                x = 0;

            if (y < 0)
                y = height;
            if (y > height)
                y = 0;

            glfw.SetCursorPos(window, x, y);

            _lastMousePosition = new Vector2((float)x, (float)y);
            MousePosition = _lastMousePosition;
            MouseDelta = Vector2.Zero;

            return;
        }

        MousePosition = new Vector2((float)x, (float)y);
        MouseDelta = MousePosition - _lastMousePosition;
        _lastMousePosition = MousePosition;
    }

    public static void SetMousePosition(Vector2 position)
    {
        glfw.SetCursorPos(window, position.X, position.Y);

        _lastMousePosition = position;
        MousePosition = position;
        MouseDelta = Vector2.Zero;
    }

    public static bool GetKeyDown(Keys key)
    {
        return InputAction.Press == (InputAction)glfw.GetKey(window, key);
    }

    public static bool GetKeyUp(Keys key)
    {
        return InputAction.Release == (InputAction)glfw.GetKey(window, key);
    }

    public static bool GetKey(Keys key)
    {
        var state = (InputAction)glfw.GetKey(window, key);

        return InputAction.Press == state || InputAction.Repeat == state;
    }

    public static bool GetKeyMouse(MouseButton button)
    {
        var state = (InputAction)glfw.GetMouseButton(window, (int)button);

        return InputAction.Press == state || InputAction.Repeat == state; 
    }
}