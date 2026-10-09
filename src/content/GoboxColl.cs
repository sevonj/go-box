using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Godot;

namespace GoBox.content;

public static class GoboxColl {
    public static ConcavePolygonShape3D Load(string path) {
        const int headerSize = 16 + 4 + 4;
        const int minSize = headerSize + 3 * 3 * 4; // + size of tri
        byte[] collSignature = "GoBox Collision\n"u8.ToArray();
        const uint gameCollVersion = 2;

        var bytes = FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0) {
            GD.PushError($"Failed to access file '{path}'");
            return null;
        }

        if (bytes.Length < minSize) {
            GD.PushError($"'{path}' is too small to be a coll file");
            return null;
        }

        var data = new ReadOnlySpan<byte>(bytes);

        if (!data.Slice(0, 16).SequenceEqual(collSignature)) {
            GD.PushError($"'{path}' Coll file signature mismatch.");
            return null;
        }

        var version = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(16, 4));
        if (version != gameCollVersion) {
            GD.PushError($"'{path}' Collision has unsupported version. file={version}, game={gameCollVersion}");
            return null;
        }

        var numTris = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(20, 4));
        var expected = headerSize + numTris * 9 * sizeof(float);
        if (bytes.Length != expected) {
            GD.PushError(
                $"'{path}' Coll file length mismatch. Your file is probably bad. expected={expected}, got={bytes.Length}");
            return null;
        }

        var verts = MemoryMarshal.Cast<byte, Vector3>(data.Slice(headerSize)).ToArray();

        return new ConcavePolygonShape3D { Data = verts };
    }
}