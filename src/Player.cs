using System.ComponentModel.DataAnnotations;
using GoBox.interfaces;
using Godot;
using Godot.Collections;

namespace GoBox;

[GlobalClass]
public partial class Player : RigidBody3D, IPhysActor {
    private const float WalkSpeed = 4f;
    private const float WalkForce = 50f;
    private const float SprintSpeed = 8.0f;
    private const float SprintForce = 50f;
    private const float FlightSpeed = 10f;
    private const float FastFlightSpeed = 50f;
    private const float JumpSpeed = 4.5f;
    private const float TerminalVelocity = 70f;
    private const float MaxFloorAngle = 45f * (Mathf.Pi / 180f);

    public bool Noclip;

    [Export] private Marker3D _eyePosMarker;
    [Export] private CollisionShape3D _worldColl;
    private PlayerPickupMgr _pickupMgr = new();
    private CameraRig _cameraRig;
    private Node3D _weaponCont = new();

    private bool _isOnFloor;
    private Vector3 _floorNormal = Vector3.Up;
    private Array<Weapon> _weapons = [];
    private int _currentWeapon = -1;


    public static Player Instantiate() => (Player)GD.Load<PackedScene>("res://scenes/player_rigid.tscn").Instantiate();

    private Player() {
        ProcessPhysicsPriority = PProcessPriorities.Player;
    }

    public bool HasControl() => !Game.World.IsMenuOpen() && !LocalVars.DisablePlayerControl;
    public bool IsInteracting() => _pickupMgr.IsCarrying;
    public bool IsInteractionAvailable() => !IsInteracting() && _pickupMgr.CanPickup;

    public void GiveWeapon(Weapon weapon) {
        foreach (var existing in _weapons) {
            if (existing.GetType() == weapon.GetType())
                return;
        }

        _weapons.Add(weapon);
        _weaponCont.AddChild(weapon);
        SelectWeapon(weapon);
    }


    // --- Node

    public override void _Ready() {
        SetContactMonitor(true);
        SetMaxContactsReported(4);

        if (!IsInstanceValid(_eyePosMarker))
            GD.PrintErr(GetPath(), " EyePosMarker invalid");
        if (!IsInstanceValid(_worldColl))
            GD.PrintErr(GetPath(), " _worldColl invalid");
        AddChild(_pickupMgr);
        AddChild(_weaponCont);

        Game.World.AddUi(new UiPlayerHud(this));

        GiveWeapon(WpnPropgun.Instantiate());
        GiveWeapon(WpnManipulator.Instantiate());
        GiveWeapon(WpnRemover.Instantiate());
        GiveWeapon(WpnSavestate.Instantiate());
        GiveWeapon(WpnCorruptor.Instantiate());
        SelectWeapon(0);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state) {
        var numContacts = state.GetContactCount();
        _isOnFloor = false;

        for (var contactIdx = 0; contactIdx < numContacts; contactIdx++) {
            var normal = state.GetContactLocalNormal(contactIdx);
            _isOnFloor |= GlobalBasis.Column1.AngleTo(normal) <= MaxFloorAngle;
        }
    }

    public override void _Process(double delta) {
        EnsureCameraRig();

        var hasControl = HasControl();
        var handsAreFree = !_pickupMgr.IsCarrying;
        _pickupMgr.Enabled = hasControl;

        if (!hasControl) return;

        var weapon = CurrentWeapon();
        if (IsInstanceValid(weapon) && handsAreFree) {
            if (Input.IsActionJustPressed("wpn_fire")) weapon.Fire();
            if (Input.IsActionJustPressed("wpn_fire_alt")) weapon.FireAlt();
        }

        if (handsAreFree) {
            if (Input.IsActionJustPressed("wpn_prev")) SelectPrevWeapon();
            else if (Input.IsActionJustPressed("wpn_next")) SelectNextWeapon();
        }

        if (Input.IsActionJustPressed("noclip")) {
            Noclip = !Noclip;
        }
    }

