import glob
import os

# Get all .scl files in the current directory
scl_files = glob.glob('*.scl')

# Define the output file path
output_file = 'output.txt'

# Open the output file for writing
with open(output_file, 'w') as outfile:
    # Iterate through each .scl file in the directory
    for scl_file in scl_files:
        # Open each .scl file for reading
        with open(scl_file, 'r') as infile:
            # Iterate through each line in the file
            for line in infile:
                # Check if the keyword "PROGRAM" is in the line (case-sensitive)
                if 'FUNCTION_BLOCK ' in line:
                    # Write the matching line to the output file
                    outfile.write(f"{scl_file}: {line[16:-2]} \n")

print(f"Lines containing 'FUNCTION_BLOCK' from .scl files have been saved to {output_file}.")
