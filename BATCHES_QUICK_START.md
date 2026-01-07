# Quick Start: Apply AJAX CRUD to Batches

This guide shows exactly what files to create and modify to add the No-Refresh CRUD system to the Batches module.

## Step 1: Create Partial Views

### File: `Views/Batches/_BatchesTable.cshtml`

```razor
@model IEnumerable<AttendanceManagementSystem.Models.Batch>

@if (!Model.Any())
{
    <tr id="emptyRow">
        <td colspan="3">
            <div class="empty-state py-4">
                <div class="empty-state-icon"><i class="bi bi-calendar3"></i></div>
                <h6 class="empty-state-title">No Batches Found</h6>
                <p class="empty-state-text mb-0">Get started by adding your first batch.</p>
            </div>
        </td>
    </tr>
}
else
{
    @foreach (var item in Model)
    {
        <tr data-id="@item.Id">
            <td class="fw-medium">@item.Name</td>
            <td><span class="badge badge-primary">@item.Year</span></td>
            <td>
                <div class="table-actions">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit" 
                            data-id="@item.Id" title="Edit">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-delete" 
                            data-id="@item.Id" data-name="@item.Name" title="Delete">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    }
}
```

### File: `Views/Batches/_CreateModal.cshtml`

```razor
@model AttendanceManagementSystem.Models.Batch

<form id="createBatchForm" asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

    <div class="form-group mb-3">
        <label asp-for="Name" class="control-label"></label>
        <input asp-for="Name" class="form-control" placeholder="e.g., 2020-2024" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>

    <div class="form-group mb-3">
        <label asp-for="Year" class="control-label"></label>
        <input asp-for="Year" class="form-control" type="number" placeholder="e.g., 2020" />
        <span asp-validation-for="Year" class="text-danger"></span>
    </div>

    <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="submit" class="btn btn-success">Create Batch</button>
    </div>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### File: `Views/Batches/_EditModal.cshtml`

```razor
@model AttendanceManagementSystem.Models.Batch

<form id="editBatchForm" asp-action="Edit" method="post">
    <input type="hidden" asp-for="Id" />
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

    <div class="form-group mb-3">
        <label asp-for="Name" class="control-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>

    <div class="form-group mb-3">
        <label asp-for="Year" class="control-label"></label>
        <input asp-for="Year" class="form-control" type="number" />
        <span asp-validation-for="Year" class="text-danger"></span>
    </div>

    <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="submit" class="btn btn-warning">Update Batch</button>
    </div>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

## Step 2: Update Controller

### File: `Controllers/BatchesController.cs`

Add this method after the `Index()` method:

```csharp
// GET: Batches/GetTableData - AJAX
public async Task<IActionResult> GetTableData()
{
    return PartialView("_BatchesTable", await _context.Batches.OrderByDescending(b => b.Year).ToListAsync());
}
```

Your Create and Edit GET methods already support AJAX (they check for `X-Requested-With`), so no changes needed there!

## Step 3: Update Index View

### File: `Views/Batches/Index.cshtml`

Replace the entire file with:

```razor
@model IEnumerable<AttendanceManagementSystem.Models.Batch>

@{
    ViewData["Title"] = "Batches";
}

<!-- Page Header -->
<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h1 class="page-title">Batches</h1>
        <p class="page-subtitle mb-0">Manage academic batches/sessions</p>
    </div>
    <button type="button" class="btn btn-primary btn-create" data-entity="batch">
        <i class="bi bi-plus-lg me-1"></i>Add Batch
    </button>
</div>

<!-- Stats -->
<div class="row g-3 mb-4">
    <div class="col-md-3">
        <div class="stat-card">
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <div class="stat-card-value" id="totalCount">@Model.Count()</div>
                    <div class="stat-card-label">Total Batches</div>
                </div>
                <div class="stat-card-icon info">
                    <i class="bi bi-calendar3"></i>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Table -->
<div class="card">
    <div class="card-header d-flex justify-content-between align-items-center">
        <h6 class="mb-0">All Batches</h6>
        <div class="input-group" style="width: 250px;">
            <span class="input-group-text"><i class="bi bi-search"></i></span>
            <input type="text" id="searchInput" class="form-control form-control-sm" placeholder="Search...">
        </div>
    </div>
    <div class="table-responsive">
        <table class="table mb-0" id="dataTable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Year</th>
                    <th style="width: 120px;">Actions</th>
                </tr>
            </thead>
            <tbody id="tableBody">
                @Html.Partial("_BatchesTable", Model)
            </tbody>
        </table>
    </div>
</div>

<!-- Modal for Create/Edit -->
<div class="modal fade" id="batchModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="batchModalLabel">Batch</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body" id="batchModalBody">
                <!-- Content loaded via AJAX -->
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function() {
            // Configuration
            const config = {
                entity: 'batch',
                entityPlural: 'batches',
                modalId: 'batchModal',
                tableBodyId: 'tableBody',
                getTableDataUrl: '@Url.Action("GetTableData")',
                createUrl: '@Url.Action("Create")',
                editUrl: '@Url.Action("Edit")',
                deleteUrl: '@Url.Action("DeleteConfirmed")'
            };

            // Initialize CRUD Handler
            AMS.CRUD.init(config);
            
            // Search functionality
            $('#searchInput').on('keyup', function() {
                const searchTerm = $(this).val().toLowerCase();
                $('#dataTable tbody tr:not(#emptyRow)').each(function() {
                    const text = $(this).text().toLowerCase();
                    $(this).toggle(text.includes(searchTerm));
                });
            });
        });
    </script>
    @Html.AntiForgeryToken()
}
```

## Step 4: Test

1. **Run the application**
2. **Navigate to Batches page**
3. **Test Create**:
   - Click "Add Batch"
   - Fill form
   - Click "Create Batch"
   - Verify modal closes and table refreshes
4. **Test Edit**:
   - Click pencil icon on any batch
   - Modify data
   - Click "Update Batch"
   - Verify changes appear in table
5. **Test Delete**:
   - Click trash icon
   - Confirm deletion
   - Verify row disappears smoothly

## Summary

You created 3 new files and modified 2 existing files:

**New Files:**
- ? `Views/Batches/_BatchesTable.cshtml`
- ? `Views/Batches/_CreateModal.cshtml`
- ? `Views/Batches/_EditModal.cshtml`

**Modified Files:**
- ? `Controllers/BatchesController.cs` (added `GetTableData()`)
- ? `Views/Batches/Index.cshtml` (converted to AJAX)

**Total Time:** ~10 minutes

Now you can apply the same pattern to **Courses**, **Sections**, and any other entity!
