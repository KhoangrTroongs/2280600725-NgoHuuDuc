import pandas as pd
import os

# Path to the Excel file
excel_path = os.path.join(os.path.dirname(__file__), "100_products_sample.xlsx")

# Read the Excel file
df = pd.read_excel(excel_path)

# Print the first 5 rows
print("First 5 rows:")
print(df.head())

# Print column names
print("\nColumn names:")
print(df.columns.tolist())

# Print summary statistics
print("\nSummary statistics:")
print(df.describe())

# Count products by category
print("\nProducts by category:")
print(df["Danh mục"].value_counts())

# Check for missing values
print("\nMissing values:")
print(df.isnull().sum())

# Print total number of products
print(f"\nTotal number of products: {len(df)}")
