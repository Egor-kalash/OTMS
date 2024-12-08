using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using LibGit2Sharp;
using System.Text.RegularExpressions;

namespace FunctionFinder
{
    public partial class Form1 : Form
    {
        private TextBox repoEntry;
        private TextBox folderEntry;
        private TextBox searchEntry;
        private FlowLayoutPanel leftPanel;
        private FlowLayoutPanel rightPanel;
        private string cloneDir = "cloned_repo";
        private string parentFilesDir = "Parent_files";
        private Dictionary<string, Label> parentFileLabels = new Dictionary<string, Label>();
        private List<Label> createdLabels = new List<Label>();
        private TextBox usernameEntry;
        private TextBox passwordEntry;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            UpdateParentFilesDisplay();
        }

        private void SetupUI()
        {
            Text = "Function Scraping Tool";
            Size = new Size(1000, 600);
            MinimumSize = new Size(1000, 600);
            MaximumSize = new Size(1000, 600);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Width = 1000
            };

            var topPanel = new FlowLayoutPanel
            {
                Width = 400,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                Anchor = AnchorStyles.None
            };

            leftPanel = new FlowLayoutPanel
            {
                Width = 300,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                Dock = DockStyle.Fill
            };

            rightPanel = new FlowLayoutPanel
            {
                Width = 300,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                Dock = DockStyle.Fill
            };

