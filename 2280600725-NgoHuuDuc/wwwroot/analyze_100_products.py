import pandas as pd
import os

# Path to the Excel file
excel_path = os.path.join(os.path.dirname(__file__), "100_products_for_import.xlsx")

# Read the Excel file
df = pd.read_excel(excel_path)

# Print basic info
print(f"Total number of products: {len(df)}")

# Count products by category
print("\nProducts by category:")
category_counts = df["Danh mục"].value_counts()
print(category_counts)

# Count products with size information
has_size_info = df["Kích thước"] != "Không có thông tin"
print(f"\nProducts with size information: {has_size_info.sum()} ({has_size_info.sum()/len(df)*100:.1f}%)")
print(f"Products without size information: {(~has_size_info).sum()} ({(~has_size_info).sum()/len(df)*100:.1f}%)")

# Analyze size distribution
print("\nSize distribution:")
size_counts = {"S": 0, "M": 0, "L": 0, "XL": 0}

for size_info in df["Kích thước"]:
    if size_info != "Không có thông tin":
        size_pairs = size_info.split(",")
        for pair in size_pairs:
            size, qty = pair.split(":")
            size_counts[size] += 1

print("Number of products with each size:")
for size, count in size_counts.items():
    print(f"{size}: {count} products ({count/has_size_info.sum()*100:.1f}% of products with size info)")

# Print a few examples
print("\nSample products:")
print(df.head(5))
