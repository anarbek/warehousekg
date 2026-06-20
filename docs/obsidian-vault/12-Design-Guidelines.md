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
