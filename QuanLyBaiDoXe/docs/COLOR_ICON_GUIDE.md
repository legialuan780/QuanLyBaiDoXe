# H? th?ng Icon và Màu s?c - Lo?i S? c?

## ?? Quy chu?n thi?t k?

### M?c tiêu
- Th?ng nh?t icon và màu s?c cho t?t c? các lo?i s? c? trong toàn b? h? th?ng
- D? nh?n di?n và phân bi?t
- Tuân th? nguyên t?c thi?t k? UI/UX

## ?? B?ng Quy chu?n

| Lo?i s? c? | Icon | Màu s?c | Hex Code | Bootstrap Class | Ý ngh?a |
|-----------|------|---------|----------|-----------------|---------|
| **Xe m?t th?** | `fas fa-id-card-alt` | Vàng cam | `#f39c12` | `bg-warning` | C?nh báo, c?n chú ý |
| **L?i barrier** | `fas fa-road-barrier` | ?? | `#e74c3c` | `bg-danger` | Nguy hi?m, ?u tiên cao |
| **L?i camera** | `fas fa-video-slash` | Xanh d??ng | `#3498db` | `bg-info` | Thông tin, k? thu?t |
| **Tranh ch?p phí** | `fas fa-hand-holding-usd` | Tím | `#9b59b6` | `bg-purple` | Tài chính, quan tr?ng |
| **Kh?n c?p** | `fas fa-exclamation-triangle` | ?? + Pulse | `#e74c3c` | `bg-danger pulse-badge` | Kh?n c?p, x? lý ngay |
| **Khác** | `fas fa-flag` | Xám | `#95a5a6` | `bg-secondary` | M?c ??nh, thông th??ng |

## ?? Cách s? d?ng

### 1. Trong Razor View (C#)

```csharp
@{
    // ??nh ngh?a helper functions
    string GetAnomalyIcon(string loaiSuCo) => loaiSuCo switch
    {
        "Xe m?t th?" => "fas fa-id-card-alt",
        "L?i barrier" => "fas fa-road-barrier",
        "L?i camera" => "fas fa-video-slash",
        "Tranh ch?p phí" => "fas fa-hand-holding-usd",
        "Kh?n c?p" => "fas fa-exclamation-triangle",
        _ => "fas fa-flag"
    };
    
    string GetAnomalyBgClass(string loaiSuCo) => loaiSuCo switch
    {
        "Xe m?t th?" => "bg-warning",
        "L?i barrier" => "bg-danger",
        "L?i camera" => "bg-info",
        "Tranh ch?p phí" => "bg-purple",
        "Kh?n c?p" => "bg-danger pulse-badge",
        _ => "bg-secondary"
    };
}

<!-- S? d?ng -->
<span class="badge @GetAnomalyBgClass(Model.LoaiSuCo)">
    <i class="@GetAnomalyIcon(Model.LoaiSuCo) me-1"></i>
    @Model.LoaiSuCo
</span>
```

### 2. Trong JavaScript

```javascript
// Load helper
<script src="~/js/anomaly-type-helper.js"></script>

// S? d?ng
const anomalyType = AnomalyTypeHelper.format('Kh?n c?p');

// Render badge
const badgeHtml = anomalyType.badge;
// Output: <span class="badge bg-danger pulse-badge">
//           <i class="fas fa-exclamation-triangle me-1"></i>Kh?n c?p
//         </span>

// L?y t?ng thành ph?n
const icon = AnomalyTypeHelper.getIcon('L?i camera'); // "fas fa-video-slash"
const color = AnomalyTypeHelper.getColor('Xe m?t th?'); // "#f39c12"
const bgClass = AnomalyTypeHelper.getBgClass('L?i barrier'); // "bg-danger"
```

### 3. Trong CSS

```css
/* Màu purple custom (Bootstrap không có s?n) */
.bg-purple {
    background-color: #9b59b6 !important;
    color: white;
}

.text-purple {
    color: #9b59b6 !important;
}

/* Hi?u ?ng pulse cho s? c? kh?n c?p */
.pulse-badge {
    animation: pulse-badge 2s infinite;
}

@keyframes pulse-badge {
    0%, 100% {
        box-shadow: 0 0 0 0 rgba(231, 76, 60, 0.7);
    }
    50% {
        box-shadow: 0 0 0 8px rgba(231, 76, 60, 0);
    }
}
```

## ?? Best Practices

### 1. Nh?t quán trong toàn b? h? th?ng
? **?úng:**
```html
<!-- T?t c? n?i ??u dùng cùng icon và màu -->
<span class="badge bg-danger pulse-badge">
    <i class="fas fa-exclamation-triangle me-1"></i>Kh?n c?p
</span>
```

