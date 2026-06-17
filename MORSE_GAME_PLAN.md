# Kế hoạch: Game Mã Morse (bản giải mã tự do)

Tham khảo: https://www.y8.com/games/morse_gro
Input: phím **Space**. Mục tiêu: gõ Morse → hiện chữ lên màn hình.

## 1. Cơ chế timing (lõi)

Đơn vị gốc `unit` ~ 0.15s. Các ngưỡng (có thể tinh chỉnh):

| Hành động | Ngưỡng | Kết quả |
|---|---|---|
| Giữ Space ngắn | < 0.3s | thêm `.` (dot) |
| Giữ Space lâu | >= 0.3s | thêm `-` (dash) |
| Nhả tay (im lặng) | ~0.7s | kết thúc chữ cái → decode |
| Nhả tay lâu hơn | ~1.5s | thêm dấu cách (hết từ) |

Logic trong `Update()`:
1. Vừa nhấn Space → lưu thời điểm bắt đầu, reset bộ đếm im lặng.
2. Vừa nhả Space → đo thời gian giữ → thêm `.` hoặc `-` vào buffer chữ hiện tại; bật đếm im lặng.
3. Đang nhả → cộng dồn thời gian im lặng:
   - đủ ngưỡng hết chữ → decode buffer, nối vào kết quả, xóa buffer.
   - đủ ngưỡng hết từ → thêm dấu cách.

## 2. Cấu trúc script

- `MorseCode.cs` — Dictionary<string,char> A-Z, 0-9 + hàm `Decode(string)`.
- `MorseInput.cs` — đo timing Space, cập nhật UI.

## 3. UI (Canvas)

- Text "Đang gõ": buffer hiện tại (vd `.-`).
- Text "Kết quả": chuỗi đã giải mã.
- Đèn báo: tròn sáng khi giữ Space.
- (Tùy chọn) Bảng tra Morse.

## 4. Lộ trình

1. Tạo `MorseCode.cs`.
2. Tạo `MorseInput.cs`.
3. Tạo scene + Canvas + UI, nối script.
4. Chạy thử, tinh chỉnh ngưỡng thời gian.
5. (Sau) Âm thanh beep + hiệu ứng + game hóa (đề bài, điểm số).

## Bảng Morse tham khảo

A .-    B -...  C -.-.  D -..   E .     F ..-.  G --.
H ....  I ..    J .---  K -.-   L .-..  M --    N -.
O ---   P .--.  Q --.-  R .-.   S ...   T -     U ..-
V ...-  W .--   X -..-  Y -.--  Z --..
1 .---- 2 ..--- 3 ...-- 4 ....- 5 ..... 6 -.... 7 --... 8 ---.. 9 ----. 0 -----
