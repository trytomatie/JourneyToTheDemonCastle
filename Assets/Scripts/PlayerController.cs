using Cinemachine;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static PlayerController;

public class PlayerController : MonoBehaviour
{
    // Character Movement Properties
    [SerializeField] private float maxMovementSpeed = 6;
    [SerializeField] private float rotationSpeed = 40;
    public float ySpeed = 0;
    private float gravity = 1f;
    private float currentSpeed;
    public Vector3 movementDirection;
    public Vector3 rootMotionMotion;
    public LayerMask groundLayer;
    // References
    public CharacterController characterController;
    public CinemachineVirtualCamera vCam;
    public ParticleSystem walkDust;
    public InteractionManager interactionManager;
    public Transform weaponPivot;
    public Transform staffTip;
    private StatusManager sm;
    private Inventory inventory;

    [Header("HitBoxes")]
    public GameObject[] hitBoxes;
    

    public Animator anim;
    private Transform cameraTransform;

    public Vector3 lastSolidGround = Vector3.zero;

    [Header("Air Stepping")]
    public float airStepDistance = 5;
    public AnimationCurve airStepCurve;
    public float airStepDuration = 0.5f;


    // States
    public enum PlayerState
    {
        Controlling,
        InWater,
        Running,
        Attacking,
        AirStepping
    }
    public PlayerState currentPlayerState = PlayerState.Controlling;

    private State[] states = new State[5];

    // Start is called before the first frame update
    void Awake()
    {
        states[(int)PlayerState.Controlling] = new PlayerStateControlling();
        states[(int)PlayerState.InWater] = new PlayerStateInWater();
        states[(int)PlayerState.AirStepping] = new PlayerStateAirStepping();
        states[(int)currentPlayerState].OnEnter(this);

        inventory = GetComponent<Inventory>();
        cameraTransform = Camera.main.transform;
        GameManager.Instance.player = gameObject;
        sm = GetComponent<StatusManager>();
        SwitchHotbarItem(0);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey1.performed += ctx => SwitchHotbarItem(0);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey2.performed += ctx => SwitchHotbarItem(1);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey3.performed += ctx => SwitchHotbarItem(2);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey4.performed += ctx => SwitchHotbarItem(3);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey5.performed += ctx => SwitchHotbarItem(4);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey6.performed += ctx => SwitchHotbarItem(5);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey7.performed += ctx => SwitchHotbarItem(6);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed += ctx => HandleItemUsage(true);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.canceled += ctx => HandleItemUsage(false);
    }

    public void SwitchHotbarItem(int index)
    {
        inventory.SwitchHotbarItem(index);
    }

    public void SwitchPlayerState(PlayerState newState)
    {
        SwitchPlayerState(newState , false);
    }

    public void SwitchPlayerState(PlayerState newState, bool force)
    {
        if(!force)
        {
            CurrentPlayerState = newState;
        }
        else
        {

            if(CurrentPlayerState == newState)
            {
                states[(int)currentPlayerState].OnExit(this);
                states[(int)currentPlayerState].OnEnter(this);
            }
            CurrentPlayerState = newState;
        }

    }

    public void SwitchPlayerState()
    {
        CurrentPlayerState = PlayerState.Controlling;
    }

    // Update is called once per frame
    void Update()
    {
        states[(int)currentPlayerState].OnUpdate(this);
        CheckForPlayerVoidOut();
    }

    public void HandleInteraction()
    {
        if(InputSystem.GetInputActionMapPlayer().Player.Interact.WasPressedThisFrame())
        {
            print("Interacting");
            interactionManager.Interact();

        }
    }

