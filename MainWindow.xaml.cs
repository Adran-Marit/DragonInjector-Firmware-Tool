using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using Octokit;
using System.Text.RegularExpressions;
using System.Net;

namespace DragonInjector_Firmware_Tool
{
    public partial class MainWindow : Window
    {
        string uf2File;
        string uf2ShortFile;
        readonly string defaultFirmware = Directory.GetCurrentDirectory() + "\\payloads\\defaultfirmware.uf2";
        readonly string defaultBootloader = Directory.GetCurrentDirectory() + "\\payloads\\defaultbootloader.uf2";
        readonly string programVersion = "1.11";
        
        
        public MainWindow()
        {
            InitializeComponent();
            TitleLabel.Text = "DragonInjector Firmware Tool - v" + programVersion;
            GetDrives();
            GetReleasesAsync();
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
            Process.Start("https://github.com/dragoninjector-project/DragonInjector-UpdateTool/releases/latest");
        }

        private void FlashButton_Click(object sender, RoutedEventArgs e)
        {
            if (uf2File != null && DriveBox.Text != "DragonBoot (default)" && DriveBox.SelectedItem != null)
            {
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Copying " + uf2ShortFile + " to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(uf2File, dest, true);
            }
            else if (DriveBox.SelectedItem != null && File.Exists(".\\payloads\\defaultfirmware.uf2") && uf2File == null || DriveBox.Text == "Leave blank for DragonBoot")
            {
                OutputBox.Content += "\n...Using default firmware";
                OutputBox.ScrollToBottom();
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Copying default firmware to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                File.Copy(defaultFirmware, dest, true);
            }
            else if (!File.Exists(".\\payloads\\defaultfirmware.uf2"))
            {
                OutputBox.Content += "\n!Missing default firmware in directory";
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
                    OutputBox.Content += "\n\\:Copying default firmware to " + item.ToString().Replace(":\\", "");
                    OutputBox.ScrollToBottom();
                    File.Copy(defaultFirmware, dest, true);
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
                File.Copy(defaultBootloader, dest, true);
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
                File.Copy(defaultBootloader, dest, true);
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
                    OutputBox.Content += "\nLoaded custom payload: " + (Path.GetFileName(filePath)).ToString();
                    OutputBox.ScrollToBottom();
                    long fileLength = (new System.IO.FileInfo(filePath).Length) / 2;
                    long maxLength = 57088;
                    OutputBox.Content += "\nCustom payload size: " + fileLength + " bytes";
                    OutputBox.ScrollToBottom();

                    if (fileLength > maxLength)
                    {
                        
                        OutputBox.Content += "\n...Payload too large, must be under " + maxLength + " bytes. Clearing custom payload";
                        OutputBox.ScrollToBottom();
                        PayloadTextBox.Text = "Too large, using default!";
                        uf2File = null;
                        uf2ShortFile = null;
                    }
                    else
                    {
                        OutputBox.Content += "\n.Custom payload size verified OK";
                        OutputBox.ScrollToBottom();
                    }
                }
            }
        }

