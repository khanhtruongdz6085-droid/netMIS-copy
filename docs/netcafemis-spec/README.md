# Đặc tả NetCafeMIS

Phiên bản 1.0, ngày 17/10/2026. Đối chiếu bản java.zip mới nhất và hai sơ đồ do người dùng cung cấp.

- [Đọc đặc tả đầy đủ](DAC_TA_NETCAFEMIS.md)
- [Tải Word](../../downloads/Dac-ta-NetCafeMIS.docx?raw=true)
- [Xem PDF 30 trang](../../downloads/Dac-ta-NetCafeMIS.pdf)

Tài liệu có 16 chương, 25 bảng và hai sơ đồ dựng lại theo code; mô tả Model/DAO/Service, 30 báo cáo, quy tắc nghiệp vụ, các trường hợp kiểm thử và bộ dữ liệu quán nhỏ. Bản Word dùng được để chỉnh sửa hoặc nhập vào Google Docs. Để nhập, tải file .docx rồi tải lên Google Drive và mở bằng Google Docs.

Sơ đồ PNG và SVG nằm trong images/, kèm nguồn Graphviz .dot. build_documents.py dùng Python (python-docx), Pandoc và Graphviz để xuất Word. Có thể xuất tiếp PDF bằng LibreOffice ở chế độ headless. Các công cụ này chỉ cần nếu muốn dựng lại định dạng từ nguồn Markdown; không cần để mở Word/PDF.

Không sửa mã nguồn ứng dụng trong lần viết tài liệu này.
