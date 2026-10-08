using Godot;

namespace GoBox.content;

public static class GoboxGltf {
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