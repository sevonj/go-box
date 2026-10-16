using GoBox.interfaces;
using Godot;

namespace GoBox;

public partial class PlayerInteractMgr(Player player) : Node3D {
    public bool Enabled = true;
    public float Range = 3f;

    public IInteractable Interactable { get; private set; }


    // --- Node

    public override void _Process(double delta) {
        if (!Enabled) return;

        if (Interactable == null) return;

        if (Input.IsActionJustPressed("interact")) {
            Interactable.Interact(player);
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (!Enabled) return;

        Interactable = FindTarget();
    }


    // --- Private

    private IInteractable FindTarget() {
        var cam = GetViewport().GetCamera3D();
        if (!IsInstanceValid(cam)) {
            GD.PushError($"{GetPath()}: Couldn't access camera.");
            return null;
        }

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = cam.ProjectRayOrigin(mousePos);
        var end = origin + cam.ProjectRayNormal(mousePos) * Range;
        var raycastParams = PhysicsRayQueryParameters3D.Create(origin, end);
        raycastParams.CollisionMask = (uint)ColLayer.Toolgunnable;
        raycastParams.CollideWithAreas = true;
        var result = spaceState.IntersectRay(raycastParams);

        if (result.Count == 0) return null;
        if ((GodotObject)result["collider"] is IInteractable target) return target;
        return null;
    }
}