import tkinter as tk
import os
from datetime import datetime

def run_function_scraping():
    os.system("python3 py_git_traverser.py")
    print("Function scraping executed.")

# Create a simple Tkinter window
window = tk.Tk()
window.title("Function Scraping Tool")
window.geometry("300x100")

# Create a button that triggers the function
scrape_button = tk.Button(window, text="Run Function Scraping", command=run_function_scraping)
scrape_button.pack(pady=20)

# Start the Tkinter event loop
window.mainloop()
