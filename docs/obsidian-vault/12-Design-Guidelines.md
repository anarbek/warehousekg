# 12 — Design Guidelines

## Icon Standards

All action icons use DevExtreme's built-in icon set. Never use raw Unicode symbols (✕, ✎, etc.) for action buttons.

### Delete
- **Icon**: `icon="trash"` — standard trash bin
- **Detail pages** (header toolbar): `stylingMode="outlined"`, `type="danger"`, with `text="Удалить"`
- **List rows** (inline): `stylingMode="text"`, `hint="Delete"` (no text, icon only)
- **Form line removal**: `stylingMode="text"`, `type="danger"`

### Edit
- **Icon**: `icon="edit"` — pencil
- **Detail pages** (header toolbar): `stylingMode="outlined"`, with `text="Редактировать"` or `text="Изменить"`
- **List rows** (inline): `stylingMode="text"`, `hint="Edit"` (no text, icon only)

### Cancel / Close
- **Icon**: `icon="close"` — X mark
- **Workflow cancel** (e.g., "Отменить заказ"): `stylingMode="outlined"`, `type="danger"`, with `text="Отменить"`
- **Form dismiss** (e.g., "Отмена" back button): `stylingMode="outlined"` (no icon needed, or `icon="close"`)
- **Popup close**: `stylingMode="text"`, `text="Отмена"`

### Workflow / Status Transitions
- **Confirm**: `icon="check"`, `type="default"`, `text="Подтвердить"`
- **Start route**: `icon="play"`, `type="default"`, `text="Старт"`
- **Complete**: `icon="check"`, `type="default"`, `text="Завершить"`
- **Ship**: `icon="export"`, `type="default"`, `text="Отгрузить"`

### Sidebar Navigation Icons

Use DevExtreme TreeView with `icon` property on each item. **`dx-tree-view` renders icons automatically** — no extra attribute needed.

**Verified DevExtreme icon names** (from `dx-icon-{name}` font):
`home`, `product`, `hierarchy`, `import`, `export`, `edit`, `fields`, `preferences`, `info`, `user`, `group`, `chart`, `car`, `map`, `globe`, `cart`, `event`, `clock`, `filter`, `key`, `close`, `trash`, `check`, `add`, `save`, `search`, `menu`, `refresh`, `runner`, `tel`, `tip`, `plus`, `minus`, `more`, `overflow`, `columnfield`, `rowfield`, `fieldset`

**Invalid icon names** (not in DevExtreme font — will render blank):
`ruler`, `todolist`, `box`, `movetofolder`, `checklist`, `detailslayout`, `task`, `card`

### Common Patterns

```html
<!-- Inline edit in data grid cell -->
<dx-button icon="edit" hint="Edit" stylingMode="text" (onClick)="..."/>

<!-- Inline delete in data grid cell -->
<dx-button icon="trash" hint="Delete" stylingMode="text" (onClick)="..."/>

<!-- Detail page header: edit -->
<dx-button text="Редактировать" icon="edit" stylingMode="outlined" [routerLink]="['edit']"/>

<!-- Detail page header: delete -->
<dx-button text="Удалить" icon="trash" type="danger" stylingMode="outlined" (onClick)="delete()"/>

<!-- Detail page header: cancel workflow -->
<dx-button text="Отменить" icon="close" type="danger" stylingMode="outlined" (onClick)="cancel()"/>

<!-- Form dismiss -->
<dx-button text="Отмена" icon="close" stylingMode="outlined" (onClick)="cancel()"/>
```

### Anti-Patterns (DO NOT USE)
- ❌ Raw Unicode: `✕`, `✎`, `✕ Отменить`
- ❌ `icon="trash"` for cancel actions (trash = delete only)
- ❌ Plain `<button>` with CSS classes for icon actions — use `<dx-button>` with DevExtreme icons
- ❌ Invalid DevExtreme icon names like `ruler`, `task`, `checklist`, `box`, `todolist`, `detailslayout`, `movetofolder`

---

## Detail Page Layout

Follow DevExtreme best practices for all detail/read-only pages. Reference implementation: `vehicle-detail`.

### Header
```html
<dx-toolbar>
  <dxi-item location="before">
    <h1 class="page__title">{{ entity.name }}</h1>
  </dxi-item>
  <dxi-item location="after">
    <dx-button text="Изменить" icon="edit" stylingMode="outlined" [routerLink]="['edit']"/>
  </dxi-item>
  <dxi-item location="after">
    <dx-button text="Назад" icon="arrowleft" stylingMode="text" routerLink="/list"/>
  </dxi-item>
</dx-toolbar>
```

### Status row
```html
<div class="status-row">
  <span class="status-badge status-badge--active">Активен</span>
  <span class="type">Type · Category</span>
</div>
```

### Tabs
```html
<dx-tabs [dataSource]="tabs" [selectedIndex]="activeTabIndex()" (onSelectionChanged)="onTabChange($event)"/>
```
```typescript
protected readonly tabs = [
  { id: 'info', text: 'Информация', icon: 'info' },
  { id: 'history', text: 'История', icon: 'event' }
];
protected readonly activeTab = signal<'info' | 'history'>('info');
```

