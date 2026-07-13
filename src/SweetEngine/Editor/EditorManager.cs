using SweetEngine.Core;

namespace SweetEngine.Editor;

public unsafe struct EditorManager
{
    public void Init()
    {
        _ = AssetDirectories.Root;
    }
}