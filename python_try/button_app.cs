using System;
using System.Windows.Forms;

namespace ButtonApp
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      Application.SetHighDpiMode(HighDpiMode.SystemAware);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      // Create the form
      Form form = new Form
      {
        Text = "Button App",
        Width = 300,
        Height = 200
      };

      // Create a button
      Button button = new Button
      {
        Text = "Click Me",
        Location = new System.Drawing.Point(100, 70),
        Size = new System.Drawing.Size(100, 30)
      };

      // Add click event for the button
      button.Click += (sender, e) =>
      {
        MessageBox.Show("Button Pressed!");
      };

      // Add button to the form
      form.Controls.Add(button);

      // Run the application
      Application.Run(form);
    }
  }
}
