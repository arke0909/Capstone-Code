using Scripts.SkillSystem.Manage;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "playerInput", menuName = "SO/PlayerInput", order = 0)]
public class PlayerInputSO : ScriptableObject, Control.IPlayerActions, Control.IFarmingActions
{
    [SerializeField] private LayerMask whatIsGround;
    public event Action<Vector2> OnCursorMoved;
    public event Action<ActiveSlotType> OnSkillPressed;
    public event Action OnInteractPressed;
    public event Action OnInventoryPressed;
    public event Action OnCraftTreePressed;
    public event Action OnSkillTreePressed;
    public event Action OnBulletShowPressed;
    public event Action OnDownBulletListPressed;
    public event Action OnUpBulletListPressed;
    public event Action OnItemInteractPressed;
    public event Action OnReloadPressed;
    public event Action OnToggleUIPressed;
    public event Action OnMinimapPressed;
    public event Action<bool> OnCameraLockPressed;
    public event Action<int> OnItemUsePressed;
    public bool AimKey { get; private set; } = false;
    public bool SprintKey { get; private set; } = false;
    public bool AttackKey { get; private set; } = false;
    public bool CameraLock { get; private set; } = false;
    public Vector2 MovementKey { get; private set; }
    private Control _controls;

    private Vector3 _worldPosition;
    private Vector2 _screenPosition;

    private void OnEnable()
    {
        CameraLock = false;
        
        if (_controls == null)
        {
            _controls = new Control();
            _controls.Player.SetCallbacks(this);
            _controls.Farming.SetCallbacks(this);
        }

        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    public void SetPlayerInput(bool isActive)
    {
        _controls.Disable();

        if (isActive)
            _controls.Player.Enable();
        else
            _controls.Farming.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movementKey = context.ReadValue<Vector2>();
        MovementKey = movementKey;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _screenPosition = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            AttackKey = true;
        else if(context.canceled)
            AttackKey = false;
    }

    public Vector3 GetWorldPosition()
    {
        Camera mainCam = Camera.main; //Unity2022부터 내부 캐싱이 되서 그냥 써도 돼.
        Debug.Assert(mainCam != null, "No main camera in this scene");

        Ray cameraRay = mainCam.ScreenPointToRay(_screenPosition);
        if (Physics.Raycast(cameraRay, out RaycastHit hit, mainCam.farClipPlane, whatIsGround))
        {
            _worldPosition = hit.point;
        }

        return _worldPosition;
    }

    public GameObject GetGameObject()
    {
        Camera mainCam = Camera.main;
        Debug.Assert(mainCam != null, "No main camera in this scene");

        Ray cameraRay = mainCam.ScreenPointToRay(_screenPosition);
        if (Physics.Raycast(cameraRay, out RaycastHit hit, mainCam.farClipPlane))
        {
            return hit.transform.gameObject;
        }

        return null;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInteractPressed?.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            SprintKey = true;
        else if (context.canceled)
            SprintKey = false;
    }

    public void OnItemInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnItemInteractPressed?.Invoke();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInventoryPressed?.Invoke();
    }

    public void OnCraftTree(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnCraftTreePressed?.Invoke();
    }

    public void OnSkillTree(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnSkillTreePressed?.Invoke();
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
            _controls.Player.Enable();
        else
            _controls.Player.Disable();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
            AimKey = true;
        else if (context.canceled)
            AimKey = false;
    }

    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnSkillPressed?.Invoke(ActiveSlotType.Space);
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnReloadPressed?.Invoke();
    }

    public void OnShowBullet(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnBulletShowPressed?.Invoke();
    }

    public void OnDownBulletList(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnDownBulletListPressed?.Invoke();
    }

    public void OnUpBulletList(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnUpBulletListPressed?.Invoke();
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnSkillPressed?.Invoke(ActiveSlotType.C);
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnSkillPressed?.Invoke(ActiveSlotType.E);
    }

    public void OnCameraLock(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CameraLock = !CameraLock;
            OnCameraLockPressed?.Invoke(CameraLock);
        }
    }

    public void OnToggleUI(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnToggleUIPressed?.Invoke();
    }

    public void OnMinimap(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnMinimapPressed?.Invoke();
    }

    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        Vector2 cursorMovement = context.ReadValue<Vector2>();
        OnCursorMoved?.Invoke(cursorMovement);
    }

    public void OnItem1(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(0);
    }

    public void OnItem2(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(1);
    }

    public void OnItem3(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(2);
    }

    public void OnItem4(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(3);
    }

    public void OnItem5(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(4);
    }

    public void OnItem6(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(5);
    }

    public void OnItem7(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(6);
    }

    public void OnItem8(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnItemUsePressed?.Invoke(7);
    }

    public void OnSkill4(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnSkillPressed?.Invoke(ActiveSlotType.Q);
    }
}