### Read-only forms (info tab)
```html
<div class="cards">
  <div class="dx-card card">
    <h3 class="dx-card__title">Section Title</h3>
    <dx-form [formData]="entity" [colCount]="1" [readOnly]="true" [showColonAfterLabel]="false" labelLocation="top">
      <dxi-item dataField="field1" [label]="{text:'Label'}" />
      <dxi-item dataField="date" [label]="{text:'Date'}" [editorType]="'dxDateBox'" [editorOptions]="{displayFormat:'dd.MM.yyyy'}" />
    </dx-form>
  </div>
</div>
```
- Use `colCount="1"` with `labelLocation="top"` for readable single-column layout.
- Cards use CSS grid: `grid-template-columns: repeat(auto-fit, minmax(380px, 1fr))`

### Data grids
```html
<dx-data-grid [dataSource]="..." [showBorders]="true" [hoverStateEnabled]="true" [columnAutoWidth]="true">
  <!-- columns -->
</dx-data-grid>
```

### Popups
```html
<dx-popup [(visible)]="popup" title="Title" [width]="520" [height]="'auto'">
  <div *dxTemplate="let data of 'content'">
    <dx-form [formData]="form" [colCount]="2">
      <!-- form items -->
    </dx-form>
    <!-- MUST be INSIDE the content template -->
    <div class="popup-actions">
      <dx-button text="Сохранить" type="default" stylingMode="contained" (onClick)="save()"/>
      <dx-button text="Отмена" stylingMode="text" (onClick)="close()"/>
    </div>
  </div>
</dx-popup>
```
**⚠️ Critical**: Popup footer buttons must be inside `*dxTemplate="let data of 'content'"`. Placing `<dx-toolbar>` directly inside `<dx-popup>` (outside the template) will NOT render.

### SCSS for popup-actions
```scss
.popup-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1.25rem;
  padding-top: 1rem;
  border-top: 1px solid var(--dx-color-border, #ddd);
}
```

---

## TenantId / Multi-Tenancy

### DbContext safety net
`ApplyTenantId()` in `WarehouseKgDbContext` stamps `TenantId` on Added entities. **Always guard against `Guid.Empty`**:
```csharp
private void ApplyTenantId()
{
    var tenantId = CurrentTenantId;
    if (tenantId == Guid.Empty) return; // silent no-op = invisible entities
    // ... set TenantId on Added entities
}
```

### Entity creation rule
- All command handlers create entities **without** explicitly setting `TenantId`.
- `ApplyTenantId()` stamps it automatically from `CurrentTenantId` at `SaveChangesAsync` time.
- If `CurrentTenantId` is `Guid.Empty`, entities become invisible under the global query filter.
- Fix: the guard above prevents the silent no-op.

---

## Map / Leaflet

### Map-picker component (`shared/map-picker`)
- Supports `mode="point"` and `mode="polygon"`
- Inputs: `markers`, `geofenceOverlays`, `editable`, `height`
- **Markers must be redrawn after map init**: the `initMap` 200ms timeout must call `drawMarkers()` if markers arrived before map ready
- Same for `drawGeofenceOverlays()`

---

## Browser-Based Verification (Live Testing)

When verifying UI changes or debugging data issues, use the integrated browser tools instead of manual clicking.

### Session management
- Backend restart **invalidates the JWT session** — the browser page will show 401 errors.
- Fix: re-login via browser tools (see [[#Auto-login flow]] below).
- Detection: `read_page` shows no sidebar/toolbar, only the DevExtreme watermark — session expired.

### Reading page state
- `read_page` returns an accessibility snapshot with element refs and visible text.
- The first `read_page` after `navigate_page` may return `<unchanged>` — call `read_page` again.
- Use `run_playwright_code` with `page.evaluate()` to extract specific DOM text when the snapshot is too large.

### Interacting with DevExtreme widgets via browser tools

**Combobox (warehouse filter, language selector):**
```
click_element on combobox ref → listbox appears → click_element on option ref
```

**Data grid verification:**
- Grid shows as `group "Data grid with N rows and M columns"`
- Each `row` has `gridcell` children with text values
- Links in grid cells have `/url:` property for verifying navigation targets

**Movement history warehouse filter workflow:**
1. Page initially shows "Выберите склад для просмотра истории движений"
2. Click warehouse combobox → options appear ("depo 1", "depo 2", etc.)
3. Click option → grid populates with movement rows showing running balance
4. "На складе" header updates to show the calculated balance for the selected warehouse

### Verifying data integrity via browser
- **Inventory detail page**: Shows `QuantityOnHand` in the header. When a warehouse is selected, compares the running balance from movement history to the warehouse stock report.
- **Sales order detail**: Check `status` field to verify workflow transitions (Draft → Confirmed → Shipped).
- **Grid + DB cross-check**: Use `docker exec -i wkg-postgres psql` to verify DB values match what the UI shows.

### Auto-login flow
```
1. navigate_page → http://localhost:4200/login
2. type_in_page on username (ref=e32) → "admin"
3. type_in_page on password (ref=e36) → "Admin1234!"
4. click_element on login button (ref=e37)
5. navigate_page → target URL
```
