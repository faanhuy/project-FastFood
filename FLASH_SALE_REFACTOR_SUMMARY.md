# Flash Sale Refactoring Summary — Multi-Product Model

**Refactoring Date:** 2026-06-13  
**Status:** Completed (Backend)  
**Migration:** Pending database update

## Overview

The Flash Sale feature has been refactored from a 1:1 product-to-sale model to a 1:N multi-product model. Each flash sale event can now contain multiple products with independent pricing and stock management.

---

## Architecture Changes

### Before (Single-Product)
```
FlashSale (1) ──── ProductId, SalePrice, OriginalPrice, StockLimit, SoldCount
```

### After (Multi-Product)
```
FlashSale (1) ──── (many) FlashSaleItem
                         ├── ProductId
                         ├── SizeId (nullable)
                         ├── SalePrice
                         ├── OriginalPrice
                         ├── StockLimit
                         └── SoldCount
```

---

## API Contract Changes

### 1. Create Flash Sale

**Endpoint:** `POST /api/admin/flash-sales`

**Request Body (NEW):**
```json
{
  "name": "Summer Sale 2026",
  "startAt": "2026-07-01T00:00:00Z",
  "endAt": "2026-07-31T23:59:59Z",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "sizeId": null,
      "salePrice": 50000,
      "stockLimit": 100
    },
    {
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "sizeId": "123e4567-e89b-12d3-a456-426614174000",
      "salePrice": 75000,
      "stockLimit": 50
    }
  ]
}
```

**Response:** `200 Created`
```json
{
  "success": true,
  "data": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "name": "Summer Sale 2026",
    "startAt": "2026-07-01T00:00:00Z",
    "endAt": "2026-07-31T23:59:59Z",
    "isActive": true,
    "remainingSeconds": 2592000,
    "items": [
      {
        "flashSaleId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
        "productId": "550e8400-e29b-41d4-a716-446655440000",
        "productName": "Product A",
        "imageUrl": "https://example.com/image.jpg",
        "sizeId": null,
        "sizeLabel": null,
        "salePrice": 50000,
        "originalPrice": 100000,
        "stockLimit": 100,
        "soldCount": 0,
        "remainingStock": 100,
        "percentDiscount": 50.0
      },
      {
        "flashSaleId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
        "productId": "550e8400-e29b-41d4-a716-446655440001",
        "productName": "Product B",
        "imageUrl": "https://example.com/image2.jpg",
        "sizeId": "123e4567-e89b-12d3-a456-426614174000",
        "sizeLabel": "M",
        "salePrice": 75000,
        "originalPrice": 150000,
        "stockLimit": 50,
        "soldCount": 0,
        "remainingStock": 50,
        "percentDiscount": 50.0
      }
    ]
  },
  "message": null,
  "errors": null
}
```

### 2. Update Flash Sale

**Endpoint:** `PUT /api/admin/flash-sales/{id}`

**Request Body (NEW):** Same structure as Create (replaces all items)
```json
{
  "name": "Updated Summer Sale",
  "startAt": "2026-07-01T00:00:00Z",
  "endAt": "2026-08-31T23:59:59Z",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "sizeId": null,
      "salePrice": 60000,
      "stockLimit": 150
    }
  ]
}
```

**Response:** `200 OK` (same structure as Create response)

### 3. Get Flash Sales (Admin)

**Endpoint:** `GET /api/admin/flash-sales?page=1&pageSize=20&isActive=true`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
        "name": "Summer Sale 2026",
        "startAt": "2026-07-01T00:00:00Z",
        "endAt": "2026-07-31T23:59:59Z",
        "isActive": true,
        "remainingSeconds": 2592000,
        "items": [
          {
            "flashSaleId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
            "productId": "550e8400-e29b-41d4-a716-446655440000",
            "productName": "Product A",
            "imageUrl": "https://example.com/image.jpg",
            "sizeId": null,
            "sizeLabel": null,
            "salePrice": 50000,
            "originalPrice": 100000,
            "stockLimit": 100,
            "soldCount": 0,
            "remainingStock": 100,
            "percentDiscount": 50.0
          }
        ]
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20
  },
  "message": null,
  "errors": null
}
```

### 4. Get Active Flash Sales (Public)

**Endpoint:** `GET /api/flash-sales?page=1&pageSize=20`

**Response:** `200 OK` (same structure, filters only active sales with items in stock)

### 5. Delete Flash Sale

**Endpoint:** `DELETE /api/admin/flash-sales/{id}`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {},
  "message": "Flash sale deleted successfully.",
  "errors": null
}
```

---

## Key Changes for Frontend

### DTOs (TypeScript Types)

**Old FlashSaleDto:**
```typescript
interface FlashSaleDto {
  id: string;
  name: string;
  productId: string;
  productName: string;
  salePrice: number;
  originalPrice: number;
  stockLimit: number;
  soldCount: number;
  remainingStock: number;
  startAt: string;
  endAt: string;
  isActive: boolean;
  remainingSeconds: number;
  percentDiscount: number;
}
```

