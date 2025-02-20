using BehaviorDesigner.Runtime;
using Invector;
using Invector.vCharacterController;
using UnityEngine;

public class vAllieShooterInput : vShooterMeleeInput
{
    [vEditorToolbar("Allie Setting")]

    [SerializeField]
    private BehaviorTree _allieBehaviorTree;

    [Header("Allie Aiming Setting")]

    [SerializeField]
    private float _maxAllieAimingDistance;

    private bool _isControlByPlayer;
    private bool _isAllieAiming;
    private bool _isAllieShooting;
    private Vector3 _shootPosition;

    public override bool IsAiming
    {
        get
        {
            if (_isControlByPlayer)
                return base.IsAiming;

            return _isAllieAiming && !isReloading;
        }
    }

    public override Vector3 AimPosition
    {
        get
        {
            if (_isControlByPlayer)
                return base.AimPosition;

            return _shootPosition;
        }
    }

    public bool IsControlledByPlayer => _isControlByPlayer;

    #region ALLIE CONTROL

    public virtual void AllieStartAiming()
    {
        _isAllieAiming = true;
        isAimingByInput = true;
    }

    public virtual void AllieStopAiming()
    {
        _isAllieAiming = false;
        isAimingByInput = false;
    }

    public virtual void AllieShoot(Vector3 targetPosition)
    {
        _shootPosition = targetPosition;
        _isAllieShooting = true;
    }

    public virtual void AllieStopShooting()
    {
        _isAllieShooting= false;
    }

    #endregion

    //Run in FixedUpdate
    public override void InputHandle()
    {
        if (_isControlByPlayer)
        {
            base.InputHandle();
        }
        else
        {
            if (cc == null || cc.isDead)
            {
                return;
            }

            #region BasicInput

            if (!cc.ragdolled)
            {
                MoveInput();
                SprintInput();
                CrouchInput();
                StrafeInput();
                JumpInput();
                RollInput();
            }

            #endregion

            #region MeleeInput

            if (MeleeAttackConditions() && !IsAiming && !isReloading && !lockMeleeInput && !CurrentActiveWeapon)
            {
                if (shooterManager.canUseMeleeWeakAttack_H || shooterManager.CurrentWeapon == null)
                {
                    MeleeWeakAttackInput();
                }

                if (shooterManager.canUseMeleeStrongAttack_H || shooterManager.CurrentWeapon == null)
                {
                    MeleeStrongAttackInput();
                }

                if (shooterManager.canUseMeleeBlock_H || shooterManager.CurrentWeapon == null)
                {
                    BlockingInput();
                }
                else
                {
                    isBlocking = false;
                }
            }

            #endregion

            #region ShooterInput

            if (shooterManager.CurrentWeapon)
            {
                AimInput();
                ShotInput();
                //ReloadInput();
                //SwitchCameraSideInput();
                //ScopeViewInput();
            }
            else
            {
                isAimingByInput = false;
                _aimTiming = 0;
                if (!cc.lockInStrafe && cc.isStrafing && cc.locomotionType != vThirdPersonMotor.LocomotionType.OnlyStrafe)
                {
                    cc.Strafe();
                }
                if (_walkingByDefaultWasChanged && !IsAiming)
                {
                    _walkingByDefaultWasChanged = false;
                    ResetWalkByDefault();
                }
                if (controlAimCanvas != null)
                {
                    if (controlAimCanvas.isAimActive || controlAimCanvas.isScopeCameraActive)
                    {
                        isUsingScopeView = false;
                        controlAimCanvas.DisableAim();
                    }
                }
            }
            #endregion
        }
    }

    public override void ShotInput()
    {
        if (_isControlByPlayer)
        {
            base.ShotInput();
        }
        else
        {
            if (!shooterManager || CurrentActiveWeapon == null || cc.isDead || isReloading || isAttacking || isEquipping)
            {
                if (shooterManager && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0)
                {
                    CurrentActiveWeapon.powerCharge = 0;
                }

                shootCountA = 0;
                return;
            }

            if (IsAiming && !shooterManager.isShooting && aimConditions)
            {
                if (CurrentActiveWeapon || (shooterManager.CurrentWeapon && shooterManager.hipfireShot))
                {
                    HandleShotCount(shooterManager.CurrentWeapon, _isAllieShooting);
                }
            }
            else if (!IsAiming)
            {
                if (shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0)
                {
                    CurrentActiveWeapon.powerCharge = 0;
                }

                shootCountA = 0;
            }
        }
    }

    protected override void UpdateAimBehaviour()
    {
        if (_isControlByPlayer)
        {
            base.UpdateAimBehaviour();
            return;
        }

        if (cc.isDead || cc.ragdolled)
        {
            return;
        }

        UpdateDesiredAimPosition();
        UpdateHeadTrack();
        OnStartUpdateIK();
        CheckAimConditions();
        if (shooterManager && CurrentActiveWeapon)
        {
            UpdateIKAdjust(shooterManager.IsLeftWeapon);
            AlignArmToAimPosition(shooterManager.IsLeftWeapon);
            UpdateArmsIK(shooterManager.IsLeftWeapon);
        }
        UpdateAllieAimPosition();
        OnFinishUpdateIK();
        DoShots();
        CheckAimEvents();
    }

    private void UpdateAllieAimPosition()
    {
        if (!shooterManager || CurrentActiveWeapon == null)
        {
            return;
        }

        DesiredAimPosition = _shootPosition;

        Vector3 rayDirection = DesiredAimPosition - CurrentActiveWeapon.aimReference.position;
        ray.origin = CurrentActiveWeapon.aimReference.position;
        ray.direction = rayDirection;
        Vector3 desiredAimPoint = DesiredAimPosition;


        RaycastHit hit;
        var castLayer = shooterManager.blockAimLayer;
        var castDistance = rayDirection.magnitude;
        var castRay = ray;

        if (Physics.Raycast(castRay, out hit, castDistance, castLayer))
        {
            bool canAimToHit = false;
            var dist = _maxAllieAimingDistance;

            //Check if hit object is child of  character transform
            if (hit.collider.transform.IsChildOf(transform))
            {
                var collider = hit.collider;
                //Clear last hit infor
                hit = new RaycastHit();
                var hits = Physics.RaycastAll(castRay, castDistance, castLayer);
                ///Try to find other hit point next to character transform
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].distance < dist && hits[i].collider.gameObject != collider.gameObject && !hits[i].collider.transform.IsChildOf(transform))
                    {
                        dist = hits[i].distance;
                        hit = hits[i];
                        canAimToHit = true;
                    }
                }
            }
            else canAimToHit = true;
            if (hit.collider && canAimToHit)
            {
                desiredAimPoint = hit.point;
            }

        }

        AimPosition = desiredAimPoint;
    }

    public override bool IsAimInputState(InputState state)
    {
        if (_isControlByPlayer)
        {
            return base.IsAimInputState(state);
        }
        else
        {
            switch (state)
            {
                case InputState.Button:
                    return _isAllieAiming;
                case InputState.ButtonDown:
                    return _isAllieAiming;
                case InputState.ButtonUp:
                    return false;
            }
            return false;
        }
    }

    public void TogglePlayerControl(bool isOn)
    {
        _isControlByPlayer = isOn;

        if (isOn)
        {
            _allieBehaviorTree.DisableBehavior();
        }
        else
        {
            _allieBehaviorTree.EnableBehavior();
        }
    }
}