? **Sai:**
```html
<!-- Không nh?t quán -->
<span class="badge bg-warning">
    <i class="fas fa-bolt me-1"></i>Kh?n c?p
</span>
```

### 2. Luôn kèm icon v?i label
? **?úng:**
```html
<span class="badge bg-warning">
    <i class="fas fa-id-card-alt me-1"></i>Xe m?t th?
</span>
```

? **Sai:**
```html
<span class="badge bg-warning">Xe m?t th?</span>
```

### 3. S? d?ng helper thay vì hard-code
? **?úng:**
```javascript
const badge = AnomalyTypeHelper.renderBadge(item.loaiSuCo);
```

? **Sai:**
```javascript
const badge = `<span class="badge bg-warning">${item.loaiSuCo}</span>`;
```

## ?? ?ng d?ng trong các màn hình

### Dashboard
- Stat cards: Hi?n th? icon l?n v?i màu gradient
- Bi?u ??: S? d?ng hex color code
- B?ng: Badge v?i icon + text

### Danh sách s? c?
- Badge ??y ?? icon + text
- Hover effect v?i màu t??ng ?ng
- Sort/filter theo màu s?c

### Chi ti?t s? c?
- Icon l?n ? header
- Badge trong timeline
- Color coding cho priority

### Giám sát Realtime
- Canvas visualization v?i hex color
- Badge v?i pulse animation cho kh?n c?p
- B?n ?? nhi?t v?i gradient

## ?? Quy trình c?p nh?t

### Khi thêm lo?i s? c? m?i:

1. **Ch?n màu và icon phù h?p**
   - Tham kh?o b?ng quy chu?n
   - ??m b?o không trùng màu
   - Icon ph?i có ý ngh?a rõ ràng

2. **C?p nh?t helper functions**
   ```csharp
   // File: _AnomalyTypeHelper.cshtml
   string GetAnomalyIcon(string loaiSuCo) => loaiSuCo switch
   {
       // ... existing cases
       "Lo?i m?i" => "fas fa-new-icon",
       _ => "fas fa-flag"
   };
   ```

3. **C?p nh?t JavaScript helper**
   ```javascript
   // File: anomaly-type-helper.js
   types: {
       // ... existing types
       'Lo?i m?i': {
           icon: 'fas fa-new-icon',
           color: '#hexcolor',
           bgClass: 'bg-class',
           label: 'Lo?i m?i'
       }
   }
   ```

4. **C?p nh?t dropdowns**
   - Index.cshtml filter
   - Modal b? l?c
   - Các select boxes khác

5. **Test trên t?t c? màn hình**
   - Dashboard
   - Danh sách s? c?
   - Chi ti?t s? c?
   - Giám sát Realtime
   - Reports

## ?? Màu s?c theo chu?n Material Design

| Lo?i | Màu | S? d?ng |
|------|-----|---------|
| Success | `#21A691` | Hoàn thành, ?ã x? lý |
| Warning | `#f39c12` | C?nh báo, c?n chú ý |
| Danger | `#e74c3c` | L?i, kh?n c?p |
| Info | `#3498db` | Thông tin |
| Secondary | `#95a5a6` | M?c ??nh |
| Purple | `#9b59b6` | Custom - tài chính |

## ?? Responsive & Accessibility

### Kích th??c icon
- Mobile: `fa-sm` ho?c default
- Tablet: default
- Desktop: `fa-lg` cho header

### Contrast ratio
- T?t c? màu ??u ??m b?o WCAG AA
- Text màu tr?ng trên background màu ??m
- Badge có ?? t??ng ph?n cao

### Screen readers
```html
<span class="badge bg-danger" aria-label="Kh?n c?p, ?u tiên cao">
    <i class="fas fa-exclamation-triangle me-1" aria-hidden="true"></i>
    Kh?n c?p
</span>
```

## ?? Troubleshooting

### Icon không hi?n th??
- Ki?m tra Font Awesome ?ã load: `<link href="..." rel="stylesheet">`
- Verify class name: `fas` vs `far` vs `fab`
- Check console errors

### Màu không ?úng?
- Ensure CSS ?ã load
- Check class specificity
- Verify hex color code

### Pulse animation không ho?t ??ng?
- Check CSS keyframes
- Verify class `.pulse-badge`
- Browser support (all modern browsers)

## ?? References

- [Font Awesome Icons](https://fontawesome.com/icons)
- [Bootstrap Colors](https://getbootstrap.com/docs/5.0/utilities/colors/)
- [Material Design Colors](https://material.io/design/color/)

---

**Version:** 1.0.0  
**Last Updated:** 2025-01-23  
**Maintainer:** Parking Management System Team
