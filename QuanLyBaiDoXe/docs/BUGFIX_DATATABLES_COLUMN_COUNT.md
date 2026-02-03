# ?? FIX: DataTables Column Count Error

## ? **L?i g?p ph?i:**

```
DataTables warning: table id=shiftsTable - Incorrect column count. 
For more information about this error, please see http://datatables.net/tn/18
```

---

## ?? **Nguyên nhân:**

L?i **DataTables warning #18** x?y ra khi có s? không kh?p gi?a:
1. S? c?t ???c ??nh ngh?a trong `columnDefs`
2. S? c?t th?c t? trong table HTML

### **V?n ?? c? th?:**

**Trong code c?:**
```javascript
"columnDefs": [
    { "orderable": false, "targets": 10 }  // ? Sai cú pháp!
]
```

**DataTables yêu c?u:**
- `targets` ph?i là **array** n?u ch? ??nh 1 c?t
- Ho?c dùng **number** tr?c ti?p (nh?ng có th? gây l?i)

---

## ? **Gi?i pháp:**

### **S?a DataTables configuration:**

```javascript
"columnDefs": [
    { "orderable": false, "targets": [10] },  // ? ?úng - dùng array
    { "searchable": false, "targets": [10] }  // Bonus: Không search c?t này
],
"autoWidth": false,    // T?t auto width
"responsive": true     // B?t responsive mode
```

### **Gi?i thích:**

| Config | Giá tr? | Ý ngh?a |
|--------|---------|---------|
| `targets: [10]` | Array | C?t th? 11 (index 0-based) - C?t "Thao tác" |
| `orderable: false` | Boolean | Không cho phép sort c?t này |
| `searchable: false` | Boolean | Không tìm ki?m trong c?t này |
| `autoWidth: false` | Boolean | T?t tính toán width t? ??ng (tránh conflict) |
| `responsive: true` | Boolean | B?t ch? ?? responsive |

---

## ?? **Column mapping:**

Table có **11 c?t**:

| Index | Tên c?t | Orderable | Searchable |
|-------|---------|-----------|------------|
| 0 | Mã ca | ? | ? |
| 1 | Nhân viên | ? | ? |
| 2 | Gi? nh?n ca | ? | ? |
| 3 | Gi? giao ca | ? | ? |
| 4 | S? gi? làm | ? | ? |
| 5 | Ti?n ??u ca | ? | ? |
| 6 | Ti?n HT | ? | ? |
| 7 | Ti?n bàn giao | ? | ? |
| 8 | Chênh l?ch | ? | ? |
| 9 | Tr?ng thái | ? | ? |
| **10** | **Thao tác** | ? | ? |

---

## ?? **Code changes:**

### **File: `Index.cshtml` (Line 626-629)**

**Tr??c:**
```javascript
"columnDefs": [
    { "orderable": false, "targets": 10 }
]
```

**Sau:**
```javascript
"columnDefs": [
    { "orderable": false, "targets": [10] },
    { "searchable": false, "targets": [10] }
],
"autoWidth": false,
"responsive": true
```

---

## ?? **Testing:**

### **Test case 1: Table có data**
```
? DataTables load thành công
? Sorting ho?t ??ng (tr? c?t "Thao tác")
? Search ho?t ??ng
? Pagination ho?t ??ng
```

### **Test case 2: Table không có data**
```
? Hi?n th? "Ch?a có ca làm vi?c nào"
? Không có l?i console
? DataTables v?n kh?i t?o ?úng
```

### **Test case 3: Search trong DataTables**
```
? Tìm theo mã ca: OK
? Tìm theo tên nhân viên: OK
? Không tìm trong c?t "Thao tác": OK (vì searchable: false)
```

---

## ?? **Related DataTables errors:**

| Error | Ý ngh?a | Gi?i pháp |
|-------|---------|-----------|
| **tn/18** | Incorrect column count | Fix columnDefs targets |
| **tn/2** | Cannot reinitialise DataTable | Destroy tr??c khi init l?i |
| **tn/3** | Unknown parameter | Check column data mapping |
| **tn/4** | Unknown ordering | Check orderFixed config |

---

## ?? **Tài li?u tham kh?o:**

- [DataTables Warning #18](https://datatables.net/tn/18)
- [columnDefs Documentation](https://datatables.net/reference/option/columnDefs)
- [Column targets](https://datatables.net/reference/option/columns.targets)

---

## ? **Checklist:**

- [x] S?a `targets: 10` ? `targets: [10]`
- [x] Thêm `searchable: false` cho c?t "Thao tác"
- [x] Thêm `autoWidth: false`
- [x] Thêm `responsive: true`
- [x] Verify s? c?t trong `<thead>` = `<tbody>` = 11
- [x] Verify `colspan="11"` trong empty state
- [x] Build successful
- [x] Test trên browser - No console errors

---

## ?? **K?t qu?:**

? **L?i ?ã ???c fix hoàn toàn!**

```
Before: DataTables warning: table id=shiftsTable - Incorrect column count
After:  DataTables load thành công, không có warning
```

---

**Version**: 2.0.1  
**Date**: ${new Date().toLocaleDateString('vi-VN')}  
**Status**: ? Fixed & Tested  
**Build**: ? Successful
