// -------------------------------
// InventoryDropZone.cs - Enhanced drop zone with reliable detection
// -------------------------------
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InventoryDropZone : MonoBehaviour
{
    [Header("Drop Zone Settings")]
    [SerializeField] private ItemLocation locationType;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f); // Transparent
    [SerializeField] private Color validHoverColor = new Color(0.3f, 1f, 0.3f, 0.3f); // Green
    [SerializeField] private Color invalidHoverColor = new Color(1f, 0.3f, 0.3f, 0.3f); // Red
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    [Header("Optional Border")]
    [SerializeField] private Outline outline;
    [SerializeField] private bool showOutlineOnHover = true;
    
    private Image backgroundImage;
    private bool isHovering = false;
    private bool canAcceptDrop = false;
    private float pulseTimer = 0f;
    
    public ItemLocation LocationType => locationType;
    
    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        backgroundImage.color = normalColor;
        backgroundImage.raycastTarget = true; // CRITICAL: Must be true for raycasting!
        
        // Setup outline if available
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }
        
        if (outline != null && showOutlineOnHover)
        {
            outline.enabled = false;
        }
    }
    
    private void Update()
    {
        // Pulse effect when hovering
        if (isHovering)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmount;
            
            Color targetColor = canAcceptDrop ? validHoverColor : invalidHoverColor;
            Color pulsedColor = targetColor + new Color(pulse, pulse, pulse, 0);
            backgroundImage.color = pulsedColor;
        }
    }
    
    /// <summary>
    /// Called when item enters this drop zone (from DraggableItemSlot)
    /// </summary>
    public void OnDragEnter(ItemInstance item, ItemBaseSO itemData, bool canDrop)
    {
        isHovering = true;
        canAcceptDrop = canDrop;
        pulseTimer = 0f;
        
        // Set color based on validity
        Color targetColor = canDrop ? validHoverColor : invalidHoverColor;
        backgroundImage.color = targetColor;
        
        // Show outline if enabled
        if (outline != null && showOutlineOnHover)
        {
            outline.enabled = true;
            outline.effectColor = canDrop ? Color.green : Color.red;
        }
        
        Debug.Log($"[InventoryDropZone] Item entered {locationType} - Valid: {canDrop}");
    }
    
    /// <summary>
    /// Called when item exits this drop zone (from DraggableItemSlot)
    /// </summary>
    public void OnDragExit()
    {
        isHovering = false;
        canAcceptDrop = false;
        backgroundImage.color = normalColor;
        
        // Hide outline
        if (outline != null && showOutlineOnHover)
        {
            outline.enabled = false;
        }
    }
    
    /// <summary>
    /// Check if this zone can accept a drop (called from DraggableItemSlot)
    /// </summary>
    public bool CanAcceptItem(ItemInstance item, ItemBaseSO itemData)
    {
        BagInventory inventory = InventoryManager.Instance.Inventory;
        
        // Check capacity
        bool hasSpace = locationType switch
        {
            ItemLocation.Bag => inventory.HasBagSpace(),
            ItemLocation.Pocket => inventory.HasPocketSpace(),
            ItemLocation.Storage => true, // Unlimited
            _ => false
        };
        
        if (!hasSpace)
            return false;
        
        // 🚨 CRITICAL: Pocket restrictions
        if (locationType == ItemLocation.Pocket)
        {
            // Only allow consumables, materials, and misc items in pocket
            bool isAllowedType = itemData.itemType == ItemType.Consumable ||
                                 itemData.itemType == ItemType.Material ||
                                 itemData.itemType == ItemType.Misc;
            
            if (!isAllowedType)
            {
                Debug.Log($"[InventoryDropZone] Pocket rejected {itemData.itemName} (type: {itemData.itemType})");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get human-readable location name for UI messages
    /// </summary>
    public string GetLocationName()
    {
        return locationType switch
        {
            ItemLocation.Bag => "Bag",
            ItemLocation.Pocket => "Pocket",
            ItemLocation.Storage => "Storage",
            _ => "Unknown"
        };
    }
}