import tkinter as tk

def print_one():
  print("1")

window = tk.Tk()
window.title("Function Scraping Tool")
window.geometry("300x100")

# Create a button that triggers the function
scrape_button = tk.Button(window, text="Run Function Scraping", command=print_one)
scrape_button.pack(pady=20)

window.mainloop()