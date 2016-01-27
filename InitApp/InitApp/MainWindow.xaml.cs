using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;

namespace InitApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, bool> _eMails = new Dictionary<string, bool>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnStartClick(object sender, RoutedEventArgs e)
        {
            List<string> validEmails;
            List<string> inValidEmails;
            SeleniumDriver.Program.Run(_eMails, out validEmails, out inValidEmails);

            // Configure save file dialog box
            var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "ValidEmails.txt",
                    Filter = "Text File | *.txt"
                };

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                var writer = new StreamWriter(dlg.OpenFile());
                foreach (var email in validEmails)
                {
                    writer.WriteLine(email);
                }
                writer.Dispose();
                writer.Close();
            }

            // Configure save file dialog box
            dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "InValidEmails.txt",
                Filter = "Text File | *.txt"
            };

            // Show save file dialog box
            result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                var writer = new StreamWriter(dlg.OpenFile());
                foreach (var email in inValidEmails)
                {
                    writer.WriteLine(email);
                }
                writer.Dispose();
                writer.Close();
            }
        }

        private void BtnStopClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void BtnSelectEmailsClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dlg = new Microsoft.Win32.OpenFileDialog {DefaultExt = ".txt", Filter = "Text documents (.txt)|*.txt"};

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result != true) return;

            // Open document 
            var allEmailsStream = dlg.OpenFile();

            using (var reader = new System.IO.StreamReader(allEmailsStream))
            {
                while (!reader.EndOfStream)
                {
                    var email = reader.ReadLine();
                    if (!string.IsNullOrEmpty(email) && !_eMails.ContainsKey(email))
                    {
                        _eMails.Add(email, false);
                    }
                }
            }

            allEmailsStream.Close();
        }
    }
}
