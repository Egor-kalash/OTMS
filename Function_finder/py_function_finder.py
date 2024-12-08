import shutil
import git
import os
import glob
import tkinter as tk
from datetime import datetime
from tkinter import messagebox
from re import IGNORECASE as IGC
from re import search as reSearch

# Define the window
window = tk.Tk()
window.title("Function Scraping Tool")
window.geometry("1000x600")


def clear_text(event):
    event.widget.delete(0, tk.END)

# Create a top frame for the repository input, target folder input, and buttons
top_frame = tk.Frame(window)
top_frame.grid(row=0, column=0, columnspan=3, pady=20)

# Bottom frame with left and right columns for function lists
bottom_frame = tk.Frame(window)
bottom_frame.grid(row=1, column=0, columnspan=3, sticky="nsew")

# Frames for source and copied functions lists
left_frame = tk.Frame(bottom_frame)
left_frame.grid(row=0, column=0, sticky="nsew", padx=10)

right_frame = tk.Frame(bottom_frame)
right_frame.grid(row=0, column=2, sticky="nsew", padx=10)

# Canvas and Scrollbar for left frame
left_canvas = tk.Canvas(left_frame)
left_canvas.grid(row=0, column=0, sticky="nsew")
left_scrollbar = tk.Scrollbar(left_frame, orient="vertical", command=left_canvas.yview)
left_scrollbar.grid(row=0, column=1, sticky="ns")
left_canvas.configure(yscrollcommand=left_scrollbar.set)
scrollable_left_frame = tk.Frame(left_canvas)
left_canvas.create_window((0, 0), window=scrollable_left_frame, anchor="nw")

scrollable_left_frame.bind("<Configure>", lambda e: left_canvas.configure(scrollregion=left_canvas.bbox("all")))

# Canvas and Scrollbar for right frame
right_canvas = tk.Canvas(right_frame)
right_canvas.grid(row=0, column=0, sticky="nsew")
right_scrollbar = tk.Scrollbar(right_frame, orient="vertical", command=right_canvas.yview)
right_scrollbar.grid(row=0, column=1, sticky="ns")
right_canvas.configure(yscrollcommand=right_scrollbar.set)
scrollable_right_frame = tk.Frame(right_canvas)
right_canvas.create_window((0, 0), window=scrollable_right_frame, anchor="nw")

scrollable_right_frame.bind("<Configure>", lambda e: right_canvas.configure(scrollregion=right_canvas.bbox("all")))

# Track created labels
created_labels = []
parent_file_labels = {}

# Function to clear labels from a previous search
def clear_labels():
    for label in created_labels:
        label.destroy()
    created_labels.clear()

# Function to copy files from source to destination
def copy_function(source_file, destination_file):
    if os.path.isfile(source_file):
        shutil.copy(source_file, destination_file)
        print(f"Copied {source_file} to {destination_file}")
        update_parent_files_display()
    else:
        messagebox.showerror("Error", f"Source file not found: {source_file}")

# Create a clickable label for a function in the source files
def create_clickable_label(fnc_name, source_file, destination_file, row):
    new_file_added = tk.Label(scrollable_left_frame, text=fnc_name, anchor="w", fg="blue", cursor="hand2")
    new_file_added.grid(row=row, column=0, sticky="w", padx=10, pady=5)
    new_file_added.bind("<Button-1>", lambda event: copy_function(source_file, destination_file))
    created_labels.append(new_file_added)

# Remove file from Parent_files directory
def remove_file_from_parent_files(fnc_name, file_path):
    try:
        os.remove(file_path)
        parent_file_labels[fnc_name].destroy()
        del parent_file_labels[fnc_name]
        messagebox.showinfo("Information", f"Removed {os.path.basename(file_path)} from Parent_files")
    except Exception as e:
        messagebox.showerror("Error", f"Failed to remove {file_path}: {str(e)}")

