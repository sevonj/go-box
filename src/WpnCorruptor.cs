using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoBox.interfaces;
using Godot;
using static Godot.Variant;

namespace GoBox;

[GlobalClass]
public partial class WpnCorruptor : Weapon {
    [Export] public uint IntensityContinuous = 10;
    [Export] public uint IntensityBlast = 100;
    [Export] public bool DontTouchPositionExponent = true;

    private const string BeamMatPath = "res://assets/materials/mat_fx_corruptobeam.tres";
    private const float BeamWidthContinuous = 0.2f;
    private const float BeamWidthBlast = 0.5f;

    [Export] private Marker3D _beamStartMarker;
    [Export] private AudioStream _sfxNoUse;
    [Export] private AudioStream _sfxContinuous;
    [Export] private AudioStream _sfxBlast;
    private RandomNumberGenerator _rng = new();
    private AudioStreamPlayer _audioStreamPlayer = new();

    private bool _isFiringContinuously;
    private double _continuousFireTimer;
    private bool _continuousNoUsePlayed;


    public static WpnCorruptor Instantiate() =>
        (WpnCorruptor)GD.Load<PackedScene>("res://scenes/wpn_corruptor.tscn").Instantiate();

    private WpnCorruptor() {
    }

    // --- Node

    public override void _Ready() {
        _rng.Randomize();

        AddChild(_audioStreamPlayer);
        _audioStreamPlayer.Name = "AudioStreamPlayer";
    }

    public override void _Process(double delta) {
        _isFiringContinuously &= Input.IsActionPressed("wpn_fire");
        _continuousFireTimer -= delta;
        _continuousFireTimer = Mathf.Max(_continuousFireTimer, 0.0);

        if (_isFiringContinuously && _continuousFireTimer <= 0.0) {
            _continuousFireTimer += 0.3;

            var cam = GetViewport().GetCamera3D();
            if (!IsInstanceValid(cam)) {
                GD.PrintErr($"{GetPath()}: Couldn't access camera.");
                _isFiringContinuously = false;
                return;
            }

            var spaceState = GetWorld3D().DirectSpaceState;
            var mousePos = GetViewport().GetMousePosition();

            var origin = cam.ProjectRayOrigin(mousePos);
            var end = origin + cam.ProjectRayNormal(mousePos) * 100f;
            var query = PhysicsRayQueryParameters3D.Create(origin, end);
            var result = spaceState.IntersectRay(query);

            if (result.Count == 0 || (GodotObject)result["collider"] is not IProp prop) {
                if (!_continuousNoUsePlayed) PlaySfx(_sfxNoUse);
                _continuousNoUsePlayed = true;
                return;
            }

            var mesh = prop.GetMesh();
            if (mesh == null) {
                _isFiringContinuously = false;
                return;
            }

            CreateBeamFx((Vector3)result["position"], BeamWidthContinuous);
            PlaySfx(_sfxContinuous);
            Corrupt(mesh, IntensityContinuous);
            _continuousNoUsePlayed = true;
        }
    }

    // --- Weapon

    public override string WpnName() => "Corruptor";

    public override string WpnDescription() => "Shoot props for controlled bit flips. Probably safe.\n" +
                                               "Restart the game to undo.\n" +
                                               "Fire: Continuous Alt fire: Blast";

    public override void Fire() {
        _isFiringContinuously = true;
        _continuousNoUsePlayed = false;
    }

    public override void FireAlt() {
        _isFiringContinuously = false;

        var cam = GetViewport().GetCamera3D();
        if (!IsInstanceValid(cam)) {
            GD.PrintErr($"{GetPath()}: Couldn't access camera.");
            return;
        }

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = cam.ProjectRayOrigin(mousePos);
        var end = origin + cam.ProjectRayNormal(mousePos) * 100f;
        var raycastParams = PhysicsRayQueryParameters3D.Create(origin, end);
        raycastParams.CollisionMask = (uint)ColLayer.Toolgunnable;
        raycastParams.CollideWithAreas = true;
        var result = spaceState.IntersectRay(raycastParams);

        if (result.Count == 0 || (GodotObject)result["collider"] is not IProp prop) {
            PlaySfx(_sfxNoUse);
            return;
        }

        var mesh = prop.GetMesh();
        if (mesh == null) return;
        CreateBeamFx((Vector3)result["position"], BeamWidthBlast);
        PlaySfx(_sfxBlast);
        Corrupt(mesh, IntensityBlast);
    }

