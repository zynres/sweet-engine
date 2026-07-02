using System.Numerics;
using Silk.NET.GLFW;

namespace input;

public unsafe static class Input
{
    private static WindowHandle* window;
    private static Glfw glfw;

    public static Vector2 MousePosition;
    public static Vector2 MouseDelta;

    private static Vector2 _lastMousePosition;

    public static int Width;
    public static int Height;

    public static void Init(WindowHandle* _window, Glfw _glfw)
    {
        window = _window;
        glfw = _glfw;

        glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);

        UpdateWindowSize();

        SetMousePosition(new Vector2(Width / 2, Height / 2));
    }

    private static void UpdateWindowSize()
    {
        glfw.GetWindowSize(window, out int _width, out int _height);

        Width = _width;
        Height = _height;
    }

    public static void Update(bool isMouseRight)
    {
        UpdateWindowSize();

        glfw.GetCursorPos(window, out double x, out double y);

        if (isMouseRight && (x < 0 || x > Width || y < 0 || y > Height))
        {
            if (x < 0)
                x = Width;
            if (x > Width)
                x = 0;

            if (y < 0)
                y = Height;
            if (y > Height)
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