    public void HandleGravity()
    {
        bool isGrounded = IsGrounded();
        if(isGrounded &&  ySpeed <= 0.2f)
        {
            ySpeed = 0;
            lastSolidGround = transform.position;
        }
        else
        {
            ySpeed += Physics.gravity.y * gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, ySpeed, 0) * Time.deltaTime);
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + new Vector3(0,0.05f,0), Vector3.down, 0.1f, groundLayer);
    }

    public void Rotation()
    {
        // Rotate the character to movement direction
        if (movementDirection != Vector3.zero)
        {

            Quaternion targetCharacterRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, rotationSpeed * Time.deltaTime);



        }
        return;
        if (!anim.GetBool("attack"))
        {

        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            float hitDistance;

            if (groundPlane.Raycast(ray, out hitDistance))
            {
                Vector3 cursorPosition = ray.GetPoint(hitDistance);

                Vector3 direction = cursorPosition - transform.position;
                direction.Normalize();
                Quaternion targetCharacterRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, 10000 * Time.deltaTime);
            }
        }
    }

    public void Animations()
    {
        anim.SetFloat("speed", currentSpeed);
    }

    public void Movement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        movementDirection.Normalize();

        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        bool shouldWalk = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        currentSpeed = shouldWalk ? inputMagnitude * 0.333f : inputMagnitude;
        if (anim.GetBool("attack"))
        {
            currentSpeed *= 0.3f;
        }
        if(currentSpeed > 0.5f)
        {
            walkDust.Play();
        }
        else
        {
            walkDust.Stop();
        }

        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up)
    * movementDirection;

        Vector3 finalMovementDirection = movementDirection * currentSpeed;
        finalMovementDirection.y = 0;
        characterController.Move((finalMovementDirection * maxMovementSpeed * Time.deltaTime) + rootMotionMotion);

        rootMotionMotion = Vector3.zero;

    }
    public void HandleAttack(bool handleAttack)
    {
        if (!EventSystem.current.IsPointerOverGameObject() && !BuildingManager.instance.PlaceBuildingMode && handleAttack)
        {
            anim.SetBool("attack", true);
        }
        else
        {
            anim.SetBool("attack", false);
        }
    }

    private GameObject staffVFXRef = null;
    private float attackChargeTime = 1;
    private float attackChargeTimer = 0;
    public void HandleStaffCharge(bool isChargeing)
    {
        if (!BuildingManager.instance.PlaceBuildingMode && isChargeing)
        {
            if(anim.GetBool("chrageStaff")== false)
            {
                staffVFXRef = VFXManager.Instance.PlayFeedback(4, staffTip);
            }
            attackChargeTimer += Time.deltaTime;
            RotationDuringCast();
            anim.SetBool("chrageStaff", true);
        }
        else
        {
            if(attackChargeTimer >= attackChargeTime)
            {
                FireMagicAttack();
                attackChargeTimer = 0;
            }
            if (staffVFXRef != null)
            {
                staffVFXRef.GetComponent<ParticleSystem>().Stop();
                Destroy(staffVFXRef,3);
            }
            anim.SetBool("chrageStaff", false);
        }
    }

    private void FireMagicAttack()
    {
        Instantiate(hitBoxes[1], hitBoxes[1].transform.position, hitBoxes[1].transform.rotation).SetActive(true);
        GameObject vfx = VFXManager.Instance.PlayFeedback(5, staffTip);
        Destroy(vfx,11);
    }

    public void RotationDuringCast()
    {
        Vector3 mousePosition = Input.mousePosition;
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        groundPlane.SetNormalAndPosition(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        float hitDistance;

        if (groundPlane.Raycast(ray, out hitDistance))
        {
            Vector3 cursorPosition = ray.GetPoint(hitDistance);

            Vector3 direction = cursorPosition - transform.position;
            direction.Normalize();
            Quaternion targetCharacterRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, 10000 * Time.deltaTime);
        }
    }

    public void HandleItemUsage(bool isUsing)
    {
        if (isUsing)
        {
            inventory.CurrentHotbarItem?.GetItemInteractionEffects.OnUsePerformed(gameObject, inventory.CurrentHotbarItem);
        }
        else
        {
            inventory.CurrentHotbarItem?.GetItemInteractionEffects.OnUseCancelled(gameObject, inventory.CurrentHotbarItem);
        }
    }

    public void RespawnAtLastSolidGround()
    {
        characterController.enabled = false;
        transform.position = lastSolidGround;
        characterController.enabled = true;
    }

    public void TeleportToDungeonLayer(bool enter)
    {
        print(enter);
        //vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 0;
        //vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 0;
        //vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;
        if (enter)
        {
            GameManager.Instance.overWorld.transform.position = new Vector3(0, -50, 0);
            GameManager.Instance.dungeonWorld.transform.position = new Vector3(0, 0, 0);
        }
        else
        {
            GameManager.Instance.overWorld.transform.position = new Vector3(0, 0, 0);
            GameManager.Instance.dungeonWorld.transform.position = new Vector3(0, -50, 0);
        }
        foreach(EnemyAI enemy in EnemyAI.enemyAIList)
        {
            if(enemy.transform.position.y <-20)
            {
                enemy.gameObject.SetActive(false);
            }
            else
            {
                enemy.gameObject.SetActive(true);
            }
            enemy.agent.enabled = false;
            enemy.agent.enabled = true;
        }
       // Invoke("ResetCamera", 0.1f);
    }

    private void ResetCamera()
    {
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 1;
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 1;
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 1;
    }

    public void AirStep(Vector3 airStepDirection,float delta)
    {
        if(airStepDirection == Vector3.zero)
        {
            return;
        }
        float airStepDistance = this.airStepDistance;
        float airStepCurveValue = airStepCurve.Evaluate(delta / airStepDuration);
        Vector3 airStepVector = airStepDirection * airStepDistance * airStepCurveValue;
        characterController.Move(airStepVector * Time.deltaTime);
    }

    public void CheckAirStepConditions(InputAction.CallbackContext ctx)
    {
        if(movementDirection != Vector3.zero)
        {
            SwitchPlayerState(PlayerState.AirStepping,true);
        }
    }

    public void CheckForPlayerVoidOut()
    {
        if(transform.position.y < -15)
        {
            CurrentPlayerState = PlayerState.InWater;
        }
    }

    public PlayerState CurrentPlayerState { get => currentPlayerState;
        set
        {
            if(currentPlayerState != value)
            {
                states[(int)currentPlayerState].OnExit(this);
                states[(int)value].OnEnter(this);
                currentPlayerState = value;
            }

        }
    }

    public void ItemUsage()
    {
        inventory.CurrentHotbarItem?.GetItemInteractionEffects.OnUse(gameObject, inventory.CurrentHotbarItem);
    }
}