    // --- Private

    private unsafe void Corrupt(ArrayMesh mesh, uint intensity) {
        var arrays = mesh.SurfaceGetArrays(0);
        var primitive = mesh.SurfaceGetPrimitiveType(0);
        var material = mesh.SurfaceGetMaterial(0);
        var blendShapes = mesh.SurfaceGetBlendShapeArrays(0);

        var buffers = new List<(int BufType, Array Data, int Words)>();
        uint numVertices = 0;

        for (var arrIdx = 0; arrIdx < arrays.Count; arrIdx++) {
            if (!VariantToArray(arrays[arrIdx], out var data, out var words) || words == 0) {
                continue;
            }

            if (arrIdx == (int)Mesh.ArrayType.Vertex) {
                numVertices = (uint)data.Length;
            }

            buffers.Add((arrIdx, data, words));
        }

        var iterations = numVertices * intensity / 100;
        var handles = new GCHandle[buffers.Count];
        for (var i = 0; i < buffers.Count; i++) {
            handles[i] = GCHandle.Alloc(buffers[i].Data, GCHandleType.Pinned);
        }

        // ReSharper disable PossibleNullReferenceException
        for (long i = 0; i < iterations; i++) {
            var chosenOne = (int)(_rng.Randi() % (uint)buffers.Count);
            var (bufType, data, numWords) = buffers[chosenOne];
            var ptr = (void*)handles[chosenOne].AddrOfPinnedObject();
            var idx = (int)(_rng.Randi() % (uint)numWords);

            switch (bufType) {
                case (int)Mesh.ArrayType.Vertex: {
                    var pBuffer = (int*)ptr;
                    pBuffer[idx] ^= 1 << (DontTouchPositionExponent
                            ? (int)(_rng.Randi() % 24 - 1) % 32 // -1 => 31 for sign
                            : (int)(_rng.Randi() % 32)
                        );
                    break;
                }

                case (int)Mesh.ArrayType.Normal:
                case (int)Mesh.ArrayType.Tangent:
                case (int)Mesh.ArrayType.Color:
                case (int)Mesh.ArrayType.TexUV:
                case (int)Mesh.ArrayType.TexUV2:
                case (int)Mesh.ArrayType.Weights: {
                    var pBuffer = (int*)ptr;
                    pBuffer[idx] ^= 1 << (int)(_rng.Randi() % 32);
                    break;
                }

                case (int)Mesh.ArrayType.Index: {
                    var pBuffer = (int*)ptr;
                    pBuffer[idx] ^= 1 << (int)(_rng.Randi() % 32);
                    if ((uint)pBuffer[idx] >= numVertices) {
                        pBuffer[idx] = (int)((uint)pBuffer[idx] % numVertices);
                    }

                    break;
                }
            }
        }

        foreach (var h in handles) {
            if (h.IsAllocated) h.Free();
        }

        foreach (var (bufType, data, _) in buffers) {
            arrays[bufType] = ArrayToVariant(data);
        }

        CorruptMaterial(material, intensity);
        mesh.ClearSurfaces();
        mesh.AddSurfaceFromArrays(primitive, arrays, blendShapes);
        mesh.SurfaceSetMaterial(0, material);
    }