# Update the display of files in Parent_files directory
def update_parent_files_display():
    parent_files = 'Parent_files'
    if not os.path.exists(parent_files):
        os.makedirs(parent_files)
    for label in parent_file_labels.values():
        label.destroy()
    parent_file_labels.clear()
    row = 0
    for file_name in os.listdir(parent_files):
        file_path = os.path.join(parent_files, file_name)
        if os.path.isfile(file_path):
            label = tk.Label(scrollable_right_frame, text=file_name, anchor="w", fg="green", cursor="hand2")
            label.grid(row=row, column=0, sticky="w", padx=10, pady=5)
            parent_file_labels[file_name] = label
            label.bind("<Button-1>", lambda event, fn=file_name, fp=file_path: remove_file_from_parent_files(fn, fp))
            row += 1

# Function to recursively search for the target folder within the cloned repo
def find_folder_recursive(root_dir, target_folder_name):
    for dirpath, dirnames, filenames in os.walk(root_dir):
        if os.path.basename(dirpath) == target_folder_name:
            return dirpath
    return None

# Function to clone repository and extract functions
def clone_and_extract(repo_url, target_folder_name):
    global clone_dir
    clone_dir = 'cloned_repo'  # Directory where the repo is cloned

    # Remove old cloned repo if it exists
    if os.path.exists(clone_dir) and os.path.isdir(clone_dir):
        shutil.rmtree(clone_dir)
    
    # Clone the repository
    try:
        git.Repo.clone_from(repo_url, clone_dir)
        print(f"Repository cloned into {clone_dir}.")
    except git.exc.GitCommandError as e:
        print(f"Error during cloning: {e}")
        return
    
    # Recursively search for the target folder
    target_subdir = find_folder_recursive(clone_dir, target_folder_name)
    if target_subdir is None:
        messagebox.showerror("Error", f"Target folder '{target_folder_name}' not found in the repository.")
        return

    # Extract function blocks from the target folder
    with open('function_list.txt', 'w') as outfile:
        scl_files = glob.glob(os.path.join(target_subdir, '**', '*.scl'), recursive=True)
        for scl_file in scl_files:
            with open(scl_file, 'r') as infile:
                for line in infile:
                    if 'FUNCTION_BLOCK ' in line:
                        start = line.find('"') + 1
                        end = line.find('"', start)
                        if start > 0 and end > start:
                            function_name = line[start:end].strip()
                            outfile.write(f"{function_name} : {os.path.join(scl_file)[12:]}\n")
        messagebox.showinfo("Information", "Scraping completed successfully!")

# Function to search for a specific function name
def search_function(item):
    clear_labels()
    row = 0
    with open('function_list.txt') as outfile:
        for line in outfile:
            if reSearch(item, line, IGC):
                split_line = line.split(": ")
                if len(split_line) == 2:
                    rel_parent_file_path = split_line[1].strip()
                    fnc_name = split_line[0].strip()
                    source_file = os.path.join(clone_dir, rel_parent_file_path)
                    destination_file = os.path.join('Parent_files', os.path.basename(source_file))
                    create_clickable_label(fnc_name, source_file, destination_file, row)
                    row += 1

# GUI setup for input fields and buttons
def run_search():
    item = entry.get().strip()
    search_function(item)

def run_function_scraping():
    repo_url = repo_entry.get().strip()
    target_folder_name = folder_entry.get().strip()
    clone_and_extract(repo_url, target_folder_name)
    update_parent_files_display()

# GUI Components
# Repository URL input
tk.Label(top_frame, text="GitHub Repository URL:").pack(pady=2)
repo_entry = tk.Entry(top_frame, width=60)
repo_entry.pack(pady=2)

# Target folder name input
tk.Label(top_frame, text="Target Folder Name:").pack(pady=2)
folder_entry = tk.Entry(top_frame, width=60)
folder_entry.pack(pady=2)

# Run Scraping button
scrape_button = tk.Button(top_frame, text="Run Function Scraping", command=run_function_scraping)
scrape_button.pack(pady=5)

# Search entry and button
entry = tk.Entry(top_frame, width=30)
entry.pack(pady=5)
find_button = tk.Button(top_frame, text="Find Function", command=run_search)
find_button.pack(pady=5)

# Bind Cmd+Delete to clear text in repo_entry and folder_entry
repo_entry.bind("<Command-Delete>", clear_text)
folder_entry.bind("<Command-Delete>", clear_text)
entry.bind("<Command-Delete>", clear_text)

# Display any existing files in Parent_files when the app starts
update_parent_files_display()

# Run Tkinter event loop
window.mainloop()
