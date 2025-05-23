import pandas as pd
import random
import os

# Define categories
categories = [
    {"id": 1, "name": "Veston", "description": "Các loại veston xịn xịn"},
    {"id": 2, "name": "Quần tây", "description": "Các loại quần tây tây - chất chơi người dơi"},
    {"id": 3, "name": "Áo sơ mi", "description": "Áo sơ mi 2 trong 1"},
    {"id": 4, "name": "Áo Gile", "description": "Các loại áo Gile, phù hợp nhiều mục đích"},
    {"id": 5, "name": "Phụ Kiện", "description": "Các loại phụ kiện phù hợp cho từng sự kiện"}
]

# Define product name templates for each category
veston_templates = [
    "Veston {color} {style} {material}",
    "Áo vest {color} {style} {fit}",
    "Bộ vest {color} {occasion} {material}",
    "Veston {brand} {color} {fit}",
    "Áo blazer {color} {style} {material}"
]

quan_tay_templates = [  
    "Quần tây {color} {style} {material}",
    "Quần âu {color} {fit} {material}",
    "Quần tây {brand} {color} {fit}",
    "Quần âu {color} {style} {occasion}",
    "Quần tây {color} {material} {fit}"
]

ao_so_mi_templates = [
    "Áo sơ mi {color} {style} {material}",
    "Áo sơ mi {brand} {color} {fit}",
    "Áo sơ mi {color} {pattern} {occasion}",
    "Áo sơ mi {color} {sleeve} {material}",
    "Áo sơ mi {color} {style} {fit}"
]

ao_gile_templates = [
    "Áo gile {color} {style} {material}",
    "Áo gile {brand} {color} {fit}",
    "Áo gile {color} {pattern} {occasion}",
    "Áo gile {color} {style} {material}",
    "Áo gile {color} {style} {fit}"
]

phu_kien_templates = [
    "Cà vạt {color} {pattern} {material}",
    "Nơ {color} {style} {occasion}",
    "Khăn túi {color} {pattern} {material}",
    "Thắt lưng {color} {material} {brand}",
    "Cufflinks {color} {style} {material}"
]

# Define attributes for product names
colors = ["đen", "trắng", "xanh navy", "xanh dương", "xám", "be", "nâu", "xanh rêu", "đỏ đô", "xanh đen", "kẻ sọc", "họa tiết"]
styles = ["cổ điển", "hiện đại", "thanh lịch", "trẻ trung", "công sở", "dự tiệc", "casual", "slim fit", "regular fit"]
materials = ["len", "cotton", "linen", "polyester", "wool", "cashmere", "tweed", "kaki", "nhung", "denim", "da", "lụa", "satin"]
brands = ["Elegance", "Gentleman", "Classic", "Modern", "Premium", "Luxury", "Executive", "Business", "Formal"]
fits = ["ôm body", "regular fit", "slim fit", "oversize", "standard fit", "relaxed fit", "skinny fit"]
occasions = ["dự tiệc", "công sở", "cưới hỏi", "dạo phố", "lễ hội", "casual", "formal"]
patterns = ["trơn", "kẻ sọc", "kẻ caro", "họa tiết", "chấm bi", "in hoa", "thêu", "windowpane"]
sleeves = ["tay dài", "tay ngắn", "tay lỡ", "cộc tay"]

# Define price ranges for each category
price_ranges = {
    "Veston": (1500000, 5000000),
    "Quần tây": (500000, 1500000),
    "Áo sơ mi": (350000, 1200000),
    "Áo Gile": (600000, 2000000),
    "Phụ Kiện": (150000, 800000)
}

# Define sizes
sizes = ["S", "M", "L", "XL", "2XL"]

# Define description templates
description_templates = [
    "{product_name} chất liệu {material} cao cấp, thiết kế {style}, phù hợp cho {occasion}.",
    "{product_name} phong cách {style}, chất {material} mềm mại, thoáng mát, thích hợp {occasion}.",
    "{product_name} thiết kế {fit}, chất liệu {material} bền đẹp, phù hợp cho {occasion}.",
    "{product_name} kiểu dáng {style}, form {fit}, chất {material} cao cấp.",
    "{product_name} thiết kế tinh tế, chất liệu {material}, kiểu dáng {style}, phù hợp {occasion}."
]

# Generate products for each category
products = []

for category in categories:
    category_name = category["name"]
    
    for i in range(30):  # 30 products per category
        # Select appropriate template based on category
        if category_name == "Veston":
            template = random.choice(veston_templates)
        elif category_name == "Quần tây":
            template = random.choice(quan_tay_templates)
        elif category_name == "Áo sơ mi":
            template = random.choice(ao_so_mi_templates)
        elif category_name == "Áo Gile":
            template = random.choice(ao_gile_templates)
        else:  # Phụ Kiện
            template = random.choice(phu_kien_templates)
        
        # Generate product name
        product_name = template.format(
            color=random.choice(colors),
            style=random.choice(styles),
            material=random.choice(materials),
            brand=random.choice(brands),
            fit=random.choice(fits),
            occasion=random.choice(occasions),
            pattern=random.choice(patterns),
            sleeve=random.choice(sleeves)
        )
        
        # Generate price within category range
        min_price, max_price = price_ranges[category_name]
        price = random.randint(min_price // 10000, max_price // 10000) * 10000  # Round to nearest 10,000 VND
        
        # Generate size quantities
        size_quantities = {}
        total_quantity = 0
        
        for size in sizes:
            # Generate random quantity for each size (0-100)
            qty = random.randint(0, 100)
            size_quantities[size] = qty
            total_quantity += qty
        
        # Generate description
        description_template = random.choice(description_templates)
        description = description_template.format(
            product_name=product_name,
            material=random.choice(materials),
            style=random.choice(styles),
            fit=random.choice(fits),
            occasion=random.choice(occasions)
        )
        
        # Add size information to description
        size_info = ",".join([f"{size}:{qty}" for size, qty in size_quantities.items()])
        description_with_sizes = f"{description}\n\n[SIZES]{size_info}[/SIZES]"
        
        # Add product to list
        product_data = {
            "Tên sản phẩm": product_name,
            "Danh mục": category_name,
            "Giá": price,
            "Số lượng": total_quantity,
            "Mô tả": description_with_sizes
        }
        
        products.append(product_data)

# Create DataFrame
df = pd.DataFrame(products)

# Save to Excel
excel_path = os.path.join(os.path.dirname(__file__), "150_products_5_categories.xlsx")
df.to_excel(excel_path, index=False)

print(f"Excel file created at: {excel_path}")
