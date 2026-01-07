# ?? AJAX CRUD Pattern - Quick Reference Card

## ?? Implementation Checklist (5 Minutes Per Module)

### Step 1: Create 3 Partial Views (2 min)

#### `_EntityTable.cshtml`
```razor
@model IEnumerable<Entity>

@if (!Model.Any())
{
    <tr id="emptyRow">
        <td colspan="N"><div class="empty-state py-4">...</div></td>
    </tr>
}
else
{
    @foreach (var item in Model)
    {
        <tr data-id="@item.Id">
            <td>@item.Property</td>
            <td>
                <button class="btn btn-sm btn-edit" data-id="@item.Id"></button>
                <button class="btn btn-sm btn-delete" data-id="@item.Id" data-name="@item.Name"></button>
            </td>
        </tr>
    }
}
```

#### `_CreateModal.cshtml`
```razor
@model Entity
<form id="createEntityForm" asp-action="Create">
    <!-- Fields -->
    <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="submit" class="btn btn-success">Create</button>
    </div>
</form>
```

#### `_EditModal.cshtml`
```razor
@model Entity
<form id="editEntityForm" asp-action="Edit">
    <input type="hidden" asp-for="Id" />
    <!-- Fields -->
    <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="submit" class="btn btn-warning">Update</button>
    </div>
</form>
```

### Step 2: Update Controller (2 min)

```csharp
// Add GetTableData
public async Task<IActionResult> GetTableData()
{
    return PartialView("_EntityTable", await _context.Entities.ToListAsync());
}

// Update Create GET
public IActionResult Create()
{
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        return PartialView("_CreateModal");
    return View();
}

// Update Create POST
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Entity entity)
{
    if (ModelState.IsValid)
    {
        _context.Add(entity);
        await _context.SaveChangesAsync();
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, message = "Created successfully!" });
        return RedirectToAction(nameof(Index));
    }
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, errors });
    }
    return View(entity);
}

// Update Edit GET
public async Task<IActionResult> Edit(int? id)
{
    if (id == null) return NotFound();
    var entity = await _context.Entities.FindAsync(id);
    if (entity == null) return NotFound();
    
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        return PartialView("_EditModal", entity);
    return View(entity);
}

// Update Edit POST (same pattern as Create POST)

// Add DeleteAjax
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteAjax(int id)
{
    try
    {
        var entity = await _context.Entities.FindAsync(id);
        if (entity == null)
            return Json(new { success = false, message = "Not found!" });
        
        _context.Entities.Remove(entity);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Deleted successfully!" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Error: " + ex.Message });
    }
}
```

### Step 3: Update Index View (1 min)

```razor
<!-- Replace button -->
<button type="button" class="btn btn-primary btn-create" data-entity="entity">
    <i class="bi bi-plus-lg me-1"></i>Add Entity
</button>

<!-- Replace table body -->
<tbody id="tableBody">
    @Html.Partial("_EntityTable", Model)
</tbody>

<!-- Add modal -->
<div class="modal fade" id="entityModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Entity</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body" id="entityModalBody"></div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function() {
            const config = {
                entity: 'entity',
                entityPlural: 'entities',
                modalId: 'entityModal',
                tableBodyId: 'tableBody',
                getTableDataUrl: '@Url.Action("GetTableData")',
                createUrl: '@Url.Action("Create")',
                editUrl: '@Url.Action("Edit")',
                deleteUrl: '@Url.Action("DeleteAjax")'
            };
            AMS.CRUD.init(config);
        });
    </script>
    @Html.AntiForgeryToken()
}
```

---

## ? Essential Element IDs & Classes

| Element | ID/Class | Purpose |
|---------|----------|---------|
| Create Button | `.btn-create` | Opens create modal |
| Edit Button | `.btn-edit` | Opens edit modal |
| Delete Button | `.btn-delete` | Triggers delete confirmation |
| Table Body | `#tableBody` | Container for table rows |
| Modal Container | `#entityModal` | Bootstrap modal |
| Modal Body | `#entityModalBody` | Content loaded here |
| Create Form | `#createEntityForm` | Form for creating |
| Edit Form | `#editEntityForm` | Form for editing |
| Empty Row | `#emptyRow` | Row shown when no data |

---

## ?? Config Object Explained

```javascript
const config = {
    entity: 'student',              // Singular, lowercase - for messages
    entityPlural: 'students',       // Plural, lowercase - for messages
    modalId: 'studentModal',        // ID of modal element
    tableBodyId: 'tableBody',       // ID of <tbody> element
    getTableDataUrl: '@Url.Action("GetTableData")',  // Endpoint to refresh table
    createUrl: '@Url.Action("Create")',              // Endpoint for create form
    editUrl: '@Url.Action("Edit")',                  // Endpoint for edit form
    deleteUrl: '@Url.Action("DeleteAjax")'           // Endpoint for delete action
};
```

---

## ?? Common Issues & Fixes

| Issue | Solution |
|-------|----------|
| Modal doesn't open | Check button has `.btn-create` class |
| Form not submitting | Verify form ID matches pattern |
| Table not refreshing | Check `tableBodyId` matches tbody ID |
| Delete not working | Ensure buttons have `.btn-delete` class |
| Validation not showing | Include `@Html.AntiForgeryToken()` in scripts section |
| Modal shows old data | Modal content clears on close automatically |

---

## ?? Success Response Format

### Create/Edit Success
```json
{
    "success": true,
    "message": "Entity created successfully!"
}
```

### Create/Edit Failure
```json
{
    "success": false,
    "errors": ["Field is required", "Invalid format"]
}
```

### Delete Success
```json
{
    "success": true,
    "message": "Entity deleted successfully!"
}
```

### Delete Failure
```json
{
    "success": false,
    "message": "Cannot delete entity with dependencies"
}
```

---

## ?? Testing Checklist

- [ ] Create: Modal opens, form submits, table refreshes
- [ ] Edit: Modal opens with data, updates work, table refreshes
- [ ] Delete: Confirmation shows, deletes work, row fades out
- [ ] Validation: Required fields show errors
- [ ] Search: Filters table in real-time (if implemented)
- [ ] Empty state: Shows when no records
- [ ] Count updates: Stat card refreshes after operations

---

## ?? Ready-to-Use Examples

All implemented modules:
- ? Students ? `Views/Students/`
- ? Teachers ? `Views/Teachers/`
- ? Courses ? `Views/Courses/`
- ? Batches ? `Views/Batches/`
- ? Sections ? `Views/Sections/`
- ? Semesters ? `Views/Semesters/`

Just copy the pattern from any of these!

---

## ?? Pro Tips

1. **Naming Convention Matters**: Stick to the pattern for IDs and classes
2. **Anti-Forgery Tokens**: Always include in scripts section
3. **Modal IDs**: Must match config.modalId
4. **Form IDs**: Must be `create{Entity}Form` or `edit{Entity}Form`
5. **Button Classes**: `.btn-create`, `.btn-edit`, `.btn-delete` are required
6. **Empty Row**: Use `id="emptyRow"` for proper hide/show

---

## ?? That's It!

Follow these 3 steps and your module will have:
- ? No page reloads
- ? Modal dialogs
- ? Smooth animations
- ? Loading states
- ? Error handling
- ? Success notifications

**Time per module: 5 minutes**  
**Difficulty: Easy (copy-paste pattern)**

---

**Need more details?** See:
- `FINAL_IMPLEMENTATION_COMPLETE.md` - Full overview
- `AJAX_CRUD_IMPLEMENTATION_GUIDE.md` - Step-by-step guide
- `BATCHES_QUICK_START.md` - Worked example