    public override void _PhysicsProcess(double delta) {
        EnsureCameraRig();

        if (!HasControl()) return;

        LockRotation = true;

        GravityScale = Noclip ? 0f : 1f;
        _worldColl.Disabled = Noclip;

        {
            _weaponCont.Position = _eyePosMarker.Position;
            _weaponCont.GlobalRotation = _cameraRig.Camera.GlobalRotation;
        }

        {
            var inputVec = Input.GetVector(
                "move_left",
                "move_right",
                "move_forward",
                "move_backwards"
            );
            var sprinting = Input.IsActionPressed("move_sprint");

            if (Noclip) {
                LinearDamp = 10f;

                var force = 1000f;
                var desiredSpeed = sprinting ? FastFlightSpeed : FlightSpeed;

                var dir = _cameraRig.Camera.GlobalBasis * new Vector3(inputVec.X, 0f, inputVec.Y);
                dir.Y += Input.GetAxis("move_down", "move_up");
                dir = dir.Normalized();
                var currentSpeed = LinearVelocity.Dot(dir);
                var remainingSpeed = Mathf.Max(0f, desiredSpeed - currentSpeed);
                var limitedForce = Mathf.Min(force, remainingSpeed / (float)delta * Mass);
                ApplyCentralForce(dir * limitedForce);
            }
            else {
                LinearDamp = 0f;

                var force = sprinting ? SprintForce : WalkForce;
                var desiredSpeed = sprinting ? SprintSpeed : WalkSpeed;

                var dir = (_cameraRig.GlobalBasis * new Vector3(inputVec.X, 0, inputVec.Y)).Normalized();
                var currentSpeed = GroundVelocity().Dot(dir);
                var remainingSpeed = Mathf.Max(0f, desiredSpeed - currentSpeed);
                var limitedForce = Mathf.Min(force, remainingSpeed / (float)delta * Mass);
                ApplyCentralForce(dir * limitedForce);
            }
        }

        var velocity = LinearVelocity;

        if (Input.IsActionJustPressed("move_jump") && _isOnFloor) {
            velocity.Y = JumpSpeed;
        }

        if (velocity.Length() > TerminalVelocity) {
            velocity = velocity.Normalized() * TerminalVelocity;
        }

        LinearVelocity = velocity;
    }


    // --- IPhysActor

    public void ApplyForce(Vector3 force) => ApplyCentralForce(force);
    public void ApplyImpulse(Vector3 impulse) => ApplyCentralImpulse(impulse);


    // --- IActor
    public Node3D AsNode3D() => this;


    // --- Private

    private void EnsureCameraRig() {
        if (!IsInstanceValid(_cameraRig)) SpawnCameraRig();
    }

    private void SpawnCameraRig() {
        GD.Print("Spawning CameraRig");
        if (IsInstanceValid(_cameraRig)) _cameraRig.QueueFree();

        _cameraRig = new CameraRigFp(this, _eyePosMarker);
        Game.World.AddActor(_cameraRig);
        _pickupMgr.FollowTarget = _cameraRig.Camera;
    }

    /*
    private bool IsOnFloor() {
        var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(
            PhysicsRayQueryParameters3D.Create(
                GlobalPosition,
                GlobalPosition - GlobalBasis.Column2 * 0.1f
            )
        );
        return result.Count > 0;
    }
    */

    private Vector3 GroundVelocity() => LinearVelocity * new Vector3(1f, 0f, 1f);
    private float GroundSpeed() => GroundVelocity().Length();

    public Weapon CurrentWeapon() {
        if (_currentWeapon < 0 || _currentWeapon >= _weapons.Count)
            return null;
        return _weapons[_currentWeapon];
    }

    private void SelectWeapon(Weapon which) {
        if (which == null) {
            SelectWeapon(-1);
            return;
        }

        var index = _weapons.IndexOf(which);
        if (index == -1) {
            GD.PrintErr("Tried to select weapon not in possession!");
            return;
        }

        SelectWeapon(index);
    }

    private void SelectWeapon(int index) {
        if (index < -1 || index >= _weapons.Count) {
            GD.PrintErr("Weapon select out of range!");
            return;
        }

        foreach (var weapon in _weapons) {
            weapon.Hide();
        }

        if (index >= 0) {
            _weapons[index].Show();
        }

        _currentWeapon = index;
    }

    private void SelectPrevWeapon() {
        if (_currentWeapon > 0) SelectWeapon(_currentWeapon - 1);
    }

    private void SelectNextWeapon() {
        if (_currentWeapon < _weapons.Count - 1) SelectWeapon(_currentWeapon + 1);
    }
}