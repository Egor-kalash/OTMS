import tkinter as tk
from tkinter import messagebox

def perform_search():
    # Collect the input text from the search field
    search_term = search_entry.get()
    # Save the collected text in a separate variable (here displayed via message box for demonstration)
    messagebox.showinfo("Search Term", f"You searched for: {search_term}")

# Create the main window
window = tk.Tk()
window.title("Search App")
window.geometry("1000x1000")

# Create a label
label = tk.Label(window, text="Enter the function name")
label.pack(pady=5)

# Create a search entry field
search_entry = tk.Entry(window, width=30)
search_entry.pack(pady=5)

# Create a search button
search_button = tk.Button(window, text="Search", command=perform_search)
search_button.pack(pady=10)

# Run the GUI loop
window.mainloop()



