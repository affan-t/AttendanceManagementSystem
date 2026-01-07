# ?? Form ID Naming Convention - Quick Reference

## ? CORRECT Pattern

The AJAX CRUD handler expects this exact pattern:

```
create{Entity}Form    where {Entity} = Capitalized entity name
edit{Entity}Form      where {Entity} = Capitalized entity name
```

### Examples by Module

| Module | Config Entity | ? Create Form ID | ? Edit Form ID |
|--------|---------------|-------------------|-----------------|
| Students | `'student'` | `createStudentForm` | `editStudentForm` |
| Teachers | `'teacher'` | `createTeacherForm` | `editTeacherForm` |
| Courses | `'course'` | `createCourseForm` | `editCourseForm` |
| Batches | `'batch'` | `createBatchForm` | `editBatchForm` |
| Sections | `'section'` | `createSectionForm` | `editSectionForm` |
| Semesters | `'semester'` | `createSemesterForm` | `editSemesterForm` |
| Course Allocations | `'allocation'` | `createAllocationForm` | `editAllocationForm` |

## ? Common Mistakes

### Mistake 1: Using Full Model Name
```razor
<!-- ? WRONG -->
<form id="createCourseAllocationForm">

<!-- ? CORRECT -->
<form id="createAllocationForm">
```

### Mistake 2: Not Capitalizing
```razor
<!-- ? WRONG -->
<form id="createallocationForm">
<form id="createallocationform">

<!-- ? CORRECT -->
<form id="createAllocationForm">
```

### Mistake 3: Wrong Prefix
```razor
<!-- ? WRONG -->
<form id="newAllocationForm">
<form id="addAllocationForm">

<!-- ? CORRECT -->
<form id="createAllocationForm">
```

### Mistake 4: Missing 'Form' Suffix
```razor
<!-- ? WRONG -->
<form id="createAllocation">
<form id="editAllocation">

<!-- ? CORRECT -->
<form id="createAllocationForm">
<form id="editAllocationForm">
```

## ?? How to Verify

### Step 1: Check Your Config
Look at your `Index.cshtml` scripts section:
```javascript
const config = {
    entity: 'allocation',  // ? Note this value
    // ...
};
```

### Step 2: Calculate Expected Form IDs
Take the `entity` value and:
1. Capitalize first letter: `allocation` ? `Allocation`
2. Add prefix and suffix:
   - Create: `create` + `Allocation` + `Form` = `createAllocationForm`
   - Edit: `edit` + `Allocation` + `Form` = `editAllocationForm`

### Step 3: Check Your Modal Files
Verify that `_CreateModal.cshtml` and `_EditModal.cshtml` use these exact IDs:

```razor
<!-- _CreateModal.cshtml -->
<form id="createAllocationForm" asp-action="Create" method="post">

<!-- _EditModal.cshtml -->
<form id="editAllocationForm" asp-action="Edit" method="post">
```

## ?? Visual Pattern

```
???????????????????????????????????????????????????
?  Config Entity: 'allocation'                    ?
???????????????????????????????????????????????????
                    ?
         Capitalize First Letter
                    ?
               'Allocation'
                    ?
         ???????????????????????
         ?                     ?
    Add 'create'          Add 'edit'
    + 'Form'              + 'Form'
         ?                     ?
??????????????????    ??????????????????
?createAllocation?    ?editAllocation  ?
?Form            ?    ?Form            ?
??????????????????    ??????????????????
```

## ?? Quick Test

After fixing form IDs, test this in browser console:

```javascript
// Should return the form element (not null)
console.log($('#createAllocationForm'));  // Should show: [form#createAllocationForm]
console.log($('#editAllocationForm'));    // Should show: [form#editAllocationForm]

// Check if AJAX handler is attached
$('#createAllocationForm').submit(); // Should NOT reload page
```

## ?? Checklist for New Modules

When adding AJAX CRUD to a new module:

- [ ] Choose a short, lowercase entity name (e.g., 'allocation', not 'courseAllocation')
- [ ] Add config with entity name to Index.cshtml
- [ ] In `_CreateModal.cshtml`: Use `id="create{Entity}Form"`
- [ ] In `_EditModal.cshtml`: Use `id="edit{Entity}Form"`
- [ ] Capitalize first letter of entity name in form IDs
- [ ] Test: Click create/edit buttons - modals should open
- [ ] Test: Submit forms - should NOT reload page
- [ ] Test: Check for success messages and table refresh

## ?? Troubleshooting

### Problem: Page still reloads after form submit
**Check:** Form IDs match the pattern

### Problem: Modal opens but form submits normally
**Check:** Form IDs are correct AND `AMS.CRUD.init()` is called

### Problem: Form validation doesn't work
**Check:** Include `@section Scripts` with `_ValidationScriptsPartial` in modal

### Problem: Dropdowns are empty in modal
**Check:** Controller loads ViewData before returning partial view

## ? Success Indicators

You know it's working when:
- ? Modal opens on button click
- ? Form submits without page reload
- ? Success toast appears
- ? Modal closes automatically
- ? Table refreshes with new/updated data
- ? Count updates in stat card
- ? No browser console errors

---

**Remember:** The form ID naming is **critical** for AJAX to work. Always follow the pattern exactly!
