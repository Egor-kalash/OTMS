using System;
using System.Diagnostics;
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
                Text = "Run Python Script",
                Width = 300,
                Height = 200
            };

            // Create a button
            Button button = new Button
            {
                Text = "Run Python Script",
                Location = new System.Drawing.Point(100, 70),
                Size = new System.Drawing.Size(150, 30)
            };

            // Add click event handler to the button
            button.Click += (sender, e) =>
            {
                RunPythonScript();
            };

            // Add the button to the form
            form.Controls.Add(button);

            // Run the application
            Application.Run(form);
        }

        // Method to run the Python script
        static void RunPythonScript()
        {
            try
            {
                // Create the process to run the Python script
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python"; // Assumes python is in the PATH environment
                start.Arguments = @"..\py_git_traverser.py"; // Relative path from ButtonApp to py_git_traverser.py
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;

                using (Process? process = Process.Start(start))
                {
                    if (process != null)
                    {
                        // Read and output standard output from the Python script
                        using (System.IO.StreamReader reader = process.StandardOutput)
                        {
                            string result = reader.ReadToEnd();
                            MessageBox.Show(result);
                        }

                        // Read and output errors from the Python script
                        using (System.IO.StreamReader errorReader = process.StandardError)
                        {
                            string errors = errorReader.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(errors))
                            {
                                MessageBox.Show($"Errors: {errors}");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to start the process.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running script: {ex.Message}");
            }
        }
    }
}
