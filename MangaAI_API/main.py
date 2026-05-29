from fastapi import FastAPI, File, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from ultralytics import YOLO
from PIL import Image
import io

app = FastAPI(title="Manga AI Segmentation API")

# Mở khóa CORS cho Frontend gọi
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], 
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==========================================
# 1. LOAD MÔ HÌNH YOLO11 CỦA BẠN
# ==========================================
# Đảm bảo file model (ví dụ: 'best.pt') đang nằm cùng thư mục với file main.py
model = YOLO("best.pt")

@app.post("/api/segment")
async def segment_manga(file: UploadFile = File(...)):
    # Đọc ảnh từ Request
    image_bytes = await file.read()
    image = Image.open(io.BytesIO(image_bytes)).convert("RGB")

    # 2. Đưa ảnh qua YOLO11 để nhận diện
    results = model(image)
    
    regions = []
    # results là một list, lấy kết quả của bức ảnh đầu tiên
    for result in results:
        boxes = result.boxes
        for box in boxes:
            # YOLO lấy tọa độ dạng x_min, y_min, x_max, y_max
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            conf = float(box.conf[0])
            cls_id = int(box.cls[0])
            
            # 3. Tính toán Width và Height chuẩn cho HTML Canvas (Góc trái trên cùng)
            w = x2 - x1
            h = y2 - y1
            
            # Lấy tên nhãn (VD: 'speech-balloon', 'text', 'panel')
            class_name = model.names[cls_id]

            # Ép tên class về đúng format mà C# đang kỳ vọng
            if class_name != "speech-balloon":
                class_name = "speech-balloon"

            regions.append({
                "x": int(x1),      # Đổi từ round(x1, 2) thành int(x1)
                "y": int(y1),      # Đổi từ round(y1, 2) thành int(y1)
                "width": int(w),   # Đổi từ round(w, 2) thành int(w)
                "height": int(h),  # Đổi từ round(h, 2) thành int(h)
                "confidence": round(conf, 4), # Giữ nguyên cái này vì nó là tỷ lệ %
                "region_type": class_name
            })
            
    return {
        "status": "success",
        "total_regions": len(regions),
        "data": regions
    }

# ====================================================================

import json
from fastapi import Form
import easyocr
from deep_translator import GoogleTranslator

# Khởi tạo bộ đọc chữ OCR hỗ trợ tiếng Nhật (ja) và tiếng Anh (en)
# Lần đầu chạy hàm dịch, thư viện sẽ tự động tải model OCR về máy bạn
ocr_reader = easyocr.Reader(['ja', 'en'], gpu=False) # Nếu máy bạn có card đồ họa rời Nvidia thì đổi thành gpu=True

# 1. SỬA DÒNG NÀY: Truyện của bạn là tiếng Anh, nên xóa 'ja' đi để AI khỏi bị ảo giác
ocr_reader = easyocr.Reader(['en'], gpu=False) 

@app.post("/api/translate")
async def translate_manga(file: UploadFile = File(...), regions_json: str = Form(...)):
    image_bytes = await file.read()
    image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    
    regions = json.loads(regions_json)
    translated_results = []
    
    for region in regions:
        x, y, w, h = region["x"], region["y"], region["width"], region["height"]
        
        # Cắt ảnh
        cropped_img = image.crop((x, y, x + w, y + h))
        
        # 2. THÊM DÒNG NÀY: Phóng to ảnh lên 2 lần để AI "sáng mắt" dễ đọc chữ hơn
        cropped_img = cropped_img.resize((w * 2, h * 2), Image.Resampling.LANCZOS)
        
        img_byte_arr = io.BytesIO()
        cropped_img.save(img_byte_arr, format='PNG')
        cropped_bytes = img_byte_arr.getvalue()
        
        # 3. SỬA DÒNG NÀY: Thêm paragraph=True để AI tự ghép các dòng đứt đoạn lại với nhau
        ocr_text_list = ocr_reader.readtext(cropped_bytes, detail=0, paragraph=True)
        original_text = " ".join(ocr_text_list).strip()
        
        translated_text = ""
        if original_text:
            try:
                translated_text = GoogleTranslator(source='auto', target='vi').translate(original_text)
            except Exception:
                translated_text = "[Lỗi dịch]"
        
        translated_results.append({
            "pageRegionId": region["pageRegionId"],
            "translated_text": translated_text
        })
        
    return {"status": "success", "data": translated_results}