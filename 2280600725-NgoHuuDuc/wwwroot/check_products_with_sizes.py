import pandas as pd
import os

# Path to the Excel file
excel_path = os.path.join(os.path.dirname(__file__), "10_products_with_sizes_in_description.xlsx")

# Read the Excel file
df = pd.read_excel(excel_path)

# Print all rows
print("All products:")
print(df.to_string())

# Print column names
print("\nColumn names:")
print(df.columns.tolist())

# Check for missing values
print("\nMissing values:")
print(df.isnull().sum())

# Print total number of products
print(f"\nTotal number of products: {len(df)}")

# Check if size information is correctly included in description
print("\nChecking size information in description:")
size_tag_count = df["Mô tả"].str.contains(r"\[SIZES\].*\[/SIZES\]").sum()
print(f"Products with size tags in description: {size_tag_count} out of {len(df)}")

# Print descriptions to verify format
print("\nDescriptions with size information:")
for i, desc in enumerate(df["Mô tả"]):
    print(f"\nProduct {i+1}:")
    print(desc)
