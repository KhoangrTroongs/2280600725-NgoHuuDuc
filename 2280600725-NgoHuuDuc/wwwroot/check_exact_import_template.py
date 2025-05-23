import pandas as pd
import os

# Path to the Excel file
excel_path = os.path.join(os.path.dirname(__file__), "import_template_exact.xlsx")

# Read the Excel file
df = pd.read_excel(excel_path)

# Print all rows
print("All products in sample template:")
print(df)

# Print column names
print("\nColumn names:")
print(df.columns.tolist())

# Check for missing values
print("\nMissing values:")
print(df.isnull().sum())

# Print total number of products
print(f"\nTotal number of products: {len(df)}")

# Check size information format
print("\nSize information format:")
for i, size_info in enumerate(df["Kích thước"]):
    print(f"Product {i+1}: {size_info}")

# Now check the 100 products file
full_excel_path = os.path.join(os.path.dirname(__file__), "100_products_exact_format.xlsx")
df_full = pd.read_excel(full_excel_path)

# Print basic info
print("\n\nFull template (100 products):")
print(f"Total number of products: {len(df_full)}")

# Count products by category
print("\nProducts by category:")
category_counts = df_full["Danh mục"].value_counts()
print(category_counts)

# Count products with size information
has_size_info = df_full["Kích thước"] != "Không có thông tin"
print(f"\nProducts with size information: {has_size_info.sum()} ({has_size_info.sum()/len(df_full)*100:.1f}%)")
print(f"Products without size information: {(~has_size_info).sum()} ({(~has_size_info).sum()/len(df_full)*100:.1f}%)")

# Analyze size distribution
print("\nSize distribution:")
size_counts = {"S": 0, "M": 0, "L": 0, "XL": 0}

for size_info in df_full["Kích thước"]:
    if size_info != "Không có thông tin":
        size_pairs = size_info.split(",")
        for pair in size_pairs:
            size, qty = pair.split(":")
            size_counts[size] += 1

print("Number of products with each size:")
for size, count in size_counts.items():
    print(f"{size}: {count} products ({count/has_size_info.sum()*100:.1f}% of products with size info)")

# Print a few examples
print("\nSample products from full template:")
print(df_full.head(3))
