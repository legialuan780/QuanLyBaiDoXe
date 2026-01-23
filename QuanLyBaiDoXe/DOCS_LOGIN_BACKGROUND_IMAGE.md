# H??ng d?n thêm ?nh n?n cho trang Login

## T?ng quan
Trang Login có th? s? d?ng ?nh n?n ? ph?n bên trái (`.login-left`) ?? t?o giao di?n ??p và chuyên nghi?p h?n.

## Options ?? thêm ?nh n?n

### Option 1: S? d?ng Gradient (M?c ??nh) ?

**?u ?i?m**: 
- Không c?n t?i ?nh
- Load nhanh
- Luôn ??p

**CSS**:
```css
.login-left {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
```

**Gradient ??p khác**:
```css
/* Blue gradient */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

/* Ocean gradient */
background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);

/* Sunset gradient */
background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);

/* Forest gradient */
background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);

/* Night gradient */
background: linear-gradient(135deg, #4776e6 0%, #8e54e9 100%);
```

### Option 2: S? d?ng ?nh t? wwwroot/images ? (Recommended)

#### B??c 1: T?o folder images
```
QuanLyBaiDoXe/
??? wwwroot/
    ??? images/
        ??? login-bg.jpg  ? ??t ?nh vào ?ây
```

#### B??c 2: Thêm ?nh
- Tên file: `login-bg.jpg` (ho?c `.png`, `.webp`)
- Kích th??c khuy?n ngh?: 1920x1080px
- Dung l??ng: < 500KB (nén ?? load nhanh)

#### B??c 3: CSS ?ã s?n sàng
```css
.login-left {
    background-image: url('/images/login-bg.jpg');
    background-position: center;
    background-size: cover;
    background-repeat: no-repeat;
}
```

### Option 3: S? d?ng ?nh online (Unsplash, Pexels)

**?u ?i?m**: 
- Không t?n dung l??ng server
- ?nh ch?t l??ng cao mi?n phí

**Nh??c ?i?m**:
- Ph? thu?c internet
- Có th? b? ch?n b?i firewall

#### Unsplash URLs
```css
/* Parking lot theme */
background-image: url('https://images.unsplash.com/photo-1506521781263-d8422e82f27a?w=1200');

/* Modern building */
background-image: url('https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=1200');

/* Technology */
background-image: url('https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1200');

/* Car parking */
background-image: url('https://images.unsplash.com/photo-1590674899484-d5640e854abe?w=1200');
```

#### Pexels URLs
```css
/* Parking garage */
background-image: url('https://images.pexels.com/photos/164634/pexels-photo-164634.jpeg?auto=compress&w=1200');

/* Modern parking */
background-image: url('https://images.pexels.com/photos/753876/pexels-photo-753876.jpeg?auto=compress&w=1200');
```

## C?u hình hi?n t?i trong login.css

```css
.login-left{
    /* M?c ??nh: Gradient */
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    
    /* Uncomment ?? dùng ?nh local */
    background-image: url('/images/login-bg.jpg');
    
    /* Uncomment ?? dùng ?nh online */
    /* background-image: url('https://images.unsplash.com/photo-1506521781263-d8422e82f27a?w=1200'); */
    
    background-position: center;
    background-size: cover;
    background-repeat: no-repeat;
    position: relative;
}
```

## Overlay ?? text d? ??c

CSS ?ã có overlay t?i ?? text tr?ng d? ??c h?n trên ?nh:

```css
.login-left .overlay{
    background: linear-gradient(
        180deg,
        rgba(0,0,0,.5),      /* Top: T?i h?n */
        rgba(0,0,0,.3) 40%,  /* Middle: V?a */
        rgba(0,0,0,.1)       /* Bottom: Nh?t */
    );
}
```

**?i?u ch?nh ?? t?i**:
```css
/* T?i h?n (?nh sáng) */
rgba(0,0,0,.7)  /* 70% opacity */

/* Nh?t h?n (?nh t?i) */
rgba(0,0,0,.3)  /* 30% opacity */
```

## H??ng d?n chi ti?t: Thêm ?nh vào wwwroot

### B??c 1: Chu?n b? ?nh

#### Tìm ?nh mi?n phí:
- **Unsplash**: https://unsplash.com/s/photos/parking
- **Pexels**: https://www.pexels.com/search/parking%20lot/
- **Pixabay**: https://pixabay.com/images/search/parking/

#### Keywords tìm ki?m:
- "parking lot"
- "car park"
- "modern building"
- "technology background"
- "business background"

#### Yêu c?u ?nh:
- Kích th??c: 1920x1080px (Full HD) ho?c l?n h?n
- T? l?: 16:9 ho?c 4:3
- Format: JPG (nh?) ho?c WebP (nh? h?n)
- Dung l??ng: < 500KB

### B??c 2: T?i ?u ?nh (n?u c?n)

#### Online tools:
- **TinyJPG**: https://tinyjpg.com/ (nén JPG/PNG)
- **Squoosh**: https://squoosh.app/ (convert to WebP)
- **Compressor.io**: https://compressor.io/

#### Resize n?u quá l?n:
```
Original: 4000x3000px ? 1920x1080px
Size: 2.5MB ? 300KB
```

### B??c 3: T?o folder và copy ?nh

