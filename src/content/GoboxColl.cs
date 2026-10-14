using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Godot;

namespace GoBox.content;

public static class GoboxColl {
    public static Shape3D Load(string path) {
        var headerSize = 16 + 4 + 4;
        int minSize = headerSize + 3 * 3 * 4; // + size of tri
        byte[] collSignature = "GoBox Collision\n"u8.ToArray();
        const uint gameCollVersion = 3;
        const uint gameCollMinVersion = 2;

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
        if (version > gameCollVersion) {
            GD.PushError($"'{path}' Collision has unsupported version. file={version}, game_maximum={gameCollVersion}");
            return null;
        }

        if (version < gameCollMinVersion) {
            GD.PushError($"'{path}' Collision has unsupported version. file={version}, game_minimum={gameCollMinVersion}");
            return null;
        }

        var concave = true;
        uint numVerts;
        if (version == 3) {
            headerSize += 4;
            var collType = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(20, 4));
            if (collType == 0) concave = false;
            numVerts = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(24, 4));
        }
        else {
            numVerts = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(20, 4)) * 3;
        }

        var expected = headerSize + numVerts * 3 * sizeof(float);
        if (bytes.Length != expected) {
            GD.PushError(
                $"'{path}' Coll file length mismatch. Your file is probably bad. expected={expected}, got={bytes.Length}");
            return null;
        }

        var verts = MemoryMarshal.Cast<byte, Vector3>(data.Slice(headerSize)).ToArray();

        if (concave) {
            return new ConcavePolygonShape3D { Data = verts };
        }

        return new ConvexPolygonShape3D { Points = verts };
    }
}