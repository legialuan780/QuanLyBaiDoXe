# Hướng dẫn sửa lỗi encoding tiếng Việt

## Vấn đề đã được khắc phục:

### 1. **Program.cs**
- Thêm `Console.OutputEncoding = Encoding.UTF8;`
- Cấu hình JSON Serializer để hỗ trợ tiếng Việt
- Sử dụng `UnsafeRelaxedJsonEscaping` để không escape ký tự Unicode

### 2. **CustomerController.cs**
- Thêm `Response.ContentType = "text/html; charset=utf-8";` trong Index action
- Đảm bảo tất cả responses trả về với UTF-8 encoding

### 3. **Index.cshtml**
- File được lưu với UTF-8 BOM encoding
- Tất cả ký tự tiếng Việt đã được kiểm tra

### 4. **.editorconfig**
- Cấu hình tất cả files sử dụng UTF-8
- CS và CSHTML files sử dụng UTF-8 BOM
- JSON, JS, CSS files sử dụng UTF-8 không BOM

### 5. **web.config**
- Cấu hình globalization cho vi-VN culture
- Thiết lập requestEncoding và responseEncoding là UTF-8
- Custom headers cho Content-Type với charset=utf-8

## Cách kiểm tra:

1. **Trong Visual Studio:**
   - File > Advanced Save Options
   - Chọn "Unicode (UTF-8 with signature) - Codepage 65001"
   - Click OK

2. **Chạy ứng dụng:**
   ```bash
   dotnet run
   ```

3. **Truy cập trang:**
   ```
   https://localhost:7093/Admin/Customer
   ```

4. **Kiểm tra tiếng Việt:**
   - Tất cả text tiếng Việt phải hiển thị đúng
   - Form input phải nhập được tiếng Việt
   - Response JSON phải chứa tiếng Việt đúng

## Nếu vẫn còn lỗi:

### Kiểm tra Database:
```sql
-- Kiểm tra collation của database
SELECT DATABASEPROPERTYEX('QuanLyBaiDoXe', 'Collation');

-- Nếu cần đổi collation (Cẩn thận!)
ALTER DATABASE QuanLyBaiDoXe COLLATE Vietnamese_CI_AS;
```

### Kiểm tra Browser:
- Mở Developer Tools (F12)
- Vào tab Network
- Kiểm tra Response Headers phải có: `Content-Type: text/html; charset=utf-8`

### Kiểm tra File Encoding trong VS Code:
- Nhấn vào phần encoding ở góc dưới phải
- Chọn "Reopen with Encoding" > "UTF-8"
- Sau đó "Save with Encoding" > "UTF-8 with BOM" cho .cs và .cshtml
- "Save with Encoding" > "UTF-8" cho .json, .js, .css

## Best Practices:

1. **Luôn sử dụng UTF-8 BOM cho:**
   - .cs files
   - .cshtml files
   - .razor files

2. **Luôn sử dụng UTF-8 (không BOM) cho:**
   - .json files
   - .js files
   - .css files
   - .html files

3. **Trong code:**
   - Luôn sử dụng `Encoding.UTF8`
   - Không hard-code Vietnamese characters trong binary format
   - Sử dụng Unicode escape sequences nếu cần: `\u####`

4. **Database:**
   - Sử dụng NVARCHAR cho Vietnamese text
   - Collation: Vietnamese_CI_AS hoặc Latin1_General_CI_AS
   - Không sử dụng VARCHAR cho tiếng Việt

## Lưu ý quan trọng:

⚠️ **Khi commit code lên Git:**
- Đảm bảo `.gitattributes` có dòng:
  ```
  * text=auto eol=lf
  *.cs text eol=crlf
  *.cshtml text eol=crlf
  ```

⚠️ **Khi làm việc nhóm:**
- Tất cả thành viên phải sử dụng cùng encoding
- Sử dụng `.editorconfig` để đồng bộ settings
- Commit `.editorconfig` vào repository

⚠️ **Deployment:**
- Đảm bảo server IIS có cài đặt .NET 8 Runtime
- `web.config` phải có trong published folder
- Kiểm tra IIS Application Pool settings

## Testing Checklist:

- [ ] Tiếng Việt hiển thị đúng trên page
- [ ] Form input nhận tiếng Việt
- [ ] Search với tiếng Việt hoạt động
- [ ] JSON API trả về tiếng Việt đúng
- [ ] Modal hiển thị tiếng Việt đúng
- [ ] Notification hiển thị tiếng Việt đúng
- [ ] Database lưu tiếng Việt đúng
- [ ] Export Excel với tiếng Việt (nếu có)

## Liên hệ:
Nếu vẫn còn vấn đề, hãy kiểm tra:
1. Browser console (F12) có lỗi gì không
2. Network tab để xem encoding của responses
3. Database collation
4. Visual Studio encoding settings
