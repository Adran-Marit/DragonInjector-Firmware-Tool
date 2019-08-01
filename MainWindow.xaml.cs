using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System;

namespace DragonInjector_Firmware_Tool
{
    public partial class MainWindow : Window
    {
        string uf2File;
        string uf2ShortFile;
        readonly string defaultUF2File = Directory.GetCurrentDirectory() + "\\defaultpayload.uf2";
        readonly string bootloader = Directory.GetCurrentDirectory() + "\\defaultbootloader.uf2";
        
        public MainWindow()
        {
            InitializeComponent();

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    if (d.VolumeLabel == "DRAGONBOOT")
                    {
                        DriveBox.Items.Add(d.Name);
                        OutputBox.Content += "\n\\:Found drive: " + (d.Name).Replace(":\\", "");
                        OutputBox.ScrollToBottom();
                    }
                }
                if (d.IsReady == false)
                {
                    //OutputBox.Content += "\n.No drives found";
                    //OutputBox.ScrollToBottom();
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception theException = e.Exception;
            string theErrorPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\GeneratorTestbedError.txt";
            using (System.IO.TextWriter theTextWriter = new System.IO.StreamWriter(theErrorPath, true))
            {
                DateTime theNow = DateTime.Now;
                theTextWriter.WriteLine("The error time: " + theNow.ToShortDateString() + " " + theNow.ToShortTimeString());
                while (theException != null)
                {
                    theTextWriter.WriteLine("Exception: " + theException.ToString());
                    theException = theException.InnerException;
                }
            }
            MessageBox.Show("The program crashed.  A stack trace can be found at:\n" + theErrorPath);
            e.Handled = true;
            Application.Current.Shutdown();
        }

        private void DriveButton_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Content += "\n...Scanning";
            int selectedIndex = DriveBox.SelectedIndex;
            DriveBox.Items.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    if (d.VolumeLabel == "DRAGONBOOT")
                    {
                        DriveBox.Items.Add(d.Name);
                        OutputBox.Content += "\n\\:Found drive: " + (d.Name).Replace(":\\", "");
                        OutputBox.ScrollToBottom();
                    }
                }
                if (d.IsReady == false)
                {
                    //OutputBox.Content += "\n.No drives found";
                    //OutputBox.ScrollToBottom();
                }
            }
            DriveBox.SelectedIndex = selectedIndex;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }

        private void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Content += "\n...Checking for updates";
            OutputBox.ScrollToBottom();
            LatestBootloaderVersionLabel.Text = "v1.1";
            LatestFirmwareVersionLabel.Text = "DRAGONBOOT v1.3";
        }

        private void FlashButton_Click(object sender, RoutedEventArgs e)
        {
            string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
            if (uf2File != null)
            {
                OutputBox.Content += "\n\\:Copying " + uf2ShortFile + " to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(uf2File, dest, true);
            }
            else
            {
                OutputBox.Content += "\n\\:Copying default payload to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(defaultUF2File, dest, true);
            }
        }

        private void FlashAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (uf2File != null)
            {
                foreach (var item in DriveBox.Items)
                {
                    string dest = item.ToString() + "\\flash.uf2";
                    OutputBox.Content += "\n\\:Copying " + uf2ShortFile + " to " + item.ToString().Replace(":\\", "");
                    OutputBox.ScrollToBottom();
                    File.Copy(uf2File, dest, true);
                }
            }
            else
            {
                foreach (var item in DriveBox.Items)
                {
                    string dest = item.ToString() + "\\flash.uf2";
                    OutputBox.Content += "\n\\:Copying default payload to " + item.ToString().Replace(":\\", "");
                    OutputBox.ScrollToBottom();
                    File.Copy(defaultUF2File, dest, true);
                }
            }
        }

        private void BootloaderButton_Click(object sender, RoutedEventArgs e)
        {
            string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
            OutputBox.Content += "\n\\:Updating bootloader on " +  DriveBox.SelectedItem.ToString().Replace(":\\", "");
            OutputBox.ScrollToBottom();
            File.Copy(bootloader, dest, true);

        }

        private void BootloaderAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in DriveBox.Items)
            {
                string dest = item.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Updating bootloader on " + (item.ToString()).Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(bootloader, dest, true);
            }
        }

        private void Drag_Click(object sender, RoutedEventArgs e)
        {
            Window.DragMove();
        }

        private void PayloadTextBox_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            {
                openFileDialog.Filter = "UF2 (*.uf2)|*.uf2";
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    uf2ShortFile = (Path.GetFileName(filePath)).ToString();
                    uf2File = Path.GetFullPath(filePath).ToString();
                    PayloadTextBox.Text = uf2ShortFile;
                    OutputBox.Content += "\nGot payload: " + (Path.GetFileName(filePath)).ToString();
                    OutputBox.ScrollToBottom();
                }
            }
        }

        private void DriveBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                string selectedItem = (DriveBox.SelectedItem).ToString();
                StreamReader infoUF2 = new System.IO.StreamReader(selectedItem + "INFO_UF2.TXT");
                string infoUF2Line = infoUF2.ReadLine();
                if (infoUF2Line.Contains("DragonInjector UF2 Bootloader"))
                {
                    BootloaderVersionLabel.Text = (infoUF2Line.Replace("DragonInjector UF2 Bootloader ", ""));
                }
                else
                {
                    BootloaderVersionLabel.Text = "Custom";
                }
            }
            catch
            {
            } 
        }

        private void PayloadButton_Click(object sender, RoutedEventArgs e)
        {
            PayloadTextBox.Clear();
            OutputBox.Content += "\n...Using default payload";
            OutputBox.ScrollToBottom();
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.dragoninjector.com");
        }
    }
}

/*
TODO:

Really need to fix and add try/catch, especially where drive scan happens
Check if default files exist first
Add customization options = show boot logo, show path only, no visual feedback
Figure out how to pull firmware version
Add "pressed" states to buttons
Make check for updates actually, you know, check for updates
Possibly add more verbose output
Remove annoying border when drive list is empty
Add output for no drive(s) selected and fix crash for it
Add window title
Fix output when no drives found and device not ready
*/