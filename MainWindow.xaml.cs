using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using Octokit;
using System.Text.RegularExpressions;

namespace DragonInjector_Firmware_Tool
{
    public partial class MainWindow : Window
    {
        string uf2File;
        string uf2ShortFile;
        readonly string defaultUF2File = Directory.GetCurrentDirectory() + "\\payloads\\defaultpayload.uf2";
        readonly string bootloader = Directory.GetCurrentDirectory() + "\\payloads\\defaultbootloader.uf2";
        
        public MainWindow()
        {
            InitializeComponent();
            GetDrives();
        }

        private void DriveButton_Click(object sender, RoutedEventArgs e)
        {
            GetDrives();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }

        private void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            GetReleasesAsync();
        }

        private void FlashButton_Click(object sender, RoutedEventArgs e)
        {
            if (uf2File != null && DriveBox.Text != "Leave blank for DragonBoot" && DriveBox.SelectedItem != null)
            {
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Copying " + uf2ShortFile + " to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(uf2File, dest, true);
            }
            else if (DriveBox.SelectedItem != null && File.Exists(".\\payloads\\defaultpayload.uf2") && uf2File == null || DriveBox.Text == "Leave blank for DragonBoot")
            {
                OutputBox.Content += "\n...Using default payload";
                OutputBox.ScrollToBottom();
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Copying default payload to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(defaultUF2File, dest, true);
            }
            else if (!File.Exists(".\\payloads\\defaultpayload.uf2"))
            {
                OutputBox.Content += "\n!Missing default payload in directory";
                OutputBox.ScrollToBottom();
            }
            else
            {
                OutputBox.Content += "\n!No drive selected";
                OutputBox.ScrollToBottom();
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
            if (DriveBox.SelectedItem != null && File.Exists(".\\payloads\\defaultbootloader.uf2"))
            {
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Updating bootloader on " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(bootloader, dest, true);
            }
            else if (!File.Exists(".\\payloads\\defaultbootloader.uf2"))
            {
                OutputBox.Content += "\n!Missing default bootloader in directory";
                OutputBox.ScrollToBottom();
            }
            else
            {
                OutputBox.Content += "\n!No drive selected";
                OutputBox.ScrollToBottom();
            }
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
            GetDIVersions();
        }

        private void PayloadButton_Click(object sender, RoutedEventArgs e)
        {
            PayloadTextBox.Clear();
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.dragoninjector.com");
        }

        private async System.Threading.Tasks.Task GetReleasesAsync()
        {
            OutputBox.Content += "\n...Checking for updates";
            OutputBox.ScrollToBottom();
            var regex = new Regex(@"\d*\.\d*");

            var githubFW = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesFW = await githubFW.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-Firmware");
            var releaseFW = releasesFW[0];
            
            string fwVersion = regex.Match(releaseFW.Name.ToString()).ToString();
            OutputBox.Content += "\n" + "Found firmware release: " + fwVersion;
            OutputBox.ScrollToBottom();
            LatestFirmwareVersionLabel.Text = fwVersion;
            
            var githubBL = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesBL = await githubBL.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-Bootloader");
            var releaseBL = releasesBL[0];
            string blVersion = regex.Match(releaseBL.Name.ToString()).ToString();
            OutputBox.Content += "\n" + "Found bootloader release: " + blVersion;
            OutputBox.ScrollToBottom();
            LatestBootloaderVersionLabel.Text = blVersion;
        }
        
        private void GetDrives()
        {
            OutputBox.Content += "\n...Scanning for drives";
            OutputBox.ScrollToBottom();
            int selectedIndex = DriveBox.SelectedIndex;
            DriveBox.Items.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            int badDrive = 0;
            int goodDrive = 0;
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady)
                {
                    if (d.VolumeLabel == "DRAGONBOOT")
                    {
                        DriveBox.Items.Add(d.Name);
                        OutputBox.Content += "\n\\:Found drive: " + (d.Name).Replace(":\\", "");
                        OutputBox.ScrollToBottom();
                        goodDrive++;
                    }
                    else
                    {
                        badDrive++;
                    }
                }
            }
            if (badDrive > 0 && goodDrive == 0)
            {
                OutputBox.Content += "\n.No drives found";
                OutputBox.ScrollToBottom();
            }
            DriveBox.SelectedIndex = selectedIndex;
        }

        private void GetDIVersions()
        {
            string selectedItem = (DriveBox.SelectedItem).ToString();
            StreamReader currentUF2 = new System.IO.StreamReader(selectedItem + "CURRENT.UF2");

            string lineX;
            int x = 0;
            while ((lineX = currentUF2.ReadLine()) != null)
            {
                if (lineX.Contains("DI_FW_"))
                {
                    var regex = new Regex(@"DI_FW_\d*\.\d*");
                    string version = (regex.Match(lineX).ToString()).Replace("DI_FW_", "");
                    FirmwareVersionLabel.Text = version;
                    OutputBox.Content += "\nFound firmware version: " + version;
                    OutputBox.ScrollToBottom();
                    x++;
                }
            }
            if (x < 1)
            {
                FirmwareVersionLabel.Text = "Custom";
            }

            currentUF2.DiscardBufferedData();
            currentUF2.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            string lineY;
            int y = 0;
            while ((lineY = currentUF2.ReadLine()) != null)
            {
                if (lineY.Contains("DI_BL_"))
                {
                    var regex = new Regex(@"DI_BL_\d*\.\d*");
                    string version = (regex.Match(lineY).ToString()).Replace("DI_BL_", "");
                    BootloaderVersionLabel.Text = version;
                    OutputBox.Content += "\nFound bootloader version: " + version;
                    OutputBox.ScrollToBottom();
                    y++;
                }
            }
            if (y < 1)
            {
                BootloaderVersionLabel.Text = "Custom";
            }
            currentUF2.Dispose();
        }
    }
}

/*
TODO:
Add customization options = show boot logo, show path only, no visual feedback
Add "pressed" states to buttons
Create local json for versions and check against it on updates
Check if default files exist on update and download if they do not
*/