**New FlashSaleDto:**
```typescript
interface FlashSaleItemDto {
  flashSaleId: string;
  productId: string;
  productName: string;
  imageUrl?: string;
  sizeId?: string;
  sizeLabel?: string;
  salePrice: number;
  originalPrice: number;
  stockLimit: number;
  soldCount: number;
  remainingStock: number;
  percentDiscount: number;
}

interface FlashSaleDto {
  id: string;
  name: string;
  startAt: string;
  endAt: string;
  isActive: boolean;
  remainingSeconds: number;
  items: FlashSaleItemDto[];
}
```

### Service Updates Required

**flashSaleService.ts** changes:

```typescript
// Get active flash sales
export const flashSaleService = {
  getActiveFlashSales: async (page = 1, pageSize = 20): Promise<PagedResult<FlashSaleDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<FlashSaleDto>>>('/flash-sales', {
      params: { page, pageSize },
    });
    return data.data;
  },

  // Admin: Get all flash sales
  getAllFlashSales: async (page = 1, pageSize = 20, isActive?: boolean): Promise<PagedResult<FlashSaleDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<FlashSaleDto>>>('/admin/flash-sales', {
      params: { page, pageSize, isActive },
    });
    return data.data;
  },

  // Admin: Create flash sale
  createFlashSale: async (request: CreateFlashSaleRequest): Promise<FlashSaleDto> => {
    const { data } = await api.post<ApiResponse<FlashSaleDto>>('/admin/flash-sales', request);
    return data.data;
  },

  // Admin: Update flash sale
  updateFlashSale: async (id: string, request: UpdateFlashSaleRequest): Promise<FlashSaleDto> => {
    const { data } = await api.put<ApiResponse<FlashSaleDto>>(`/admin/flash-sales/${id}`, request);
    return data.data;
  },

  // Admin: Delete flash sale
  deleteFlashSale: async (id: string): Promise<void> => {
    await api.delete(`/admin/flash-sales/${id}`);
  }
};

export interface CreateFlashSaleItemRequest {
  productId: string;
  sizeId?: string;
  salePrice: number;
  stockLimit: number;
}

export interface CreateFlashSaleRequest {
  name: string;
  startAt: string;
  endAt: string;
  items: CreateFlashSaleItemRequest[];
}

export interface UpdateFlashSaleRequest {
  name: string;
  startAt: string;
  endAt: string;
  items: CreateFlashSaleItemRequest[];
}
```

### UI Component Updates

**Flash Sale Display (Public):**
- Iterate through `flashSale.items` instead of displaying single product
- Show product image, name, size label (if applicable) per item
- Display sale price, original price, discount percent per item
- Show remaining stock per item

**Flash Sale Admin (Create/Edit):**
- Change form to support multiple items
- Add dynamic form fields for items list
- Validate: at least one item required
- Validate: if product has sizes, size is required
- Show product selector + size selector (conditional) + prices + stock limit per item

**Example:** Display active flash sales as a carousel with multiple product cards per sale.

---

## Validation Rules (Important for Frontend)

### Create/Update Flash Sale

| Field | Rule | Error Message |
|-------|------|---------------|
| name | not empty | "Name cannot be empty" |
| startAt | datetime | "Invalid start time" |
| endAt | after startAt | "End time must be after start time" |
| items | count > 0 | "At least one item is required" |
| item.productId | exists, active | "Product not found or inactive" |
| item.salePrice | > 0 and < originalPrice | "Sale price must be positive and less than original price" |
| item.stockLimit | > 0 | "Stock limit must be positive" |
| item.sizeId | required if product.hasSize | "Size is required for products with sizes" |
| item.sizeId | active if provided | "Size is inactive" |
| items | no duplicate (productId, sizeId) | "Duplicate product/size combinations found" |

### Example Error Responses

```json
{
  "success": false,
  "data": null,
  "message": "Flash sale must contain at least one item.",
  "errors": null
}
```

```json
{
  "success": false,
  "data": null,
  "message": "Product is inactive and cannot have flash sales.",
  "errors": null
}
```

```json
{
  "success": false,
  "data": null,
  "message": "Product requires a size selection.",
  "errors": null
}
```

---

## Database Changes

### New Table: FlashSaleItems

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | uniqueidentifier | No | PK |
| FlashSaleId | uniqueidentifier | No | FK → FlashSales (Cascade) |
| ProductId | uniqueidentifier | No | FK → Products (Restrict) |
| SizeId | uniqueidentifier | Yes | FK → Sizes (SetNull) |
| SalePrice | decimal(18,2) | No | |
| OriginalPrice | decimal(18,2) | No | |
| StockLimit | int | No | |
| SoldCount | int | No | |
| CreatedAt | datetime2 | No | |
| CreatedBy | nvarchar | Yes | |
| UpdatedAt | datetime2 | Yes | |
| UpdatedBy | nvarchar | Yes | |

