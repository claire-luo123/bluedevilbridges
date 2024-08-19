import csv

n = 44  # Number of rows and columns

# Create a 2D list filled with 0.0 values
matrix = [[0.0] * n for _ in range(n)]

# Set diagonal elements to 1.0
for i in range(n):
    matrix[i][i] = 1.0

# Path to save the CSV file
file_path = 'identity_matrix.csv'

# Write the matrix to CSV
with open(file_path, mode='w', newline='') as file:
    writer = csv.writer(file)
    
    # Write headers (column names)
    headers = [''] + [f'{i + 1}' for i in range(n)]  # Row and column headers
    writer.writerow(headers)
    
    # Write each row of the matrix
    for i in range(n):
        row = [f'{i + 1}'] + [f'{matrix[i][j]}' for j in range(n)]
        writer.writerow(row)

print(f"CSV file '{file_path}' has been generated with {n} rows and columns. Diagonal elements are '1.0'.")
