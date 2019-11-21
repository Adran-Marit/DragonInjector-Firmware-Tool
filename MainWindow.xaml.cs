using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using Octokit;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.IO.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DragonInjector_Firmware_Tool


{
    public partial class MainWindow : Window
    {
        string uf2File;
        string uf2ShortFile;
        readonly string defaultFirmware = Directory.GetCurrentDirectory() + "\\bin\\defaultfirmware.uf2";
        readonly string defaultBootloader = Directory.GetCurrentDirectory() + "\\bin\\defaultbootloader.uf2";
        readonly string programVersion = "1.15";

        public MainWindow()
        {
            InitializeComponent();
            TitleLabel.Text = "DragonInjector Firmware Tool - v" + programVersion;
            GetDrives();
            GetReleasesAsync();
        }

        async Task diDelay()
        {
            await Task.Delay(7000);
        }

        private void disableButtons()
        {
            FlashAllButton.IsEnabled = false;
            FlashButton.IsEnabled = false;
            BootloaderAllButton.IsEnabled = false;
            BootloaderButton.IsEnabled = false;
            PayloadButton.IsEnabled = false;
            DriveButton.IsEnabled = false;
            DriveBox.IsHitTestVisible = false;
            PayloadTextBox.IsEnabled = false;
            SettingsButton.IsEnabled = false;
        }

        private void enableButtons()
        {
            FlashAllButton.IsEnabled = true;
            FlashButton.IsEnabled = true;
            BootloaderAllButton.IsEnabled = true;
            BootloaderButton.IsEnabled = true;
            PayloadButton.IsEnabled = true;
            DriveButton.IsEnabled = true;
            DriveBox.IsHitTestVisible = true;
            PayloadTextBox.IsEnabled = true;
            SettingsButton.IsEnabled = true;
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

        private async void FlashButton_Click(object sender, RoutedEventArgs e)
        {
            if (uf2File != null && DriveBox.Text != "DragonBoot (default)" && DriveBox.SelectedItem != null)
            {
                disableButtons();
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Copying " + uf2ShortFile + " to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                try
                {
                    File.Copy(uf2File, dest, true);
                    OutputBox.Content += "\n...Waiting for DragonInjector";
                    OutputBox.ScrollToBottom();
                    await diDelay();
                    MessageBox.Show("Flash complete!");
                }
                catch
                {
                    MessageBox.Show("DragonInjector unplugged!");
                    OutputBox.Content += "\n...DragonInjector unplugged";
                    OutputBox.ScrollToBottom();
                }
                GetDrives();
                enableButtons();
            }
            else if (DriveBox.SelectedItem != null && File.Exists(".\\bin\\defaultfirmware.uf2") && uf2File == null || DriveBox.Text == "Leave blank for DragonBoot")
            {
                disableButtons();
                OutputBox.Content += "\n...Using default firmware";
                OutputBox.ScrollToBottom();
                string dest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                OutputBox.Content += "\n\\:Copying default firmware to " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                try
                {
                    File.Copy(defaultFirmware, dest, true);
                    OutputBox.Content += "\n...Waiting for DragonInjector";
                    OutputBox.ScrollToBottom();
                    await diDelay();
                    MessageBox.Show("Flash complete!");
                }
                catch
                {
                    MessageBox.Show("DragonInjector unplugged!");
                    OutputBox.Content += "\n...DragonInjector unplugged";
                    OutputBox.ScrollToBottom();
                }
                GetDrives();
                enableButtons();
            }
            else if (!File.Exists(".\\bin\\defaultfirmware.uf2"))
            {
                OutputBox.Content += "\n!Missing default firmware in directory";
                OutputBox.ScrollToBottom();
            }
            else
            {
                MessageBox.Show("No DragonInjector selected!");
                OutputBox.Content += "\n!No DragonInjector selected";
                OutputBox.ScrollToBottom();
            }
        }

        private async void FlashAllButton_Click(object sender, RoutedEventArgs e)
        {
            disableButtons();
            bool success = false;
            if (uf2File != null)
            {
                try
                {
                    foreach (var item in DriveBox.Items)
                    {
                        try
                        {
                            string dest = item.ToString() + "\\flash.uf2";
                            OutputBox.Content += "\n\\:Copying " + uf2ShortFile + " to " + item.ToString().Replace(":\\", "");
                            OutputBox.ScrollToBottom();
                            File.Copy(uf2File, dest, true);
                            success = true;
                        }
                        catch
                        {
                            MessageBox.Show("DragonInjector unplugged!");
                            OutputBox.Content += "\n...DragonInjector unplugged";
                            OutputBox.ScrollToBottom();
                            success = false;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("DragonInjector unplugged!");
                    OutputBox.Content += "\n...DragonInjector unplugged";
                    OutputBox.ScrollToBottom();
                    success = false;
                }
                OutputBox.Content += "\n...Waiting for DragonInjector";
                OutputBox.ScrollToBottom();
                await diDelay();
                if (success == true)
                {
                    MessageBox.Show("Flash complete!");
                }
            }
            else
            {
                try
                {
                    foreach (var item in DriveBox.Items)
                    {
                        try
                        {
                            string dest = item.ToString() + "\\flash.uf2";
                            OutputBox.Content += "\n\\:Copying default firmware to " + item.ToString().Replace(":\\", "");
                            OutputBox.ScrollToBottom();
                            File.Copy(defaultFirmware, dest, true);
                            success = true;
                        }
                        catch
                        {
                            MessageBox.Show("DragonInjector unplugged!");
                            OutputBox.Content += "\n...DragonInjector unplugged";
                            OutputBox.ScrollToBottom();
                            success = false;
                        }

                    }
                }
                catch
                {
                    MessageBox.Show("DragonInjector unplugged!");
                    OutputBox.Content += "\n...DragonInjector unplugged";
                    OutputBox.ScrollToBottom();
                    success = false;
                }
                OutputBox.Content += "\n...Waiting for DragonInjectors";
                OutputBox.ScrollToBottom();
                await diDelay();
                if (success == true)
                {
                    MessageBox.Show("Flash complete!");
                }
            }
            GetDrives();
            enableButtons();
        }

        private async void BootloaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DriveBox.SelectedItem != null && File.Exists(".\\bin\\defaultbootloader.uf2"))
            {
                disableButtons();
                string bldest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                string uf2local = ".\\bin\\current.uf2";
                string uf2di = DriveBox.SelectedItem.ToString() + "\\current.uf2";
                OutputBox.Content += "\n\\:Updating bootloader on " + DriveBox.SelectedItem.ToString().Replace(":\\", "");
                OutputBox.ScrollToBottom();
                try
                {
                    File.Copy(uf2di, uf2local, true);
                    File.Copy(defaultBootloader, bldest, true);
                    OutputBox.Content += "\n...Waiting for DragonInjector";
                    OutputBox.ScrollToBottom();
                    await diDelay();
                    try
                    {
                        File.Copy(uf2local, uf2di, true);
                        File.Delete(uf2local);
                        await diDelay();
                    }
                    catch
                    {
                        MessageBox.Show("Bootloader written but unable to write firmware. Please flash firmware again!");
                        OutputBox.Content += "\n!Bootloader written but unable to write firmware. Please flash firmware again";
                        OutputBox.ScrollToBottom();
                    }
                    MessageBox.Show("Flash complete!");
                }
                catch
                {
                    MessageBox.Show("DragonInjector unplugged!");
                    OutputBox.Content += "\n...DragonInjector unplugged";
                    OutputBox.ScrollToBottom();
                }
                GetDrives();
                enableButtons();
            }
            else if (!File.Exists(".\\bin\\defaultbootloader.uf2"))
            {
                OutputBox.Content += "\n!Missing default bootloader in directory";
                OutputBox.ScrollToBottom();
            }
            else
            {
                MessageBox.Show("No DragonInjector selected!");
                OutputBox.Content += "\n!No DragonInjector selected";
                OutputBox.ScrollToBottom();
            }
        }

        private async void BootloaderAllButton_Click(object sender, RoutedEventArgs e)
        {
            disableButtons();
            bool success = false;
            try
            {
                foreach (var item in DriveBox.Items)
                {
                    try
                    {
                        string bldest = DriveBox.SelectedItem.ToString() + "\\flash.uf2";
                        string uf2local = ".\\bin\\current.uf2";
                        string uf2di = DriveBox.SelectedItem.ToString() + "\\current.uf2";
                        OutputBox.Content += "\n\\:Updating bootloader on " + (item.ToString()).Replace(":\\", "");
                        OutputBox.ScrollToBottom();
                        File.Copy(uf2di, uf2local, true);
                        File.Copy(defaultBootloader, bldest, true);
                        OutputBox.Content += "\n...Waiting for DragonInjector";
                        OutputBox.ScrollToBottom();
                        await diDelay();
                        success = true;
                        try
                        {
                            File.Copy(uf2local, uf2di, true);
                            File.Delete(uf2local);
                            await diDelay();
                        }
                        catch
                        {
                            MessageBox.Show("Bootloader written but unable to write firmware. Please flash firmware again!");
                            OutputBox.Content += "\n!Bootloader written but unable to write firmware. Please flash firmware again";
                            OutputBox.ScrollToBottom();
                        }
                    }
                    catch
                    {
                        OutputBox.Content += "\n...DragonInjector unplugged";
                        OutputBox.ScrollToBottom();
                        success = false;
                    }
                }
            }
            catch
            {
                MessageBox.Show("DragonInjector unplugged!");
                OutputBox.Content += "\n...DragonInjector unplugged";
                OutputBox.ScrollToBottom();
                success = false;
            }
            if (success == true)
            {
                MessageBox.Show("Flash complete!");
            }
            GetDrives();
            enableButtons();
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
                        MessageBox.Show("File too large, using default!");
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

        private void downloadUF2(string urlDownload, string uf2Location)
        {
            var downloader = new WebClient();
            downloader.DownloadFile(urlDownload, uf2Location);
            downloader.Dispose();
        }

        private bool isToolUpdate = false;

        private async System.Threading.Tasks.Task GetReleasesAsync()
        {
            OutputBox.Content += "\n...Checking for updates";
            OutputBox.ScrollToBottom();
            var regexGIT = new Regex(@"\d*\.\d*");
            Directory.CreateDirectory(".\\bin");

            var githubProgram = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesProgram = await githubProgram.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-UpdateTool");
            var releaseProgram = releasesProgram[0];
            string gitProgramVersion = regexGIT.Match(releaseProgram.TagName.ToString()).ToString();
            if (gitProgramVersion != programVersion)
            {
                OutputBox.Content += "\n!Tool is outdated";
                OutputBox.ScrollToBottom();
                CheckUpdateButton.Visibility = Visibility.Visible;
                isToolUpdate = true;
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
            bool downloadFW = false;
            OutputBox.Content += "\nFound firmware release on github: v" + fwVersion;
            OutputBox.ScrollToBottom();
            LatestFirmwareVersionLabel.Text = "v" + fwVersion;
            if (File.Exists(".\\bin\\defaultfirmware.uf2"))
            {
                StreamReader localFW = new System.IO.StreamReader(".\\bin\\defaultfirmware.uf2");
                string lineFW;
                while ((lineFW = localFW.ReadLine()) != null)
                {
                    if (lineFW.Contains("DI_FW_"))
                    {
                        var regex = new Regex(@"DI_FW_\d*\.\d*");
                        string version = (regex.Match(lineFW).ToString()).Replace("DI_FW_", "");
                        if (version == fwVersion)
                        {
                            OutputBox.Content += "\n...Local firmware same as github version. Skipping";
                            OutputBox.ScrollToBottom();
                        }
                        else
                        {
                            OutputBox.Content += "\n...Newer firmware found in github. Downloading";
                            OutputBox.ScrollToBottom();
                            downloadFW = true;
                        }
                    }
                }
                localFW.ReadToEnd();
                localFW.Close();
            }
            else
            {
                OutputBox.Content += "\n...No local firmware found. Downloading";
                OutputBox.ScrollToBottom();
                downloadFW = true;
            }
            if (downloadFW == true)
            {
                downloadUF2(urlFW, ".\\bin\\defaultfirmware.uf2");
            }

            var githubBL = new GitHubClient(new ProductHeaderValue("Nothing"));
            var releasesBL = await githubBL.Repository.Release.GetAll("dragoninjector-project", "DragonInjector-Bootloader");
            var releaseBL = releasesBL[0];
            string blVersion = regexGIT.Match(releaseBL.TagName.ToString()).ToString();
            string urlBL = releaseBL.Assets[0].BrowserDownloadUrl.ToString();
            bool downloadBL = false;
            OutputBox.Content += "\nFound bootloader release on github: v" + blVersion;
            OutputBox.ScrollToBottom();
            LatestBootloaderVersionLabel.Text = "v" + blVersion;
            if (File.Exists(".\\bin\\defaultbootloader.uf2"))
            {
                StreamReader localBL = new System.IO.StreamReader(".\\bin\\defaultbootloader.uf2");
                string lineBL;
                while ((lineBL = localBL.ReadLine()) != null)
                {
                    if (lineBL.Contains("DI_BL_"))
                    {
                        var regex = new Regex(@"DI_BL_\d*\.\d*");
                        string version = (regex.Match(lineBL).ToString()).Replace("DI_BL_", "");
                        if (version == blVersion)
                        {
                            OutputBox.Content += "\n...Local bootloader same as github version. Skipping";
                            OutputBox.ScrollToBottom();
                        }
                        else
                        {
                            OutputBox.Content += "\n...Newer bootloader found in github. Downloading";
                            OutputBox.ScrollToBottom();
                            downloadBL = true;
                        }
                    }
                }
                localBL.ReadToEnd();
                localBL.Close();
            }
            else
            {
                OutputBox.Content += "\n...No local bootloader found. Downloading";
                OutputBox.ScrollToBottom();
                downloadBL = true;
            }
            if (downloadBL == true)
            {
                downloadUF2(urlBL, ".\\bin\\defaultbootloader.uf2");
            }
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

        private void BLDelayHelpButton_Click(object sender, RoutedEventArgs e)
        {
            BLDelayPopup.IsOpen = true;
        }

        private void BLDelayHelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BLDelayPopup.IsOpen = false;
        }

        private void PayloadSlotsHelpButton_Click(object sender, RoutedEventArgs e)
        {
            PayloadSlotsPopup.IsOpen = true;
        }

        private void PayloadSlotsHelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PayloadSlotsPopup.IsOpen = false;
        }

        private void ModeSwitchDelayHelpButton_Click(object sender, RoutedEventArgs e)
        {
            ModeSwitchDelayPopup.IsOpen = true;
        }

        private void ModeSwitchDelayHelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ModeSwitchDelayPopup.IsOpen = false;
        }

        private void SelectedPayloadHelpButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPayloadPopup.IsOpen = true;
        }

        private void SelectedPayloadHelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SelectedPayloadPopup.IsOpen = false;
        }

        private void PreRCMDelayHelpButton_Click(object sender, RoutedEventArgs e)
        {
            PreRCMDelayPopup.IsOpen = true;
        }

        private void PreRCMDelayHelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PreRCMDelayPopup.IsOpen = false;
        }

        private void DualPayloadHelpButton_Click(object sender, RoutedEventArgs e)
        {
            DualPayloadPopup.IsOpen = true;
        }

        private void DualPayloadHelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DualPayloadPopup.IsOpen = false;
        }

        private bool elementToggle;

        private void ToggleElements()
        {
            if (elementToggle == false)
            {
                GetCOM();
                HideMain();
            }
            else
            {
                ShowMain();
            }
        }

        private void HideMain()
        {
            PayloadGroupPicture.Visibility = Visibility.Hidden;
            FirmwareGroupPicture.Visibility = Visibility.Hidden;
            PayloadLabel.Visibility = Visibility.Hidden;
            DriveLabel.Visibility = Visibility.Hidden;
            DriveBox.Visibility = Visibility.Hidden;
            PayloadTextBox.Visibility = Visibility.Hidden;
            PayloadButton.Visibility = Visibility.Hidden;
            DriveButton.Visibility = Visibility.Hidden;
            FirmwareLabel.Visibility = Visibility.Hidden;
            LatestFirmwareLabel.Visibility = Visibility.Hidden;
            FlashButton.Visibility = Visibility.Hidden;
            FlashAllButton.Visibility = Visibility.Hidden;
            BootloaderGroupPicture.Visibility = Visibility.Hidden;
            BootloaderLabel.Visibility = Visibility.Hidden;
            BootloaderVersionLabel.Visibility = Visibility.Hidden;
            LatestBootloaderVersionLabel.Visibility = Visibility.Hidden;
            FirmwareVersionLabel.Visibility = Visibility.Hidden;
            LatestFirmwareVersionLabel.Visibility = Visibility.Hidden;
            LatestBootloaderLabel.Visibility = Visibility.Hidden;
            BootloaderButton.Visibility = Visibility.Hidden;
            BootloaderAllButton.Visibility = Visibility.Hidden;
            CheckUpdateButton.Visibility = Visibility.Hidden;
            COMSelectLabel.Visibility = Visibility.Visible;
            COMSelectBox.Visibility = Visibility.Visible;
            SettingsButton.Visibility = Visibility.Hidden;
            SettingsBackButton.Visibility = Visibility.Visible;
            SettingsGroupPicture.Visibility = Visibility.Visible;
            COMRescanButton.Visibility = Visibility.Visible;
            BLDelayLabel.Visibility = Visibility.Visible;
            PayloadSlotsLabel.Visibility = Visibility.Visible;
            ModeSwitchDelayLabel.Visibility = Visibility.Visible;
            SelectedPayloadLabel.Visibility = Visibility.Visible;
            PreRCMDelayLabel.Visibility = Visibility.Visible;
            DualPayloadLabel.Visibility = Visibility.Visible;
            BLDelayBox.Visibility = Visibility.Visible;
            PayloadSlotsBox.Visibility = Visibility.Visible;
            ModeSwitchDelayBox.Visibility = Visibility.Visible;
            SelectedPayloadBox.Visibility = Visibility.Visible;
            PreRCMDelayBox.Visibility = Visibility.Visible;
            DualPayloadBox.Visibility = Visibility.Visible;
            BLDelayHelpButton.Visibility = Visibility.Visible;
            PayloadSlotsHelpButton.Visibility = Visibility.Visible;
            ModeSwitchDelayHelpButton.Visibility = Visibility.Visible;
            SelectedPayloadHelpButton.Visibility = Visibility.Visible;
            PreRCMDelayHelpButton.Visibility = Visibility.Visible;
            DualPayloadHelpButton.Visibility = Visibility.Visible;
            LoadSettingsButton.Visibility = Visibility.Visible;
            SaveSettingsButton.Visibility = Visibility.Visible;
            DefaultSettingsButton.Visibility = Visibility.Visible;
        }

        private void ShowMain()
        {
            PayloadGroupPicture.Visibility = Visibility.Visible;
            FirmwareGroupPicture.Visibility = Visibility.Visible;
            PayloadLabel.Visibility = Visibility.Visible;
            DriveLabel.Visibility = Visibility.Visible;
            DriveBox.Visibility = Visibility.Visible;
            PayloadTextBox.Visibility = Visibility.Visible;
            PayloadButton.Visibility = Visibility.Visible;
            DriveButton.Visibility = Visibility.Visible;
            FirmwareLabel.Visibility = Visibility.Visible;
            LatestFirmwareLabel.Visibility = Visibility.Visible;
            FlashButton.Visibility = Visibility.Visible;
            FlashAllButton.Visibility = Visibility.Visible;
            BootloaderGroupPicture.Visibility = Visibility.Visible;
            BootloaderLabel.Visibility = Visibility.Visible;
            BootloaderVersionLabel.Visibility = Visibility.Visible;
            LatestBootloaderVersionLabel.Visibility = Visibility.Visible;
            FirmwareVersionLabel.Visibility = Visibility.Visible;
            LatestFirmwareVersionLabel.Visibility = Visibility.Visible;
            LatestBootloaderLabel.Visibility = Visibility.Visible;
            BootloaderButton.Visibility = Visibility.Visible;
            BootloaderAllButton.Visibility = Visibility.Visible;
            if (isToolUpdate == true)
            {
                CheckUpdateButton.Visibility = Visibility.Visible;
            }
            COMSelectLabel.Visibility = Visibility.Hidden;
            COMSelectBox.Visibility = Visibility.Hidden;
            SettingsButton.Visibility = Visibility.Visible;
            SettingsBackButton.Visibility = Visibility.Hidden;
            SettingsGroupPicture.Visibility = Visibility.Hidden;
            COMRescanButton.Visibility = Visibility.Hidden;
            BLDelayLabel.Visibility = Visibility.Hidden;
            PayloadSlotsLabel.Visibility = Visibility.Hidden;
            ModeSwitchDelayLabel.Visibility = Visibility.Hidden;
            SelectedPayloadLabel.Visibility = Visibility.Hidden;
            PreRCMDelayLabel.Visibility = Visibility.Hidden;
            DualPayloadLabel.Visibility = Visibility.Hidden;
            BLDelayBox.Visibility = Visibility.Hidden;
            PayloadSlotsBox.Visibility = Visibility.Hidden;
            ModeSwitchDelayBox.Visibility = Visibility.Hidden;
            SelectedPayloadBox.Visibility = Visibility.Hidden;
            PreRCMDelayBox.Visibility = Visibility.Hidden;
            DualPayloadBox.Visibility = Visibility.Hidden;
            BLDelayHelpButton.Visibility = Visibility.Hidden;
            PayloadSlotsHelpButton.Visibility = Visibility.Hidden;
            ModeSwitchDelayHelpButton.Visibility = Visibility.Hidden;
            SelectedPayloadHelpButton.Visibility = Visibility.Hidden;
            PreRCMDelayHelpButton.Visibility = Visibility.Hidden;
            DualPayloadHelpButton.Visibility = Visibility.Hidden;
            LoadSettingsButton.Visibility = Visibility.Hidden;
            SaveSettingsButton.Visibility = Visibility.Hidden;
            DefaultSettingsButton.Visibility = Visibility.Hidden;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleElements();
            elementToggle = !elementToggle;
        }

        private void SettingsBackButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleElements();
            elementToggle = !elementToggle;
        }

        private void GetCOM()
        {
            COMSelectBox.ItemsSource = SerialPort.GetPortNames();
        }

        public int changeSelect= -1;

        private void BLDelayBox_DropDownOpened(object sender, EventArgs e)
        {
            changeSelect = BLDelayBox.SelectedIndex;
        }

        private void BLDelayBox_DropDownClosed(object sender, EventArgs e)
        {
            if (COMSelectBox.SelectedValue == null)
            {
                BLDelayBox.SelectedIndex = -1;
                MessageBox.Show("No COM port selected.");
            }
            else if (BLDelayBox.SelectedIndex != changeSelect)
            {
                string selectedOption = BLDelayBox.SelectedValue.ToString();
                ConnectSerial(selectedOption);
                OutputBox.Content += "\n!Wrote setting";
                OutputBox.ScrollToBottom();
                changeSelect = -1;
            }
        }

        private void PayloadSlotsBox_DropDownOpened(object sender, EventArgs e)
        {
            changeSelect = PayloadSlotsBox.SelectedIndex;
        }

        private void PayloadSlotsBox_DropDownClosed(object sender, EventArgs e)
        {
            if (COMSelectBox.SelectedValue == null)
            {
                PayloadSlotsBox.SelectedIndex = -1;
                MessageBox.Show("No COM port selected.");
            }
            else if (PayloadSlotsBox.SelectedIndex != changeSelect)
            {
                string selectedOption = PayloadSlotsBox.SelectedValue.ToString();
                ConnectSerial(selectedOption);
                OutputBox.Content += "\n!Wrote setting";
                OutputBox.ScrollToBottom();
                changeSelect = -1;
            }
        }

        private void ModeSwitchDelayBox_DropDownOpened(object sender, EventArgs e)
        {
            changeSelect = ModeSwitchDelayBox.SelectedIndex;
        }

        private void ModeSwitchDelayBox_DropDownClosed(object sender, EventArgs e)
        {
            if (COMSelectBox.SelectedValue == null)
            {
                ModeSwitchDelayBox.SelectedIndex = -1;
                MessageBox.Show("No COM port selected.");
            }
            else if (ModeSwitchDelayBox.SelectedIndex != changeSelect)
            {
                string selectedOption = ModeSwitchDelayBox.SelectedValue.ToString();
                ConnectSerial(selectedOption);
                OutputBox.Content += "\n!Wrote setting";
                OutputBox.ScrollToBottom();
                changeSelect = -1;
            }
        }

        private void SelectedPayloadBox_DropDownOpened(object sender, EventArgs e)
        {
            changeSelect = SelectedPayloadBox.SelectedIndex;
        }

        private void SelectedPayloadBox_DropDownClosed(object sender, EventArgs e)
        {
            if (COMSelectBox.SelectedValue == null)
            {
                SelectedPayloadBox.SelectedIndex = -1;
                MessageBox.Show("No COM port selected.");
            }
            else if (SelectedPayloadBox.SelectedIndex != changeSelect)
            {
                string selectedOption = SelectedPayloadBox.SelectedValue.ToString();
                ConnectSerial(selectedOption);
                OutputBox.Content += "\n!Wrote setting";
                OutputBox.ScrollToBottom();
                changeSelect = -1;
            }
        }

        private void PreRCMDelayBox_DropDownOpened(object sender, EventArgs e)
        {
            changeSelect = PreRCMDelayBox.SelectedIndex;
        }

        private void PreRCMDelayBox_DropDownClosed(object sender, EventArgs e)
        {
            if (COMSelectBox.SelectedValue == null)
            {
                PreRCMDelayBox.SelectedIndex = -1;
                MessageBox.Show("No COM port selected.");
            }
            else if (PreRCMDelayBox.SelectedIndex != changeSelect)
            {
                string selectedOption = PreRCMDelayBox.SelectedValue.ToString();
                ConnectSerial(selectedOption);
                OutputBox.Content += "\n!Wrote setting";
                OutputBox.ScrollToBottom();
                changeSelect = -1;
            }
        }

        private void DualPayloadBox_DropDownOpened(object sender, EventArgs e)
        {
            changeSelect = DualPayloadBox.SelectedIndex;
        }

        private void DualPayloadBox_DropDownClosed(object sender, EventArgs e)
        {
            if (COMSelectBox.SelectedValue == null)
            {
                DualPayloadBox.SelectedIndex = -1;
                MessageBox.Show("No COM port selected.");
            }
            else if (DualPayloadBox.SelectedIndex != changeSelect)
            {
                string selectedOption = DualPayloadBox.SelectedValue.ToString();
                ConnectSerial(selectedOption);
                OutputBox.Content += "\n!Wrote setting";
                OutputBox.ScrollToBottom();
                changeSelect = -1;
            }
        }

        private void COMSelectBox_SelectionChanged(object sender, EventArgs e)
        {
            ReadSerial();
        }

        private void ConnectSerial(string selectedOption)
        {
            SerialPort serialComms = new SerialPort();
            serialComms.BaudRate = 115200;
            if (COMSelectBox.SelectedValue != null)
            {
                try
                {
                    serialComms.PortName = COMSelectBox.SelectedValue.ToString();
                    serialComms.Open();
                    serialComms.Write(selectedOption);
                    serialComms.Close();
                }
                catch
                {
                    MessageBox.Show("Cannot connect to DragonInjector!");
                }
            }
        }

        private void ReadSerial()
        {
            SerialPort serialComms = new SerialPort();
            serialComms.BaudRate = 115200;
            serialComms.PortName = COMSelectBox.SelectedValue.ToString();
            try
            {
                string results;
                string trim;
                var regex = new Regex(@"\d");
                serialComms.PortName = COMSelectBox.SelectedValue.ToString();
                serialComms.Open();
                serialComms.Write("bdelay");
                results = serialComms.ReadLine();
                serialComms.Close();
                trim = (regex.Match(results)).ToString();
                BLDelayBox.SelectedIndex = System.Convert.ToInt32(trim) - 1;
                serialComms.Open();
                serialComms.Write("slots");
                results = serialComms.ReadLine();
                serialComms.Close();
                trim = (regex.Match(results)).ToString();
                PayloadSlotsBox.SelectedIndex = System.Convert.ToInt32(trim) - 1;
                serialComms.Open();
                serialComms.Write("mdelay");
                results = serialComms.ReadLine();
                serialComms.Close();
                trim = (regex.Match(results)).ToString();
                ModeSwitchDelayBox.SelectedIndex = System.Convert.ToInt32(trim) - 1;
                serialComms.Open();
                serialComms.Write("cslot");
                results = serialComms.ReadLine();
                serialComms.Close();
                trim = (regex.Match(results)).ToString();
                SelectedPayloadBox.SelectedIndex = System.Convert.ToInt32(trim);
                serialComms.Open();
                serialComms.Write("rdelay");
                results = serialComms.ReadLine();
                serialComms.Close();
                trim = (regex.Match(results)).ToString();
                PreRCMDelayBox.SelectedIndex = System.Convert.ToInt32(trim) - 1;
                serialComms.Open();
                serialComms.Write("dmode");
                results = serialComms.ReadLine();
                serialComms.Close();
                trim = (regex.Match(results)).ToString();
                DualPayloadBox.SelectedIndex = System.Convert.ToInt32(trim);
            }
            catch
            {
                MessageBox.Show("Cannot connect to DragonInjector!");
            }
        }

        private void COMRescanButton_Click(object sender, RoutedEventArgs e)
        {
            GetCOM();
        }

        private void LoadSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to load and apply settings?", "Default Settings", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (COMSelectBox.SelectedValue != null)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    {
                        openFileDialog.Filter = "JSON (*.json)|*.json";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            string filePath = openFileDialog.FileName;
                            string settingsShortFile = (Path.GetFileName(filePath)).ToString();
                            string settingsFile = Path.GetFullPath(filePath).ToString();
                            string json = File.ReadAllText(settingsFile);
                            jsonSettingsObject jsonSetting = JsonConvert.DeserializeObject<jsonSettingsObject>(json);
                            BLDelayBox.SelectedIndex = jsonSetting.bdelay - 1;
                            ConnectSerial("bdelay " + jsonSetting.bdelay.ToString());
                            PayloadSlotsBox.SelectedIndex = jsonSetting.slots - 1;
                            ConnectSerial("slots " + jsonSetting.slots.ToString());
                            ModeSwitchDelayBox.SelectedIndex = jsonSetting.mdelay - 1;
                            ConnectSerial("mdelay " + jsonSetting.mdelay.ToString());
                            SelectedPayloadBox.SelectedIndex = jsonSetting.cslot;
                            ConnectSerial("cslot " + jsonSetting.cslot.ToString());
                            PreRCMDelayBox.SelectedIndex = jsonSetting.rdelay - 1;
                            ConnectSerial("rdelay " + jsonSetting.rdelay.ToString());
                            DualPayloadBox.SelectedIndex = jsonSetting.dmode;
                            ConnectSerial("dmode " + jsonSetting.dmode.ToString());
                            OutputBox.Content += "\n!Loaded settings";
                            OutputBox.ScrollToBottom();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No COM port selected.");
                }
            }
            else if (result == MessageBoxResult.No)
            {
                OutputBox.Content += "\n!Cancelled";
                OutputBox.ScrollToBottom();
            }
        }

        public class jsonSettingsObject
        {
            public int bdelay;
            public int slots;
            public int mdelay;
            public int cslot;
            public int rdelay;
            public int dmode;
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            JObject saveSettingsObject = new JObject(
                new JProperty("bdelay", BLDelayBox.SelectedIndex + 1),
                new JProperty("slots", PayloadSlotsBox.SelectedIndex + 1),
                new JProperty("mdelay", ModeSwitchDelayBox.SelectedIndex + 1),
                new JProperty("cslot", SelectedPayloadBox.SelectedIndex),
                new JProperty("rdelay", PreRCMDelayBox.SelectedIndex + 1),
                new JProperty("dmode", DualPayloadBox.SelectedIndex));
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string settingsShortFile = (Path.GetFileName(filePath)).ToString();
                string settingsFile = Path.GetFullPath(filePath).ToString();
                File.WriteAllText(settingsFile, saveSettingsObject.ToString());
                OutputBox.Content += "\n!Saved settings";
                OutputBox.ScrollToBottom();
            }
        }

        private void DefaultSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to revert to factory defaults?", "Default Settings", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (COMSelectBox.SelectedValue != null)
                {
                    BLDelayBox.SelectedIndex = 4;
                    ConnectSerial("bdelay 5");
                    PayloadSlotsBox.SelectedIndex = 3;
                    ConnectSerial("slots 4");
                    ModeSwitchDelayBox.SelectedIndex = 2;
                    ConnectSerial("mdelay 3");
                    SelectedPayloadBox.SelectedIndex = 0;
                    ConnectSerial("cslot 0");
                    PreRCMDelayBox.SelectedIndex = 0;
                    ConnectSerial("rdelay 1");
                    DualPayloadBox.SelectedIndex = 0;
                    ConnectSerial("dmode 0");
                    OutputBox.Content += "\n!Default settings applied";
                    OutputBox.ScrollToBottom();
                }
                else
                {
                    MessageBox.Show("No COM port selected.");
                }
            }
            else if (result == MessageBoxResult.No)
            {
                OutputBox.Content += "\n!Cancelled";
                OutputBox.ScrollToBottom();
            }
        }
    }
}

/*
TODO:
use variables to reference uf2 locations - too many hard paths coded, updating is a chore
make window resizable with elements?
fix visibility settings to be cleaner (less lines, use a stack panel and hide that?)
fix button lock settings to be cleaner (toggle boolean so it can be a single method with less lines)
Associate com port with actual DI - likely requires registry parsing to find it. Seems kind of messy.
Add not available image on lower firmwares for settings screen combo boxes
embed dlls
*/