**Indexes:**
- IX_FlashSaleItems_FlashSaleId
- IX_FlashSaleItems_ProductId
- IX_FlashSaleItems_SizeId

### Modified Table: FlashSales

**Removed Columns:**
- ProductId
- SalePrice
- OriginalPrice
- StockLimit
- SoldCount

**Removed Indexes:**
- IX_FlashSales_ProductId

**New FK Relationship:**
- HasMany(Items) → FlashSaleItem

### Data Migration

Existing flash sales are automatically migrated:
- Each existing FlashSale row creates one FlashSaleItem
- The product details are copied to the new FlashSaleItem row
- SizeId is set to NULL for migrated items

---

## Migration & Deployment

### Prerequisites
- All backend code changes must be built successfully
- Database must be online

### Steps

1. **Stop the running application**
   ```bash
   # Stop backend server
   # Stop any background jobs/schedulers
   ```

2. **Apply migration**
   ```bash
   cd D:\Projects\Personal\SmartShop
   dotnet ef database update -p src/SmartShop.Infrastructure -s src/SmartShop.WebAPI
   ```

3. **Start the application**
   ```bash
   dotnet run --project src/SmartShop.WebAPI
   ```

4. **Verify migration**
   - Check FlashSaleItems table exists
   - Verify data was migrated from FlashSales
   - Test API endpoints

---

## Backward Compatibility

**None.** This is a breaking change. Frontend must be updated to:
- Send items array in create/update requests
- Handle items array in responses
- Update display logic to iterate items

---

## Files Modified/Created

### Domain Layer
- ✅ Created: `src/SmartShop.Domain/Entities/FlashSaleItem.cs`
- ✅ Modified: `src/SmartShop.Domain/Entities/FlashSale.cs`

### Application Layer
- ✅ Created: `src/SmartShop.Application/Features/FlashSales/FlashSaleItemDto.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/FlashSaleDto.cs`
- ✅ Created: `src/SmartShop.Application/Features/FlashSales/FlashSaleItemRequest.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/Commands/CreateFlashSale/CreateFlashSaleCommand.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/Commands/CreateFlashSale/CreateFlashSaleCommandHandler.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/Commands/UpdateFlashSale/UpdateFlashSaleCommand.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/Commands/UpdateFlashSale/UpdateFlashSaleCommandHandler.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/Queries/GetActiveFlashSales/GetActiveFlashSalesQueryHandler.cs`
- ✅ Modified: `src/SmartShop.Application/Features/FlashSales/Queries/GetFlashSalesAdmin/GetFlashSalesAdminQueryHandler.cs`
- ✅ Modified: `src/SmartShop.Application/Features/Orders/Commands/PlaceOrder/PlaceOrderCommandHandler.cs`

### Infrastructure Layer
- ✅ Created: `src/SmartShop.Infrastructure/Data/Configurations/FlashSaleItemConfiguration.cs`
- ✅ Modified: `src/SmartShop.Infrastructure/Data/Configurations/FlashSaleConfiguration.cs`
- ✅ Modified: `src/SmartShop.Infrastructure/Data/ApplicationDbContext.cs`
- ✅ Modified: `src/SmartShop.Infrastructure/Repositories/FlashSaleRepository.cs`
- ✅ Modified: `src/SmartShop.Infrastructure/Migrations/20260613114253_RefactorFlashSalesToMultiProduct.cs` (created)

### WebAPI Layer
- ✅ Modified: `src/SmartShop.WebAPI/Controllers/AdminFlashSalesController.cs`
- ✅ No changes: `src/SmartShop.WebAPI/Controllers/FlashSalesController.cs`

---

## Testing Checklist

### Manual Testing (Backend)

- [ ] Create flash sale with single product (no size)
- [ ] Create flash sale with product that has sizes (multiple items)
- [ ] Update flash sale (replace items)
- [ ] Delete flash sale
- [ ] Get active flash sales (filter by stock)
- [ ] Get admin flash sales (all statuses)
- [ ] Verify flash sale discount applied in PlaceOrder
- [ ] Verify flash sale stock incremented on order placement

### Frontend Testing (TODO)

- [ ] Create flash sale form accepts multiple items
- [ ] Update flash sale form pre-fills existing items
- [ ] Display active flash sales with product cards per item
- [ ] Display admin list with item count
- [ ] Validate required fields per item

---

## Notes

- All API responses use same structure as before (wrapped in ApiResponse<T>)
- Cache key prefix `flashsales:active:` still valid (filters by content)
- Background job (FlashSaleExpiryJob) works as before (deactivates expired sales)
- All size references load from Product.Sizes (no separate size API call needed)

---

**Created:** 2026-06-13  
**Refactored By:** Claude Backend Agent
