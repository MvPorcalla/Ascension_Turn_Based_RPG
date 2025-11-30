// -------------------------------
// DraggableItemSlot.cs - FIXED: Reliable mobile drag & drop with long-press
// -------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class DraggableItemSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Long Press Settings")]
    [SerializeField] private float longPressDuration = 0.7f; // Hold time to start drag
    [SerializeField] private float tapTimeThreshold = 0.3f; // Max time for tap
    
    [Header("Drag Settings")]
    [SerializeField] private float dragAlpha = 0.6f;
    [SerializeField] private float dragUpdateInterval = 0.01f; // How often to update position
    
    [Header("Haptic Feedback")]
    [SerializeField] private bool enableHaptics = true;
    
    // Data
    private ItemInstance itemInstance;
    private ItemBaseSO itemData;
    private Action onTapCallback;
    
    // Long-press tracking
    private Coroutine longPressCoroutine;
    private bool isLongPressActive = false;
    private Vector2 pointerDownPosition;
    private float pointerDownTime;
    
    // Drag state
    private bool isDragging = false;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private GameObject dragGhost;
    private Coroutine dragUpdateCoroutine;
    
    // Drop detection
    private InventoryDropZone currentDropZone;
    private InventoryDropZone lastValidDropZone;
    
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void Setup(ItemBaseSO data, ItemInstance instance, Action onTap)
    {
        itemData = data;
        itemInstance = instance;
        onTapCallback = onTap;
    }
    
    #region Pointer Handlers
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (itemInstance == null || itemData == null)
            return;
        
        // Can't drag equipped items
        if (itemInstance.isEquipped)
        {
            Debug.Log("[DraggableItemSlot] Cannot drag equipped items");
            PlayHaptic(HapticType.Warning);
            return;
        }
        
        // Record touch start
        pointerDownPosition = eventData.position;
        pointerDownTime = Time.time;
        isLongPressActive = false;
        
        // Visual feedback
        transform.localScale = Vector3.one * 0.95f;
        PlayHaptic(HapticType.Selection);
        
        // Start long-press detection
        longPressCoroutine = StartCoroutine(DetectLongPress(eventData));
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // Stop long-press detection
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }
        
        // Reset scale
        transform.localScale = Vector3.one;
        
        if (itemInstance == null || itemData == null)
            return;
        
        // If we're dragging, handle drop
        if (isDragging)
        {
            EndDragging(eventData.position);
            return;
        }
        
        // Check if this was a tap (didn't become long-press)
        if (!isLongPressActive)
        {
            float holdTime = Time.time - pointerDownTime;
            float distance = Vector2.Distance(pointerDownPosition, eventData.position);
            
            bool wasTap = holdTime < tapTimeThreshold && distance < 30f;
            
            if (wasTap)
            {
                HandleTap();
            }
        }
    }
    
    #endregion
    
    #region Long Press Detection
    
    private IEnumerator DetectLongPress(PointerEventData eventData)
    {
        float elapsed = 0f;
        Vector2 startPosition = eventData.position;
        
        while (elapsed < longPressDuration)
        {
            elapsed += Time.deltaTime;
            
            // Check if finger moved too much (cancel long-press if dragged early)
            float distance = Vector2.Distance(startPosition, Input.mousePosition);
            if (distance > 30f)
            {
                yield break; // Cancel long-press
            }
            
            yield return null;
        }
        
        // Long-press successful!
        isLongPressActive = true;
        StartDragging(startPosition);
    }
    
    #endregion
    
    #region Drag Logic
    
    private void StartDragging(Vector2 startPosition)
    {
        isDragging = true;
        
        // Store original position
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        
        // Create drag ghost
        CreateDragGhost();
        dragGhost.transform.position = startPosition;
        
        // Make original semi-transparent
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        
        // Start continuous drag update
        dragUpdateCoroutine = StartCoroutine(UpdateDragPosition());
        
        // Haptic feedback
        PlayHaptic(HapticType.LightImpact);
        
        Debug.Log($"[DraggableItemSlot] Started dragging {itemData.itemName} (long-press)");
    }
    
    private IEnumerator UpdateDragPosition()
    {
        WaitForSeconds wait = new WaitForSeconds(dragUpdateInterval);
        
        while (isDragging)
        {
            Vector2 currentPointerPosition = Input.mousePosition;
            
            // Update ghost position
            if (dragGhost != null)
            {
                dragGhost.transform.position = currentPointerPosition;
            }
            
            // Raycast to detect drop zones
            DetectDropZone(currentPointerPosition);
            
            yield return wait;
        }
    }
    
    private void DetectDropZone(Vector2 screenPosition)
    {
        // Create pointer event data for raycast
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        
        // Raycast to find what's under the pointer
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        
        InventoryDropZone foundZone = null;
        
        // Find the first drop zone in results
        foreach (var result in results)
        {
            InventoryDropZone zone = result.gameObject.GetComponent<InventoryDropZone>();
            if (zone != null)
            {
                foundZone = zone;
                break;
            }
        }
        
        // Update drop zone highlighting
        if (foundZone != currentDropZone)
        {
            // Exit previous zone
            if (currentDropZone != null)
            {
                currentDropZone.OnDragExit();
            }
            
            // Enter new zone
            currentDropZone = foundZone;
            
            if (currentDropZone != null)
            {
                bool canDrop = CanDropInZone(currentDropZone);
                currentDropZone.OnDragEnter(itemInstance, itemData, canDrop);
                
                if (canDrop)
                {
                    lastValidDropZone = currentDropZone;
                }
            }
        }
    }
    
    private void EndDragging(Vector2 dropPosition)
    {
        isDragging = false;
        
        // Stop drag update coroutine
        if (dragUpdateCoroutine != null)
        {
            StopCoroutine(dragUpdateCoroutine);
            dragUpdateCoroutine = null;
        }
        
        // Restore alpha
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Destroy ghost
        if (dragGhost != null)
        {
            Destroy(dragGhost);
            dragGhost = null;
        }
        
        // Exit current drop zone
        if (currentDropZone != null)
        {
            currentDropZone.OnDragExit();
        }
        
        // Attempt drop
        if (lastValidDropZone != null && CanDropInZone(lastValidDropZone))
        {
            HandleDrop(lastValidDropZone);
        }
        else
        {
            // Invalid drop - return to original position
            ReturnToOriginalPosition();
            PlayHaptic(HapticType.Warning);
            Debug.Log("[DraggableItemSlot] Invalid drop - returned to original position");
        }
        
        // Reset drop zone tracking
        currentDropZone = null;
        lastValidDropZone = null;
    }
    
    private bool CanDropInZone(InventoryDropZone zone)
    {
        ItemLocation targetLocation = zone.LocationType;
        ItemLocation currentLocation = GetCurrentLocation();
        
        // Can't drop in same location
        if (currentLocation == targetLocation)
            return false;
        
        // Check if target has space
        BagInventory inventory = InventoryManager.Instance.Inventory;
        
        bool hasSpace = targetLocation switch
        {
            ItemLocation.Bag => inventory.HasBagSpace(),
            ItemLocation.Pocket => inventory.HasPocketSpace(),
            ItemLocation.Storage => true,
            _ => false
        };
        
        if (!hasSpace)
            return false;
        
        // 🚨 CRITICAL: Weapons/Gear cannot go into Pocket
        if (targetLocation == ItemLocation.Pocket)
        {
            bool isWeaponOrGear = itemData.itemType == ItemType.Weapon || 
                                  itemData.itemType == ItemType.Gear;
            
            if (isWeaponOrGear)
            {
                Debug.Log($"[DraggableItemSlot] {itemData.itemName} cannot be placed in Pocket (weapons/gear not allowed)");
                return false;
            }
        }
        
        return true;
    }
    
    private void HandleDrop(InventoryDropZone dropZone)
    {
        ItemLocation targetLocation = dropZone.LocationType;
        ItemLocation currentLocation = GetCurrentLocation();
        
        bool success = MoveItem(currentLocation, targetLocation);
        
        if (success)
        {
            PlayHaptic(HapticType.Success);
            Debug.Log($"[DraggableItemSlot] Successfully moved {itemData.itemName} from {currentLocation} to {targetLocation}");
        }
        else
        {
            ReturnToOriginalPosition();
            PlayHaptic(HapticType.Warning);
        }
    }
    
    private void HandleTap()
    {
        PlayHaptic(HapticType.Selection);
        onTapCallback?.Invoke();
        Debug.Log($"[DraggableItemSlot] Tapped {itemData.itemName}");
    }
    
    #endregion
    
    #region Item Movement
    
    private bool MoveItem(ItemLocation from, ItemLocation to)
    {
        BagInventory inventory = InventoryManager.Instance.Inventory;
        
        bool success = to switch
        {
            ItemLocation.Bag => inventory.MoveToBag(itemInstance, itemInstance.quantity),
            ItemLocation.Pocket => inventory.MoveToPocket(itemInstance, itemInstance.quantity),
            ItemLocation.Storage => inventory.MoveToStorage(itemInstance, itemInstance.quantity),
            _ => false
        };
        
        return success;
    }
    
    private ItemLocation GetCurrentLocation()
    {
        if (itemInstance.isInBag) return ItemLocation.Bag;
        if (itemInstance.isInPocket) return ItemLocation.Pocket;
        return ItemLocation.Storage;
    }
    
    #endregion
    
    #region Visual Feedback
    
    private void CreateDragGhost()
    {
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(canvas.transform, false);
        dragGhost.transform.SetAsLastSibling();
        
        // Copy image
        Image ghostImage = dragGhost.AddComponent<Image>();
        ghostImage.sprite = itemIcon.sprite;
        ghostImage.color = itemIcon.color;
        ghostImage.raycastTarget = false;
        
        // Match size (slightly bigger)
        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        RectTransform originalRect = GetComponent<RectTransform>();
        ghostRect.sizeDelta = originalRect.sizeDelta * 1.2f;
        
        // Add transparency
        CanvasGroup ghostGroup = dragGhost.AddComponent<CanvasGroup>();
        ghostGroup.alpha = 0.85f;
        ghostGroup.blocksRaycasts = false;
        
        // Add outline
        Outline outline = dragGhost.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3, -3);
    }
    
    private void ReturnToOriginalPosition()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
    
    #endregion
    
    #region Haptic Feedback
    
    private void PlayHaptic(HapticType type)
    {
        if (!enableHaptics)
            return;
        
        #if UNITY_ANDROID || UNITY_IOS
        switch (type)
        {
            case HapticType.Selection:
            case HapticType.LightImpact:
                Handheld.Vibrate();
                break;
                
            case HapticType.Success:
                StartCoroutine(HapticPattern(new float[] { 0f, 0.05f, 0.05f }));
                break;
                
            case HapticType.Warning:
                StartCoroutine(HapticPattern(new float[] { 0f, 0.15f }));
                break;
        }
        #endif
    }
    
    private IEnumerator HapticPattern(float[] timings)
    {
        foreach (float delay in timings)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            
            Handheld.Vibrate();
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Cleanup coroutines
        if (longPressCoroutine != null)
            StopCoroutine(longPressCoroutine);
        
        if (dragUpdateCoroutine != null)
            StopCoroutine(dragUpdateCoroutine);
        
        // Cleanup ghost
        if (dragGhost != null)
            Destroy(dragGhost);
    }
}