            // Repository URL
            var repoLabel = new Label { Text = "GitHub Repository URL:", AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            repoEntry = new TextBox { Width = 380, Margin = new Padding(0, 0, 0, 10) };

            var usernameLabel = new Label { Text = "Username:", AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            usernameEntry = new TextBox { Width = 380, Margin = new Padding(0, 0, 0, 10) };

            var passwordLabel = new Label { Text = "Password:", AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            passwordEntry = new TextBox { Width = 380, Margin = new Padding(0, 0, 0, 10), PasswordChar = '*' };

            // Target Folder
            var folderLabel = new Label { Text = "Target Folder Name:", AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            folderEntry = new TextBox { Width = 380, Margin = new Padding(0, 0, 0, 10) };

            // Scrape Button
            var scrapeButton = new Button 
            { 
                Text = "Run Function Scraping",
                Width = 380,
                Margin = new Padding(0, 0, 0, 20)
            };

            // Search
            var searchLabel = new Label { Text = "Search Function:", AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            searchEntry = new TextBox { Width = 380, Margin = new Padding(0, 0, 0, 10) };
            var findButton = new Button 
            { 
                Text = "Find Function",
                Width = 380,
                Margin = new Padding(0, 0, 0, 10)
            };

            scrapeButton.Click += (s, e) => RunFunctionScraping();
            findButton.Click += (s, e) => RunSearch();

            topPanel.Controls.AddRange(new Control[] {
                repoLabel,
                repoEntry,
                usernameLabel,
                usernameEntry, 
                passwordLabel,
                passwordEntry,
                folderLabel,
                folderEntry,
                scrapeButton,
                searchLabel,
                searchEntry,
                findButton
            });

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(topPanel, 1, 0);
            mainPanel.Controls.Add(rightPanel, 2, 0);
            
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            Controls.Add(mainPanel);
        }

        private void RunFunctionScraping()
        {
            string repoUrl = repoEntry.Text.Trim();
            string targetFolderName = folderEntry.Text.Trim();
            CloneAndExtract(repoUrl, targetFolderName);
        }
private void CloneAndExtract(string repoUrl, string targetFolderName)
        {
            try
            {
                if (Directory.Exists(cloneDir))
                    Directory.Delete(cloneDir, true);

                var cloneOptions = new CloneOptions
{
    CredentialsProvider = (_url, _user, _cred) =>
        new UsernamePasswordCredentials
        {
            Username = usernameEntry.Text,
            Password = passwordEntry.Text
        }
};

Repository.Clone(repoUrl, cloneDir, cloneOptions);


                string targetSubdir = FindFolderRecursive(cloneDir, targetFolderName);

                if (targetSubdir == null)
                {
                    MessageBox.Show($"Target folder '{targetFolderName}' not found in repository.");
                    return;
                }

                using (StreamWriter outfile = new StreamWriter("function_list.txt"))
                {
                    foreach (string sclFile in Directory.GetFiles(targetSubdir, "*.scl", SearchOption.AllDirectories))
                    {
                        foreach (string line in File.ReadAllLines(sclFile))
                        {
                            if (line.Contains("FUNCTION_BLOCK "))
                            {
                                int start = line.IndexOf('"') + 1;
                                int end = line.IndexOf('"', start);
                                if (start > 0 && end > start)
                                {
                                    string functionName = line.Substring(start, end - start).Trim();
                                    outfile.WriteLine($"{functionName} : {sclFile.Substring(12)}");
                                }
                            }
                        }
                    }
                }
                MessageBox.Show("Scraping completed successfully!");
                UpdateParentFilesDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void CopyDirectory(string sourceDir, string targetDir, bool copySubDirs)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFileName = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFileName, true);
            }

            if (copySubDirs)
            {
                foreach (string subdir in Directory.GetDirectories(sourceDir))
                {
                    string destSubDir = Path.Combine(targetDir, Path.GetFileName(subdir));
                    CopyDirectory(subdir, destSubDir, true);
                }
            }
        }
        private string FindFolderRecursive(string rootDir, string targetFolderName)
        {
            foreach (string dir in Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(dir) == targetFolderName)
                    return dir;
            }
            return null;
        }

        private void RunSearch()
        {
            string searchTerm = searchEntry.Text.Trim();
            SearchFunction(searchTerm);
        }

        private void SearchFunction(string searchTerm)
        {
            ClearLabels();
            int row = 0;

            foreach (string line in File.ReadAllLines("function_list.txt"))
            {
                if (Regex.IsMatch(line, searchTerm, RegexOptions.IgnoreCase))
                {
                    string[] splitLine = line.Split(new[] { ": " }, StringSplitOptions.None);
                    if (splitLine.Length == 2)
                    {
                        string relParentFilePath = splitLine[1].Trim();
                        string fncName = splitLine[0].Trim();
                        string sourceFile = Path.Combine(cloneDir, relParentFilePath);
                        string destinationFile = Path.Combine(parentFilesDir, Path.GetFileName(sourceFile));
                        CreateClickableLabel(fncName, sourceFile, destinationFile, row++);
                    }
                }
            }
        }

        private void CreateClickableLabel(string fncName, string sourceFile, string destinationFile, int row)
        {
            var label = new Label
            {
                Text = fncName,
                AutoSize = true,
                ForeColor = Color.Blue,
                Cursor = Cursors.Hand
            };
            label.Click += (s, e) => CopyFunction(sourceFile, destinationFile);
            label.Location = new Point(10, row * 25);
            leftPanel.Controls.Add(label);
            createdLabels.Add(label);
        }

        private void CopyFunction(string sourceFile, string destinationFile)
        {
            try
            {
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, destinationFile, true);
                    UpdateParentFilesDisplay();
                }
                else
                {
                    MessageBox.Show($"Source file not found: {sourceFile}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying file: {ex.Message}");
            }
        }

        private void UpdateParentFilesDisplay()
        {
            rightPanel.Controls.Clear();
            parentFileLabels.Clear();

            if (!Directory.Exists(parentFilesDir))
                Directory.CreateDirectory(parentFilesDir);

            int row = 0;
            foreach (string file in Directory.GetFiles(parentFilesDir))
            {
                var label = new Label
                {
                    Text = Path.GetFileName(file),
                    AutoSize = true,
                    ForeColor = Color.Green,
                    Cursor = Cursors.Hand,
                    Location = new Point(10, row++ * 25)
                };
                string fileName = Path.GetFileName(file);
                label.Click += (s, e) => RemoveFileFromParentFiles(fileName, file);
                rightPanel.Controls.Add(label);
                parentFileLabels[fileName] = label;
            }
        }

        private void RemoveFileFromParentFiles(string fileName, string filePath)
        {
            try
            {
                File.Delete(filePath);
                if (parentFileLabels.ContainsKey(fileName))
                {
                    parentFileLabels[fileName].Dispose();
                    parentFileLabels.Remove(fileName);
                }
                MessageBox.Show($"Removed {fileName} from Parent_files");
                UpdateParentFilesDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove {filePath}: {ex.Message}");
            }
        }

        private void ClearLabels()
        {
            foreach (var label in createdLabels)
            {
                label.Dispose();
            }
            createdLabels.Clear();
        }
    }
}