    private unsafe void CorruptMaterial(Material material, uint intensity) {
        if (material is BaseMaterial3D baseMa) {
            foreach (var param in Enum.GetValues<BaseMaterial3D.TextureParam>()) {
                if (param == BaseMaterial3D.TextureParam.Max) {
                    continue;
                }

                baseMa.SetTexture(param, CorruptTexture(baseMa.GetTexture(param), intensity));
            }

            foreach (var dic in baseMa.GetPropertyList()) {
                if (_rng.Randi() % 100 > intensity) continue;

                var name = dic["name"].AsString();
                var v = baseMa.Get(name);
                if (Corrupp̣̅Variant(ref v)) {
                    baseMa.Set(name, v);
                }
            }
        }

        // TODO: Other material types

        return;

        bool Corrupp̣̅Variant(ref Variant v) {
            switch (v.VariantType) {
                // this should be all the float types
                case Variant.Type.Float: Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ((float)v.AsDouble()); break;
                case Variant.Type.Vector2: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsVector2())); break;
                case Variant.Type.Vector3: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsVector3())); break;
                case Variant.Type.Vector4: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsVector4())); break;
                case Variant.Type.Color: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsColor())); break;
                case Variant.Type.Quaternion: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsQuaternion())); break;
                case Variant.Type.Plane: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsPlane())); break;
                case Variant.Type.Rect2: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsRect2())); break;
                case Variant.Type.Aabb: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsAabb())); break;
                case Variant.Type.Basis: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsBasis())); break;
                case Variant.Type.Transform2D: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsTransform2D())); break;
                case Variant.Type.Transform3D: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsTransform3D())); break;
                case Variant.Type.Projection: v = From(Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ(v.AsProjection())); break;
                default: return false;
            }

            return true;

            // ReSharper disable once InconsistentNaming
            T Corrup̧p̵ͬp̶p̶ͬ̑ͅp̸͓̏p̷͈̞̍̾p̶ͨ̿̇ṕ̸̲͚̳͕̗̹̤̣͙̻͍̲̰̹̠͜p̵̢̢͓̼̟̗̝͓̭̱̱͍̗͚̣̎͆̾̃̈́̂̆̾̋͊͆͛̀̑̈́͛̚̚͜͝͝͝͠ͅp̧̨̨͚̘ͩ̉̿p̷̸̡̧̨̢̨̡̧̬͕̞̳͉̘̮̯̺͎͔͙̲͚̳̯̹͉̬̩̻̯̥̦̥̺̙̗͍͍̙̳̳̦͚̙̠̖̝͖̝̙͗̈́̊̓̊́̀̀̆̉͐̿̓̐̀́͗̾̐͐̔͒͗ͨ͂̒̈̐͗̒̕̕͜͝͠ͅp̷̡̨̨̢̼̹̟̥̭̺̳̗͖̰͈͉̹̱̙͖̝̥͈͕̟͚̎͜p̴̧̯̭̩̥̖̯p̷̵̧̧̢̢̨̨̡̧̢̢̝̲͚͍̥̖̹̖̹̰̣͕̹̩̻̻̗̣̣̖̥̞̙̩͉̩̘͙͚͉͚̱̱͕̩̹͍͇͖̠͉̟̭̼̳̥̪̲̫̹̦̭͔̜͈̝͈̋̑̇̅̍̑͆̾͒̐͆̈́̓̊̿̅̆̃̋͛͌͛̅̅͊̏̈͊́̀́̇̑͒̈́̀͒̏̎̀ͩ̀̑̉́́̎̊̓̆̿͒̇̄̽̽̊̀̈́̉̓̈̈́͑̚̚̚̚͜͜͜͝͠ͅͅͅͅͅͅͅ<T>(T value) where T : unmanaged {
                var numWords = (uint)(sizeof(T) / sizeof(int));
                var p = (int*)&value;
                p[_rng.Randi() % numWords] ^= 1 << (int)(_rng.Randi() % 32);
                return value;
            }
        }
    }

    private unsafe Texture2D CorruptTexture(Texture2D t̵̢̢̙̣̗̼̯͔̟̱̖͕̠̳͈͍̪̹̝͕̱͇͙̝̼̬͍̲̗̤̙̰͕̲̣̜͚̟̕͜͜͜é̵̢̛̼̝̗͕͕̫̣͍̙͍͙̫̹̩̿͑͊̋̔̆̔͑̒͗̕͝x̴̢̢̧̳̠͉͉͓̖̻̗̮̦̘̻̲̳̜͍͎͍̉ͅ, uint į̵̢̨̡̛͕̹͕̻̦̫̮͉̺̫̼͍̰̱̙̞̹̤͎̯̗̯̜̙̹͍̱̙̠̪̦͗̓̑͆̈́͋͘ͅn̴̢̛̥̮͖̺̠͕͕̩̙͕̩̻͕̉̂́̓͐̎̌̃̆̐̾͌́͋͊̌̀̎́̉̍̌̒̏̅͒͌̓̓̓̈́̉͆͊̿̈́̈́̿̍̍̿̕͘͜͝͝͠͝͝͠ţ̴̢̡̡̛̛̫͇͙̞̻͍̻̩͎̩͇̬̞̹̱̯͇̯̻͖̠̘̤̖͉̗̘̱̞͉̠͐̿̈́̿̇̍̓́̈̊̀̽̄̆̄̈́̾͛̉̅́̊̄͊̆̚̕̕͘̚̕͜͝͠͠͝ę̷̼͔͓̼̼͔̞͖̬̟̐̌̒̓̓́̀̿́͆̕ṉ̶̝̜̈̔̅̿s̶̢̡̧̛̛̯̙̹̭͉̣͎͕̬͍̖͈̲̟̟͉̗̠̓̀̀̔̔̏̆̃̓̐̑̐͐̌͛̌̈́̐̑̓̉̏̒̀͐̾̈́̂̄͐̈́̌͗̄̋̇̄͊͌͐͒́͘͝͝͠͝ḯ̸̛̛̘̠̭̺̮̟̱̭͈̾̓͂̾͑̽́̇̑̎̌͛̃̂͛̃͛́͛̾̄̀́͗͐͋͗͂̓̃̓̑͛̂͑̋̃̓̕͝͝͝t̸̨̻̯̫̩͖͕̺̻͈̤̹̭̳̦̬͉͇͈̗̟̣̖͈̭͉̺̦̣͙̦̽͒̉͋͒̋̊̅̃̉͜͝͝ͅͅy̶̨̢̧̼̺̳͈͎̩̱̝̣̣̻̲̱͍̤̘͒͂̾̊̏́́͒̑͗́̔̚) {
        if (t̵̢̢̙̣̗̼̯͔̟̱̖͕̠̳͈͍̪̹̝͕̱͇͙̝̼̬͍̲̗̤̙̰͕̲̣̜͚̟̕͜͜͜é̵̢̛̼̝̗͕͕̫̣͍̙͍͙̫̹̩̿͑͊̋̔̆̔͑̒͗̕͝x̴̢̢̧̳̠͉͉͓̖̻̗̮̦̘̻̲̳̜͍͎͍̉ͅ == null) return null;
        var i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌ = t̵̢̢̙̣̗̼̯͔̟̱̖͕̠̳͈͍̪̹̝͕̱͇͙̝̼̬͍̲̗̤̙̰͕̲̣̜͚̟̕͜͜͜é̵̢̛̼̝̗͕͕̫̣͍̙͍͙̫̹̩̿͑͊̋̔̆̔͑̒͗̕͝x̴̢̢̧̳̠͉͉͓̖̻̗̮̦̘̻̲̳̜͍͎͍̉ͅ.GetImage();
        if (i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌ == null) return t̵̢̢̙̣̗̼̯͔̟̱̖͕̠̳͈͍̪̹̝͕̱͇͙̝̼̬͍̲̗̤̙̰͕̲̣̜͚̟̕͜͜͜é̵̢̛̼̝̗͕͕̫̣͍̙͍͙̫̹̩̿͑͊̋̔̆̔͑̒͗̕͝x̴̢̢̧̳̠͉͉͓̖̻̗̮̦̘̻̲̳̜͍͎͍̉ͅ;
        var d̸̡̰̞͚̤̹̝̩͍̥͓̬͙͔̖̫̱̰̞̹̲̺̝̭̩͖͉͆͂̄̏͑̀̋̐̔͐̈́͑̋̐̇͗͗̎̀̽̐͆́͋̅͂̾̔͊͘̕͝͠a̸͙̳̩̪͚͇̬͔͕̝̣̤͓̤͓͊̊̈́͆́̆͘t̷̨̛͙̼̠̟̲̤̼̜̫͎̖͓̗͔̬̔̏͊̽͛̓͛̌͐́̄̎͑͆̀̇̅̾͋͋̅̈́̍͋͛͗̋̐͂̏͑͂́̾͆̕͘̚̕͜͜͝͠ͅą̴̧̧̡̢̢̙̲͙̩̭̪̱̯̘̗̯͔͈͕̪̻̘͕̫͍̜͎̲͇͎͈̩͎̞̼͇͔̥̘̲͓̱͎̙̯̩̹̎̒̅́͌̋̿̓̃̆͋̑̂́̃̑̓̍̆̃͆́͋͊́͊̓́͆̑̏̆́̊̆͌̆̃̊͘̚̚̕͜͠͠ = i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌.GetData();

        var i̴̢̛̺̪͚̲̼̘̮̝̞̭̙̰̺̱̘͛̀̎́͠ṱ̷̨̛̛̌̓̈̾̄͆͛͗̄͐̽̓͆̍̒̓̉́̈́̔́͑̈́͑̈́̓͒͗̾͛̓̈̇̕̕͠ę̴̞̱̝͈̣̜̦͈̖̠̙̹̹̻̲͕̼̝̩̗̦̜̥̤̙̗̹̫̝̫̲͔̳͙͎̓́̈́́̅̃̑̽̇͆͛̍̉͒̈́͗͋̊̍͛̋̽̿̕̚͠͠r̵̢̡̧̡̨̡̡̡̛͎͚̻͓̦̬̣͕̘͎̺̫̜͚̟͈̹̫̘̞͖̭͇̗͉̹͇̣̙͙̗̱̙̲̭̠͇͌̒͊͒͑̀̌̎̄̒̇̿̑̾̈̒̄̋́̄̐͐̀̂̎̏̚̕͘͜͝͝ą̵̡̛̼̱͚̭̖̲̘̻̙̬͓̱͍̜͕̼̬̮͙̘̤̱̝̰̥͒̃̎ṱ̸̡̡̢͈̻̠̱̮̫̦̻̝̬̮͚͕̲͚͍̻͔̥͍̙͙̙̜̲͚̬͓̭͔̜̼̞̖͓͈̋̈́͊͌̀̓͋̔̅̈́͂͊̃̄͆͌̓̈́̐́̆́̕͜͜͝͝ͅï̸̗̩̠̥̗̭͉͚͎̀̈́̅͆̊̌̂͐́̈͂̉͋̄̑̍̈͊̈́̈́̈́̏̅̚̕͘͜͝͝͠ǫ̸̢̨͈̲̟̠͕͈͖͉͚̳̱͍͖͓̞̥͈̤͇̠͖̠̳̰͙̲̙͙͙̙̫̳͖͒͂͛͊̂̀̒͒͒͊̌̀̓̈̈́̊̅̒̈́̌̉̆̑͊̓͛͊́͗͑͐͐̀̈̀̈́͑͜͠͠͝ṉ̵̡̛̛̬̼͎̖̱͚̲̦̭͓̞̦̦͚̜̦̪̗̪̠̰̖̯̬̫͉̗̫̩̙̣̮̯͉̥͖̹̜͍͎́̄̉̑͐̌͌̽̌͑̽̀͆̏̈̓͌͊͗͌̃̀̀́̑̀͑͌͑̈́̒̔͋̓̒̀̍̓̚̕͜͝͝͝͝͝͝s̵̡̧̥̗̥̤̲̼͖̰̺̞͈̣̱͓̞͖̹̪̤̻͔͍̤̞͇̤̺̪̠̪̗̙̠̲̻͇̤̲͗̍̂̈́̈͗̑͋̃̂͆̆̏͆͂̓͌̋̀̉̑̆̈́̚͜͝͝͝͝͝ = d̸̡̰̞͚̤̹̝̩͍̥͓̬͙͔̖̫̱̰̞̹̲̺̝̭̩͖͉͆͂̄̏͑̀̋̐̔͐̈́͑̋̐̇͗͗̎̀̽̐͆́͋̅͂̾̔͊͘̕͝͠a̸͙̳̩̪͚͇̬͔͕̝̣̤͓̤͓͊̊̈́͆́̆͘t̷̨̛͙̼̠̟̲̤̼̜̫͎̖͓̗͔̬̔̏͊̽͛̓͛̌͐́̄̎͑͆̀̇̅̾͋͋̅̈́̍͋͛͗̋̐͂̏͑͂́̾͆̕͘̚̕͜͜͝͠ͅą̴̧̧̡̢̢̙̲͙̩̭̪̱̯̘̗̯͔͈͕̪̻̘͕̫͍̜͎̲͇͎͈̩͎̞̼͇͔̥̘̲͓̱͎̙̯̩̹̎̒̅́͌̋̿̓̃̆͋̑̂́̃̑̓̍̆̃͆́͋͊́͊̓́͆̑̏̆́̊̆͌̆̃̊͘̚̚̕͜͠͠.Length * į̵̢̨̡̛͕̹͕̻̦̫̮͉̺̫̼͍̰̱̙̞̹̤͎̯̗̯̜̙̹͍̱̙̠̪̦͗̓̑͆̈́͋͘ͅn̴̢̛̥̮͖̺̠͕͕̩̙͕̩̻͕̉̂́̓͐̎̌̃̆̐̾͌́͋͊̌̀̎́̉̍̌̒̏̅͒͌̓̓̓̈́̉͆͊̿̈́̈́̿̍̍̿̕͘͜͝͝͠͝͝͠ţ̴̢̡̡̛̛̫͇͙̞̻͍̻̩͎̩͇̬̞̹̱̯͇̯̻͖̠̘̤̖͉̗̘̱̞͉̠͐̿̈́̿̇̍̓́̈̊̀̽̄̆̄̈́̾͛̉̅́̊̄͊̆̚̕̕͘̚̕͜͝͠͠͝ę̷̼͔͓̼̼͔̞͖̬̟̐̌̒̓̓́̀̿́͆̕ṉ̶̝̜̈̔̅̿s̶̢̡̧̛̛̯̙̹̭͉̣͎͕̬͍̖͈̲̟̟͉̗̠̓̀̀̔̔̏̆̃̓̐̑̐͐̌͛̌̈́̐̑̓̉̏̒̀͐̾̈́̂̄͐̈́̌͗̄̋̇̄͊͌͐͒́͘͝͝͠͝ḯ̸̛̛̘̠̭̺̮̟̱̭͈̾̓͂̾͑̽́̇̑̎̌͛̃̂͛̃͛́͛̾̄̀́͗͐͋͗͂̓̃̓̑͛̂͑̋̃̓̕͝͝͝t̸̨̻̯̫̩͖͕̺̻͈̤̹̭̳̦̬͉͇͈̗̟̣̖͈̭͉̺̦̣͙̦̽͒̉͋͒̋̊̅̃̉͜͝͝ͅͅy̶̨̢̧̼̺̳͈͎̩̱̝̣̣̻̲̱͍̤̘͒͂̾̊̏́́͒̑͗́̔̚ / 1000;
        fixed (byte* p = d̸̡̰̞͚̤̹̝̩͍̥͓̬͙͔̖̫̱̰̞̹̲̺̝̭̩͖͉͆͂̄̏͑̀̋̐̔͐̈́͑̋̐̇͗͗̎̀̽̐͆́͋̅͂̾̔͊͘̕͝͠a̸͙̳̩̪͚͇̬͔͕̝̣̤͓̤͓͊̊̈́͆́̆͘t̷̨̛͙̼̠̟̲̤̼̜̫͎̖͓̗͔̬̔̏͊̽͛̓͛̌͐́̄̎͑͆̀̇̅̾͋͋̅̈́̍͋͛͗̋̐͂̏͑͂́̾͆̕͘̚̕͜͜͝͠ͅą̴̧̧̡̢̢̙̲͙̩̭̪̱̯̘̗̯͔͈͕̪̻̘͕̫͍̜͎̲͇͎͈̩͎̞̼͇͔̥̘̲͓̱͎̙̯̩̹̎̒̅́͌̋̿̓̃̆͋̑̂́̃̑̓̍̆̃͆́͋͊́͊̓́͆̑̏̆́̊̆͌̆̃̊͘̚̚̕͜͠͠) {
            for (long i̶̤̔͛͑̔̇̿͘͝ = 0; i̶̤̔͛͑̔̇̿͘͝ < i̴̢̛̺̪͚̲̼̘̮̝̞̭̙̰̺̱̘͛̀̎́͠ṱ̷̨̛̛̌̓̈̾̄͆͛͗̄͐̽̓͆̍̒̓̉́̈́̔́͑̈́͑̈́̓͒͗̾͛̓̈̇̕̕͠ę̴̞̱̝͈̣̜̦͈̖̠̙̹̹̻̲͕̼̝̩̗̦̜̥̤̙̗̹̫̝̫̲͔̳͙͎̓́̈́́̅̃̑̽̇͆͛̍̉͒̈́͗͋̊̍͛̋̽̿̕̚͠͠r̵̢̡̧̡̨̡̡̡̛͎͚̻͓̦̬̣͕̘͎̺̫̜͚̟͈̹̫̘̞͖̭͇̗͉̹͇̣̙͙̗̱̙̲̭̠͇͌̒͊͒͑̀̌̎̄̒̇̿̑̾̈̒̄̋́̄̐͐̀̂̎̏̚̕͘͜͝͝ą̵̡̛̼̱͚̭̖̲̘̻̙̬͓̱͍̜͕̼̬̮͙̘̤̱̝̰̥͒̃̎ṱ̸̡̡̢͈̻̠̱̮̫̦̻̝̬̮͚͕̲͚͍̻͔̥͍̙͙̙̜̲͚̬͓̭͔̜̼̞̖͓͈̋̈́͊͌̀̓͋̔̅̈́͂͊̃̄͆͌̓̈́̐́̆́̕͜͜͝͝ͅï̸̗̩̠̥̗̭͉͚͎̀̈́̅͆̊̌̂͐́̈͂̉͋̄̑̍̈͊̈́̈́̈́̏̅̚̕͘͜͝͝͠ǫ̸̢̨͈̲̟̠͕͈͖͉͚̳̱͍͖͓̞̥͈̤͇̠͖̠̳̰͙̲̙͙͙̙̫̳͖͒͂͛͊̂̀̒͒͒͊̌̀̓̈̈́̊̅̒̈́̌̉̆̑͊̓͛͊́͗͑͐͐̀̈̀̈́͑͜͠͠͝ṉ̵̡̛̛̬̼͎̖̱͚̲̦̭͓̞̦̦͚̜̦̪̗̪̠̰̖̯̬̫͉̗̫̩̙̣̮̯͉̥͖̹̜͍͎́̄̉̑͐̌͌̽̌͑̽̀͆̏̈̓͌͊͗͌̃̀̀́̑̀͑͌͑̈́̒̔͋̓̒̀̍̓̚̕͜͝͝͝͝͝͝s̵̡̧̥̗̥̤̲̼͖̰̺̞͈̣̱͓̞͖̹̪̤̻͔͍̤̞͇̤̺̪̠̪̗̙̠̲̻͇̤̲͗̍̂̈́̈͗̑͋̃̂͆̆̏͆͂̓͌̋̀̉̑̆̈́̚͜͝͝͝͝͝; i̶̤̔͛͑̔̇̿͘͝++) {
                p[_rng.Randi() % (uint)d̸̡̰̞͚̤̹̝̩͍̥͓̬͙͔̖̫̱̰̞̹̲̺̝̭̩͖͉͆͂̄̏͑̀̋̐̔͐̈́͑̋̐̇͗͗̎̀̽̐͆́͋̅͂̾̔͊͘̕͝͠a̸͙̳̩̪͚͇̬͔͕̝̣̤͓̤͓͊̊̈́͆́̆͘t̷̨̛͙̼̠̟̲̤̼̜̫͎̖͓̗͔̬̔̏͊̽͛̓͛̌͐́̄̎͑͆̀̇̅̾͋͋̅̈́̍͋͛͗̋̐͂̏͑͂́̾͆̕͘̚̕͜͜͝͠ͅą̴̧̧̡̢̢̙̲͙̩̭̪̱̯̘̗̯͔͈͕̪̻̘͕̫͍̜͎̲͇͎͈̩͎̞̼͇͔̥̘̲͓̱͎̙̯̩̹̎̒̅́͌̋̿̓̃̆͋̑̂́̃̑̓̍̆̃͆́͋͊́͊̓́͆̑̏̆́̊̆͌̆̃̊͘̚̚̕͜͠͠.Length] ^= (byte)(1 << (int)(_rng.Randi() % 8));
            }
        }

        var corrupted = Image.CreateFromData(i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌.GetWidth(), i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌.GetHeight(), i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌.HasMipmaps(), i̸̢̨̧̡̢̢͈̠̲̳̞͚̙͉̩̫͙͚̩̘̜̰̞̤̫̙̻͚̻̫̼̘͙̪̤̬͙͔̳̺̻̬͇͆͒̆́̀̅͌̇͋̄̓̽̓̍͜ͅͅͅm̴̨̨̧̧̛̛̖̫̪͖̖̠̱̬̗̝̙̳͍͓̺̭̪̙͎͈̟̩̣̖͇͉̖̪͕͙̦̣͖̲̪͔̦̟̦͎̥̈́̒͆̒̎̈́͒̄̿̇͊͐̅͆̏͆̒̂̂̓̈̀̌̑̔͛̚͜͝ͅͅͅg̴̨̤͉̲̒̽̌̌̌.GetFormat(), d̸̡̰̞͚̤̹̝̩͍̥͓̬͙͔̖̫̱̰̞̹̲̺̝̭̩͖͉͆͂̄̏͑̀̋̐̔͐̈́͑̋̐̇͗͗̎̀̽̐͆́͋̅͂̾̔͊͘̕͝͠a̸͙̳̩̪͚͇̬͔͕̝̣̤͓̤͓͊̊̈́͆́̆͘t̷̨̛͙̼̠̟̲̤̼̜̫͎̖͓̗͔̬̔̏͊̽͛̓͛̌͐́̄̎͑͆̀̇̅̾͋͋̅̈́̍͋͛͗̋̐͂̏͑͂́̾͆̕͘̚̕͜͜͝͠ͅą̴̧̧̡̢̢̙̲͙̩̭̪̱̯̘̗̯͔͈͕̪̻̘͕̫͍̜͎̲͇͎͈̩͎̞̼͇͔̥̘̲͓̱͎̙̯̩̹̎̒̅́͌̋̿̓̃̆͋̑̂́̃̑̓̍̆̃͆́͋͊́͊̓́͆̑̏̆́̊̆͌̆̃̊͘̚̚̕͜͠͠);
        return ImageTexture.CreateFromImage(corrupted);
    }

    private static bool VariantToArray(Variant v, out Array data, out int numWords) {
        switch (v.VariantType) {
            case Variant.Type.PackedVector3Array: {
                var a = v.AsVector3Array();
                data = a;
                numWords = a.Length * 3;
                return true;
            }
            case Variant.Type.PackedVector2Array: {
                var a = v.AsVector2Array();
                data = a;
                numWords = a.Length * 2;
                return true;
            }
            case Variant.Type.PackedColorArray: {
                var a = v.AsColorArray();
                data = a;
                numWords = a.Length * 4;
                return true;
            }
            case Variant.Type.PackedFloat32Array: {
                var a = v.AsFloat32Array();
                data = a;
                numWords = a.Length;
                return true;
            }
            case Variant.Type.PackedInt32Array: {
                var a = v.AsInt32Array();
                data = a;
                numWords = a.Length;
                return true;
            }
            case Variant.Type.PackedByteArray: {
                var a = v.AsByteArray();
                data = a;
                numWords = a.Length;
                return true;
            }
            default:
                data = null;
                numWords = 0;
                return false;
        }
    }

    private static Variant ArrayToVariant(Array data) => data switch {
        Vector3[] a => a,
        Vector2[] a => a,
        Color[] a => a,
        float[] a => a,
        int[] a => a,
        byte[] a => a,
        _ => default
    };

    private void CreateBeamFx(Vector3 target, float beamWidth) {
        var start = _beamStartMarker.GlobalPosition;

        var mesh = new ImmediateMesh();
        var len = start.DistanceTo(target);
        mesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);
        mesh.SurfaceSetUV(new Vector2(0f, 0f));
        mesh.SurfaceAddVertex(new Vector3(0f, beamWidth / 2f, 0f));
        mesh.SurfaceSetUV(new Vector2(1f, 0f));
        mesh.SurfaceAddVertex(new Vector3(0f, beamWidth / 2f, -len));
        mesh.SurfaceSetUV(new Vector2(0f, 1f));
        mesh.SurfaceAddVertex(new Vector3(0f, -beamWidth / 2f, 0f));
        mesh.SurfaceSetUV(new Vector2(1f, 1f));
        mesh.SurfaceAddVertex(new Vector3(0f, -beamWidth / 2f, -len));
        mesh.SurfaceEnd();
        mesh.SurfaceSetMaterial(0, ResourceLoader.Load<Material>(BeamMatPath));

        var beam = new MeshInstance3D();
        beam.SetMesh(mesh);
        beam.Name = "FxCorruptoBeam";
        Game.World.AddActor(beam);
        beam.GlobalPosition = start;
        beam.LookAt(target);
        GetTree().CreateTimer(0.1).Timeout += beam.QueueFree;
    }

    private void PlaySfx(AudioStream sfx) {
        //  if (sfx == _sfxContinuous && _audioStreamPlayer.Playing && _audioStreamPlayer.Stream == sfx) {
        //      return;
        //  }

        _audioStreamPlayer.VolumeLinear = sfx == _sfxNoUse ? 0.25f : 1f;
        _audioStreamPlayer.Stream = sfx;
        _audioStreamPlayer.Play();
    }

    /* private void StopContinuousSfx() {
         if (!_audioStreamPlayer.Playing || _audioStreamPlayer.Stream != _sfxContinuous) {
             return;
         }

         _audioStreamPlayer.Stop();
     } */
}