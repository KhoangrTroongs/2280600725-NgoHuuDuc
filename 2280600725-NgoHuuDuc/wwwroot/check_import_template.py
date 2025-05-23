import pandas as pd
import os

# Path to the Excel file
excel_path = os.path.join(os.path.dirname(__file__), "import_template_matching_export.xlsx")

# Read the Excel file
df = pd.read_excel(excel_path)

# Print all rows
print("All products:")
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