        private void DriveBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GetDIVersions();
        }

        private void PayloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (PayloadTextBox.Text != "DragonBoot (default)")
            {
                PayloadTextBox.Text = "DragonBoot (default)";
                OutputBox.Content += "\n...Using default firmware";
                OutputBox.ScrollToBottom();
            }
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.dragoninjector.com");
        }

        private async System.Threading.Tasks.Task GetReleasesAsync()
        {
            OutputBox.Content += "\n...Checking for updates";
            OutputBox.ScrollToBottom();
            var regexGIT = new Regex(@"\d*\.\d*");
            Directory.CreateDirectory(".\\payloads");
            var downloader = new WebClient();

            var githubProgram = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesProgram = await githubProgram.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-UpdateTool");
            var releaseProgram = releasesProgram[0];
            string gitProgramVersion = regexGIT.Match(releaseProgram.TagName.ToString()).ToString();
            if (gitProgramVersion != programVersion)
            {
                OutputBox.Content += "\n!Tool is outdated";
                OutputBox.ScrollToBottom();
                CheckUpdateButton.Visibility = Visibility.Visible;
            }
            else
            {
                OutputBox.Content += "\n.Tool is the latest version";
                OutputBox.ScrollToBottom();
            }

            var githubFW = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesFW = await githubFW.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-Firmware");
            var releaseFW = releasesFW[0];
            string fwVersion = regexGIT.Match(releaseFW.TagName.ToString()).ToString();
            string urlFW = releaseFW.Assets[0].BrowserDownloadUrl.ToString();
            OutputBox.Content += "\nFound firmware release on github: v" + fwVersion;
            OutputBox.ScrollToBottom();
            LatestFirmwareVersionLabel.Text = "v" + fwVersion;
            if (File.Exists(".\\payloads\\defaultfirmware.uf2"))
            {
                StreamReader localFW = new System.IO.StreamReader(".\\payloads\\defaultfirmware.uf2");
                string lineFW;
                while ((lineFW = localFW.ReadLine()) != null)
                {
                    if (lineFW.Contains("DI_FW_"))
                    {
                        var regex = new Regex(@"DI_FW_\d*\.\d*");
                        string version = (regex.Match(lineFW).ToString()).Replace("DI_FW_", "");
                        if (version == fwVersion)
                        {
                            localFW.ReadToEnd();
                            OutputBox.Content += "\n...Local firmware same as github version. Skipping";
                            OutputBox.ScrollToBottom();
                        }
                        else
                        {
                            localFW.ReadToEnd();
                            OutputBox.Content += "\n...Newer firmware found in github. Downloading";
                            OutputBox.ScrollToBottom();
                            downloader.DownloadFile(urlFW, ".\\payloads\\defaultfirmware.uf2");
                        }
                    }
                }
                localFW.Dispose();
            }
            else
            {
                OutputBox.Content += "\n...No local firmware found. Downloading";
                OutputBox.ScrollToBottom();
                downloader.DownloadFile(urlFW, ".\\payloads\\defaultfirmware.uf2");
            }

            var githubBL = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesBL = await githubBL.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-Bootloader");
            var releaseBL = releasesBL[0];
            string blVersion = regexGIT.Match(releaseBL.TagName.ToString()).ToString();
            string urlBL = releaseBL.Assets[0].BrowserDownloadUrl.ToString();
            OutputBox.Content += "\nFound bootloader release on github: v" + blVersion;
            OutputBox.ScrollToBottom();
            LatestBootloaderVersionLabel.Text = "v" + blVersion;
            if (File.Exists(".\\payloads\\defaultbootloader.uf2"))
            {
                StreamReader localBL = new System.IO.StreamReader(".\\payloads\\defaultbootloader.uf2");
                string lineBL;
                while ((lineBL = localBL.ReadLine()) != null)
                {
                    if (lineBL.Contains("DI_BL_"))
                    {
                        var regex = new Regex(@"DI_BL_\d*\.\d*");
                        string version = (regex.Match(lineBL).ToString()).Replace("DI_BL_", "");
                        if (version == blVersion)
                        {
                            localBL.ReadToEnd();
                            OutputBox.Content += "\n...Local bootloader same as github version. Skipping";
                            OutputBox.ScrollToBottom();
                        }
                        else
                        {
                            localBL.ReadToEnd();
                            OutputBox.Content += "\n...Newer bootloader found in github. Downloading";
                            OutputBox.ScrollToBottom();
                            downloader.DownloadFile(urlBL, ".\\payloads\\defaultbootloader.uf2");
                        }
                    }
                }
                localBL.Dispose();
            }
            else
            {
                OutputBox.Content += "\n...No local bootloader found. Downloading";
                OutputBox.ScrollToBottom();
                downloader.DownloadFile(urlBL, ".\\payloads\\defaultbootloader.uf2");
            }
            downloader.Dispose();
        }
        
        private void GetDrives()
        {
            OutputBox.Content += "\n...Scanning for drives";
            OutputBox.ScrollToBottom();
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
                FirmwareVersionLabel.Text = "NONE";
                BootloaderVersionLabel.Text = "NONE";
            }
            else
            {
                DriveBox.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task GetDIVersions()
        {
            string selectedItem = DriveBox.SelectedItem.ToString();
            if (File.Exists(selectedItem + "CURRENT.UF2"))
            {
                StreamReader currentUF2 = new System.IO.StreamReader(selectedItem + "CURRENT.UF2");

                string lineFW;
                int x = 0;
                while ((lineFW = currentUF2.ReadLine()) != null)
                {
                    if (lineFW.Contains("DI_FW_"))
                    {
                        var regex = new Regex(@"DI_FW_\d*\.\d*");
                        string version = (regex.Match(lineFW).ToString()).Replace("DI_FW_", "");
                        FirmwareVersionLabel.Text = "v" + version;
                        OutputBox.Content += "\nFound firmware version on DragonInjector (" + selectedItem.Replace("\\", "") + "): v" + version;
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
                string lineBL;
                int y = 0;
                while ((lineBL = currentUF2.ReadLine()) != null)
                {
                    if (lineBL.Contains("DI_BL_"))
                    {
                        var regex = new Regex(@"DI_BL_\d*\.\d*");
                        string version = (regex.Match(lineBL).ToString()).Replace("DI_BL_", "");
                        BootloaderVersionLabel.Text = "v" + version;
                        OutputBox.Content += "\nFound bootloader version on DragonInjector (" + selectedItem.Replace("\\", "") + "): v" + version;
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
            else
            {
                FirmwareVersionLabel.Text = "UNKNOWN";
                OutputBox.Content += "\n!Couldn't find firmware version on DragonInjector";
                OutputBox.ScrollToBottom();
                BootloaderVersionLabel.Text = "UNKNOWN";
                OutputBox.Content += "\n!Couldn't find bootloader version on DragonInjector";
                OutputBox.ScrollToBottom();
            }
        }
    }
}

/*
TODO:
wont pickup new current bootloader version on flash or firmware - call getdiversions after flash?
crash without error if selected drive no longer plugged in (like after flash) - pause after flash then get drives?
make downloading more verbose
make bootloader update firmware too
*/