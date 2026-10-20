using Godot;

namespace GoBox.types;

/// <summary>
/// All models are transported via gltf.
/// This will probably be replaced by a custom format.
/// </summary>
public static class Gltf {
    public static Node3D Load(string path) {
        var gltfDoc = new GltfDocument();
        var gltfSt = new GltfState();
        var result = gltfDoc.AppendFromFile(path, gltfSt);

        if (result != Error.Ok) {
            GD.PrintErr($"Failed to load GLTF from '{path}': code {result}");
            return null;
        }

        var node = gltfDoc.GenerateScene(gltfSt);
        if (node is not Node3D nodethreedee) {
            GD.PrintErr($"wtf: GLTF didn't result in a Node3D. '{path}'");
            node.Free();
            return null;
        }

        return nodethreedee;
    }
}