#### Via File Explorer:
1. M? folder: `QuanLyBaiDoXe/wwwroot/`
2. T?o folder m?i: `images`
3. Copy ?nh vào: `wwwroot/images/login-bg.jpg`

#### Via Command Line:
```bash
cd QuanLyBaiDoXe/wwwroot
mkdir images
copy "C:\Downloads\your-image.jpg" images\login-bg.jpg
```

### B??c 4: Verify trong project

**Solution Explorer**:
```
QuanLyBaiDoXe
??? wwwroot
    ??? images
        ??? login-bg.jpg  ? Check file này
```

**File size**: Nên < 500KB

### B??c 5: Test

1. Build project: `Ctrl + Shift + B`
2. Run: `F5`
3. Navigate: `https://localhost:xxxx/Account/Login`
4. Check: ?nh hi?n th? ? bên trái

## Troubleshooting

### Issue 1: ?nh không hi?n th?

**Ki?m tra**:
```css
/* login.css - ??m b?o có dòng này */
background-image: url('/images/login-bg.jpg');
```

**Verify path**:
- URL: `/images/login-bg.jpg` (b?t ??u b?ng `/`)
- NOT: `~/images/login-bg.jpg`
- NOT: `images/login-bg.jpg`

**Check file t?n t?i**:
```
Browser: https://localhost:xxxx/images/login-bg.jpg
? Should display the image
```

### Issue 2: ?nh b? v?/không ?úng t? l?

**CSS fix**:
```css
.login-left {
    background-position: center;  /* Center the image */
    background-size: cover;       /* Cover entire area */
    background-repeat: no-repeat; /* Don't repeat */
}
```

### Issue 3: Text không ??c ???c

**T?ng overlay opacity**:
```css
.login-left .overlay {
    background: linear-gradient(
        180deg,
        rgba(0,0,0,.7),  /* Increase from .5 to .7 */
        rgba(0,0,0,.5) 40%,
        rgba(0,0,0,.2)
    );
}
```

### Issue 4: ?nh quá n?ng, load ch?m

**Solution**:
1. Nén ?nh: TinyJPG, Squoosh
2. Resize: 1920x1080px là ??
3. Convert to WebP:
   ```css
   background-image: url('/images/login-bg.webp');
   ```

### Issue 5: 404 Not Found

**Check**:
1. File name chính xác: `login-bg.jpg` (case-sensitive trên Linux)
2. File trong folder: `wwwroot/images/`
3. Build l?i project
4. Clear browser cache: `Ctrl + F5`

## Recommended Images

### Parking Lot Theme
```css
/* Professional parking */
background-image: url('https://images.unsplash.com/photo-1506521781263-d8422e82f27a?w=1200');

/* Modern garage */
background-image: url('https://images.unsplash.com/photo-1590674899484-d5640e854abe?w=1200');

/* Underground parking */
background-image: url('https://images.pexels.com/photos/753876/pexels-photo-753876.jpeg?w=1200');
```

### Business/Tech Theme
```css
/* Modern building */
background-image: url('https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=1200');

/* Technology */
background-image: url('https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1200');

/* City lights */
background-image: url('https://images.unsplash.com/photo-1477959858617-67f85cf4f1df?w=1200');
```

## Gradient + Image Combination

K?t h?p c? gradient và ?nh:
```css
.login-left {
    background: 
        linear-gradient(135deg, rgba(102,126,234,0.8), rgba(118,75,162,0.8)),
        url('/images/login-bg.jpg');
    background-position: center;
    background-size: cover;
}
```

## Multiple Images (Slideshow - Advanced)

?? có slideshow t? ??ng ??i ?nh, c?n thêm JavaScript:

```html
<!-- Trong Login.cshtml, thêm vào <head> -->
<style>
.login-left {
    animation: bgSlideshow 20s infinite;
}

@keyframes bgSlideshow {
    0%   { background-image: url('/images/bg1.jpg'); }
    33%  { background-image: url('/images/bg2.jpg'); }
    66%  { background-image: url('/images/bg3.jpg'); }
    100% { background-image: url('/images/bg1.jpg'); }
}
</style>
```

## Performance Tips

1. **Optimize images**: < 500KB
2. **Use WebP**: 30% smaller than JPG
3. **Lazy load**: Not needed (above fold)
4. **CDN**: Use Unsplash/Pexels URLs
5. **Preload** (optional):
   ```html
   <link rel="preload" as="image" href="/images/login-bg.jpg">
   ```

## Accessibility

??m b?o text v?n d? ??c:
- Contrast ratio: >= 4.5:1
- Text shadow n?u c?n
- Overlay ?? t?i

## Summary

? **Quick Start**:
1. Download ?nh t? Unsplash/Pexels
2. Nén xu?ng < 500KB
3. Copy vào `wwwroot/images/login-bg.jpg`
4. Uncomment dòng trong `login.css`:
   ```css
   background-image: url('/images/login-bg.jpg');
   ```
5. Build và test

?? **Customization**:
- Gradient: ??i màu trong CSS
- Overlay: ?i?u ch?nh rgba opacity
- Multiple images: Thêm slideshow

?? **Troubleshooting**:
- Check file path: `/images/login-bg.jpg`
- Verify file exists: Navigate directly in browser
- Clear cache: Ctrl + F5
- Check CSS syntax

?? **Recommended**:
- Size: 1920x1080px
- Format: JPG or WebP
- Weight: < 500KB
- Theme: Parking, business, technology
