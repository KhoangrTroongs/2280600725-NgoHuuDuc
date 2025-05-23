import pandas as pd
import os

# Path to the Excel file
excel_path = os.path.join(os.path.dirname(__file__), "100_products_with_sizes.xlsx")

# Read the Excel file
df = pd.read_excel(excel_path)

# Print the first 3 rows
print("First 3 rows:")
print(df.head(3).to_string())

# Print column names
print("\nColumn names:")
print(df.columns.tolist())

# Print summary statistics for numeric columns
print("\nSummary statistics:")
print(df[["Giá", "Size S", "Size M", "Size L", "Size XL", "Size 2XL", "Tổng số lượng"]].describe())

# Count products by category
print("\nProducts by category:")
print(df["Danh mục"].value_counts())

# Check for missing values
print("\nMissing values:")
print(df.isnull().sum())

# Print total number of products
print(f"\nTotal number of products: {len(df)}")

# Check if size quantities match total quantity
print("\nVerifying size quantities match total quantity:")
calculated_total = df["Size S"] + df["Size M"] + df["Size L"] + df["Size XL"] + df["Size 2XL"]
matches = (calculated_total == df["Tổng số lượng"]).all()
print(f"All size quantities match total: {matches}")

# Check if size information is correctly included in description
print("\nChecking size information in description:")
size_tag_count = df["Mô tả"].str.contains(r"\[SIZES\].*\[/SIZES\]").sum()
print(f"Products with size tags in description: {size_tag_count} out of {len(df)}")

# Print a sample description to verify format
print("\nSample description with size information:")
print(df["Mô tả"].iloc[0])
