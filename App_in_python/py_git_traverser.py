import shutil
import git
import os
import glob
import tkinter as tk
from datetime import datetime
from tkinter import messagebox

# Define the Git repository URL and the local directory to clone into
repo_url = 'https://github.com/yuriyOtomakeit/LOMS_TIA.git'  # Replace with your repository URL
clone_dir = 'cloned_repo'  # This will be the directory where the repo is cloned

current_time = datetime.now().strftime('%Y-%m-%d_%H:%M:%S')
# If the Repo was already cloned in this folder it will be replaced by its newest version
if os.path.exists(clone_dir) and os.path.isdir(clone_dir):
    shutil.rmtree(clone_dir)
    print(f"Old {clone_dir} has been removed.")
else:
    print(f"{clone_dir} does not exist or is not a directory.")

# Clone the repository if it hasn't been cloned already
print(f"Cloning repository from {repo_url} into {clone_dir}...")
try:
    git.Repo.clone_from(repo_url, clone_dir)
    print(f"Repository cloned into {clone_dir} +1.")
except git.exc.GitCommandError as e:
    print(f"Error during cloning: {e}")
    exit(1)

# Define the subdirectory you want to specifically target
target_subdir = os.path.join(clone_dir, 'TIA_Portal_Library', 'Classes')

# Define the number of characters to remove from the start of each line
chars_to_remove = 16  # Adjust this number as needed
chars_rm_back = -2
# Define the output file path

output_file = 'function_list.txt'
logs_folder = 'logs'
if not os.path.exists(logs_folder):
    os.makedirs(logs_folder)
output_file_log = os.path.join(logs_folder, f'output_{current_time}.txt')


# Check if the target subdirectory exists
if os.path.exists(target_subdir):
    print(f"Processing .scl files in the subdirectory: {target_subdir}")

    # Open the output file for writing
    with open(output_file, 'w') as outfile:
        # Find all .scl files recursively in the target subdirectory and its subdirectories
        scl_files = glob.glob(os.path.join(target_subdir, '**', '*.scl'), recursive=True)

        # Iterate through each .scl file found
        for scl_file in scl_files:
            print(f"Reading file: {scl_file} ...")
            with open(scl_file, 'r') as infile:
                # Iterate through each line in the file
                for line in infile:
                    # Check if the keyword "FUNCTION_BLOCK" is in the line (case-sensitive)
                    if 'FUNCTION_BLOCK ' in line:
                        print(f"Match found in {scl_file}: {line.strip()}")  # Debugging: print the matching line
                        # Write the modified line to the output file, including the file name for context
                        if 'FUNCTION_BLOCK' in line:
                          # Find the content between the first pair of quotes
                          start = line.find('"') + 1
                          end = line.find('"', start)
                          if start > 0 and end > start:
                              function_name = line[start:end].strip()
                              # Write just the function name to the output file
                              outfile.write(f"{function_name} : {os.path.join(scl_file)[12:]}\n")
                              with open(output_file_log, 'a') as log_file:
                                log_file.write(f"{function_name} : {os.path.join(scl_file)[12:]}\n")
                              

    print(f"Modified lines containing 'FUNCTION_BLOCK' have been saved to {output_file}.")
else:
    print(f"Subdirectory {target_subdir} does not exist. Please check the path.")

messagebox.showinfo("Information", "Scraping completed successfully!")