public interface State
{
    void OnEnter(PlayerController pc);
    void OnExit(PlayerController pc);
    void OnUpdate(PlayerController pc);
}

public class PlayerStateControlling : State
{
    public void OnEnter(PlayerController pc)
    {
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed += pc.CheckAirStepConditions;
    }

    public void OnExit(PlayerController pc)
    {
        pc.walkDust.Stop();
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed -= pc.CheckAirStepConditions;
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.Movement();
        pc.Rotation();
        pc.Animations();
        pc.ItemUsage();
        pc.HandleGravity();
        pc.HandleInteraction();
    }
}

public class PlayerStateInWater : State
{
    private Transform initialFollowTarget;
    public void OnEnter(PlayerController pc)
    {
        pc.Invoke("RespawnAtLastSolidGround", 1.5f);
        pc.Invoke("SwitchPlayerState", 1.8f);
        initialFollowTarget = pc.vCam.Follow;
        pc.vCam.Follow = null;
    }

    public void OnExit(PlayerController pc)
    {
        pc.vCam.Follow = initialFollowTarget;
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.Animations();
        pc.HandleGravity();
    }
}

public class PlayerStateAirStepping : State
{
    private float onEnterTime;
    private Vector3 facingDirection;
    private float airStepInputLockoutTime = 0.9f; // Percentage of the airStepDuration
    public void OnEnter(PlayerController pc)
    {
        AudioManager.PlaySound(pc.transform.position, SoundType.Player_Dash);
        VFXManager.Instance.PlayFeedback(0, pc.transform);
        onEnterTime = Time.time + pc.airStepDuration;
        facingDirection = pc.movementDirection;
        if(facingDirection == Vector3.zero)
        {
            facingDirection = pc.transform.forward;
        }
        pc.anim.SetFloat("speed",0);
        pc.StartCoroutine(InputLockout(pc));
    }

    IEnumerator InputLockout(PlayerController pc)
    {
        yield return new WaitForSeconds(airStepInputLockoutTime * pc.airStepDuration);
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed += pc.CheckAirStepConditions;
    }

    public void OnExit(PlayerController pc)
    {
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed -= pc.CheckAirStepConditions;
        pc.Movement();
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.AirStep(facingDirection,pc.airStepDuration - (onEnterTime - Time.time));
        pc.ItemUsage();
        if (Time.time > onEnterTime)
        {
            pc.SwitchPlayerState(PlayerState.Controlling);

        }
    }
}
