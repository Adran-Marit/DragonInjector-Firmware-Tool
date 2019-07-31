#########################################################################################################################
#                                                   F U N C T I O N S                                                   #
#########################################################################################################################

#Find the working directory
Function Get-Directory {
    $Global:path = Get-Location
}

#Get the bin file
Function Open-Picker ($initialdirectory) {
    $FileBrowser = New-Object System.Windows.Forms.OpenFileDialog
    $FileBrowser = New-Object System.Windows.Forms.OpenFileDialog 
    $FileBrowser.InitialDirectory = $initialdirectory
    $FileBrowser.Filter = "UF2 (*.uf2)|*.uf2"
    $FileBrowser.ShowDialog() | Out-Null
    $Global:binfile = $FileBrowser.FileName
    return $FileBrowser.FileName
}

Function Get-Filename {
    $binfile = Open-Picker -InitialDirectory $path
    if ($binfile.length -ne 0) {
        $Global:binaryselected = $true
        $Window.Findname("OutputBox").Content += "`n" + "Binary selected: " + $binfile
    }
    else {
        $result = [System.Windows.Forms.MessageBox]::Show('No file selected. Try again?', 'Error', 'YesNo', 'Warning')
            if ($result -eq 'Yes') {
                Get-Filename
            }
            else {
                $Window.Findname("OutputBox").Content += "`n" + ".No binary selected" + $binfile
                $Window.Findname("OutputBox").ScrollToEnd()
                $Global:binaryselected = $false
            }
    }
}

#Get a list of Dragonboot drives
Function Get-Drives {
    $drives = (wmic logicaldisk where "volumename='DRAGONBOOT'" get deviceid /value).replace("DeviceID=","")
    $drives = $drives | where {$_}
    $drives = $drives -replace ":",":\"
    Get-DriveList
}

Function Get-DriveList {
    if ($drives -ne 0) {
        $Window.Findname("DriveBox").Items.Clear()
        foreach ($drive in $drives) {
            $Window.Findname("DriveBox").Items.Add($drive)
            $Window.Findname("OutputBox").Content += "`n" + "\:Found drive: " + ($drive).replace(":\","")
            $Window.Findname("OutputBox").ScrollToEnd()
            $Window.Findname("DriveBox").SelectedIndex = 0
        }
    }
    else {
        $Window.Findname("OutputBox").Content += "`n" + ".No drives found"
        $Window.Findname("OutputBox").ScrollToEnd()
        $Window.Findname("DriveBox").SelectedIndex = -1
    }
}

Function New-Drives {
    $drives = (wmic logicaldisk where "volumename='DRAGONBOOT'" get deviceid /value).replace("DeviceID=","")
    $drives = $drives | where {$_}
    $drives = $drives -replace ":",":\"
    New-DriveList
}

#Populate the drives list
Function New-DriveList {
    if ($drives -ne 0) {
        $tempselectedindex = $Window.Findname("DriveBox").SelectedIndex
        $Window.Findname("DriveBox").Items.Clear()
        foreach ($drive in $drives) {
            $Window.Findname("DriveBox").Items.Add($drive)
            $Window.Findname("OutputBox").Content += "`n" + "\:Found drive: " + ($drive).replace(":\","")
            $Window.Findname("OutputBox").ScrollToEnd()
        }
    }
    else {
        $Window.Findname("OutputBox").Content += "`n" + ".No drives found"
        $Window.Findname("OutputBox").ScrollToEnd()
        $Window.Findname("DriveBox").SelectedIndex = -1
    }
    $Window.Findname("DriveBox").SelectedIndex = $tempselectedindex
}

#Copy to drives
Function Copy-Drive {
    $Window.Findname("OutputBox").Content += "`n" + "\:Copying to: " + ($selecteddrive).replace(":\","")
    $Window.Findname("OutputBox").ScrollToEnd()
    Copy-Item $binfile $selecteddrive -Force
}

Function Copy-DriveDefault {
    $Window.Findname("OutputBox").Content += "`n" + "\:Copying default payload to: " + ($selecteddrive).replace(":\","")
    $Window.Findname("OutputBox").ScrollToEnd()
    Copy-Item "$path\files\uf2\flash.uf2" $selecteddrive -Force
}

Function Copy-AllDrives {
    if ($alldrives.count -ne 0) {
        foreach ($singledrive in $alldrives) {
            $Window.Findname("OutputBox").Content += "`n" + "\:Copying to: " + ($singledrive).replace(":\","")
            $Window.Findname("OutputBox").ScrollToEnd()
            Copy-Item $binfile $singledrive -Force
        }
    }
    else {
    }
}

Function Copy-AllDrivesDefault {
    if ($alldrives.count -ne 0) {
        foreach ($singledrive in $alldrives) {
            $Window.Findname("OutputBox").Content += "`n" + "\:Copying default payload to: " + ($singledrive).replace(":\","")
            $Window.Findname("OutputBox").ScrollToEnd()
            Copy-Item "$path\files\uf2\flash.uf2" $singledrive -Force
        }
    }
    else {
    }
}

Function Get-Bootloader {
    $blversion = Get-Content ($Window.Findname("DriveBox").SelectedItem + "INFO_UF2.TXT") -First 1 -ErrorAction SilentlyContinue
    if ($Window.Findname("DriveBox").SelectedIndex -ne -1) {
        if ($blversion -like "DragonInjector UF2 Bootloader*") {
            $blversion = ($blversion -replace "DragonInjector UF2 Bootloader ","").ToLower()
            $blversion
        }
        else {
            $blversion = "Custom"
            $blversion
        }
    }
    else {
    }
}

Function Start-Flash {
    if ($selecteddrive -ne $null -and $binaryselected -eq $true){
        Copy-Drive
        Get-Drives
    }
    elseif ($selecteddrive -eq $null) {
        $result = [System.Windows.Forms.MessageBox]::Show('No drive selected!', 'Error', 'Ok')
    }
    else {
        Copy-DriveDefault
        Get-Drives
    }
}

Function Start-FlashAll {
    if ($alldrives.count -ne 0 -and $binaryselected -eq $true) {
        Copy-AllDrives
        Get-Drives
    }
    elseif ($alldrives.count -eq 0) {
        $result = [System.Windows.Forms.MessageBox]::Show('No drives found!', 'Error', 'Ok')
    }
    else {
        Copy-AllDrivesDefault
        Get-Drives
    }
}

#Add GUI window
Function New-GUI {
    Add-Type -AssemblyName PresentationFramework
    Add-Type -assembly System.Windows.Forms

[xml]$Xaml = @"
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    Name="Window" Title="DragonInjector Firmware Tool" Height="519" Width="919" Opacity="0.96" WindowStartupLocation="CenterScreen" HorizontalAlignment="Left" Margin="0" VerticalAlignment="Top" ResizeMode="NoResize" AllowsTransparency="True" BorderThickness="2" BorderBrush="Black">
    <Window.Resources>
        <ImageBrush x:Key="BoxImg" ImageSource="$path\files\resources\input_field_normal_277_56.png"></ImageBrush>
        <Style x:Key="NoHover" TargetType="{x:Type Button}">
            <Setter Property="FocusVisualStyle">
                <Setter.Value>
                    <Style>
                        <Setter Property="Control.Template">
                            <Setter.Value>
                                <ControlTemplate>
                                    <Rectangle Margin="2" SnapsToDevicePixels="True" Stroke="{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}" StrokeThickness="1" StrokeDashArray="1 2"/>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>
                </Setter.Value>
            </Setter>
            <Setter Property="Background" Value="#00FFFFFF"/>
            <Setter Property="BorderBrush" Value="#00FFFFFF"/>
            <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="HorizontalContentAlignment" Value="Center"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Padding" Value="1"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type Button}">
                        <Border x:Name="border" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" Background="{TemplateBinding Background}" SnapsToDevicePixels="True">
                            <ContentPresenter x:Name="contentPresenter" ContentTemplate="{TemplateBinding ContentTemplate}" Content="{TemplateBinding Content}" ContentStringFormat="{TemplateBinding ContentStringFormat}" Focusable="False" HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" Margin="{TemplateBinding Padding}" RecognizesAccessKey="True" SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}" VerticalAlignment="{TemplateBinding VerticalContentAlignment}"/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsDefaulted" Value="True">
                                <Setter Property="BorderBrush" TargetName="border" Value="{DynamicResource {x:Static SystemColors.HighlightBrushKey}}"/>
                            </Trigger>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Background" TargetName="border" Value="#00FFFFFF"/>
                                <Setter Property="BorderBrush" TargetName="border" Value="#00FFFFFF"/>
                            </Trigger>
                            <Trigger Property="IsPressed" Value="True">
                                <Setter Property="Background" TargetName="border" Value="#00FFFFFF"/>
                                <Setter Property="BorderBrush" TargetName="border" Value="#00FFFFFF"/>
                            </Trigger>
                            <Trigger Property="ToggleButton.IsChecked" Value="True">
                                <Setter Property="Background" TargetName="border" Value="#00FFFFFF"/>
                                <Setter Property="BorderBrush" TargetName="border" Value="#00FFFFFF"/>
                            </Trigger>
                            <Trigger Property="IsEnabled" Value="False">
                                <Setter Property="Background" TargetName="border" Value="#00FFFFFF"/>
                                <Setter Property="BorderBrush" TargetName="border" Value="#00FFFFFF"/>
                                <Setter Property="Foreground" Value="#00FFFFFF"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                        </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        <Style x:Key="EuroTitle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="22"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="FontFamily" Value="$path\files\resources\EurostileNextLTPro-Regular.ttf #Eurostile Next LT Pro"/>
        </Style>
        <Style x:Key="Euro" TargetType="TextBlock">
            <Setter Property="FontSize" Value="20"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="HorizontalAlignment" Value="Right"/>
            <Setter Property="TextWrapping" Value="NoWrap"/>
            <Setter Property="TextTrimming" Value="None"/>
            <Setter Property="FontFamily" Value="$path\files\resources\EurostileNextLTPro-Regular.ttf #Eurostile Next LT Pro"/>
        </Style>
        <Style x:Key="EuroConsole" TargetType="ScrollViewer">
            <Setter Property="FontSize" Value="18"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="FlowDirection" Value="RightToLeft"/>
            <Setter Property="FontFamily" Value="$path\files\resources\EurostileNextLTPro-Regular.ttf #Eurostile Next LT Pro"/>
            <Setter Property="OverridesDefaultStyle" Value="True"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ScrollViewer}">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>
                            <ScrollContentPresenter Grid.Column="1"/>
                            <ScrollBar Name="PART_VerticalScrollBar"
                                Value="{TemplateBinding VerticalOffset}"
                                Maximum="{TemplateBinding ScrollableHeight}"
                                ViewportSize="{TemplateBinding ViewportHeight}"
                                Visibility="{TemplateBinding ComputedVerticalScrollBarVisibility}"/>
                            <ScrollBar Name="PART_HorizontalScrollBar"
                                Orientation="Horizontal"
                                Grid.Row="1"
                                Grid.Column="1"
                                Value="{TemplateBinding HorizontalOffset}"
                                Maximum="{TemplateBinding ScrollableWidth}"
                                ViewportSize="{TemplateBinding ViewportWidth}"
                                Visibility="{TemplateBinding ComputedHorizontalScrollBarVisibility}"/>
                        </Grid>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        <Style x:Key="EuroBox" TargetType="TextBox">
            <Setter Property="FontSize" Value="20"/>
            <Setter Property="Foreground" Value="Gray"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Background" Value="{StaticResource BoxImg}"/>
            <Setter Property="FontFamily" Value="$path\files\resources\EurostileNextLTPro-Regular.ttf #Eurostile Next LT Pro"/>
        </Style>

        <SolidColorBrush x:Key="ComboBoxNormalBorderBrush" Color="Transparent" />
        <SolidColorBrush x:Key="ComboBoxNormalBackgroundBrush" Color="Transparent" />
        <SolidColorBrush x:Key="ComboBoxDisabledForegroundBrush" Color="Transparent" />
        <SolidColorBrush x:Key="ComboBoxDisabledBackgroundBrush" Color="Transparent" />
        <SolidColorBrush x:Key="ComboBoxDisabledBorderBrush" Color="Transparent" />

        <ControlTemplate TargetType="ToggleButton" x:Key="ComboBoxToggleButtonTemplate">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition Width="16" />
                </Grid.ColumnDefinitions>
                <Border Grid.ColumnSpan="2" Name="Border" BorderBrush="Transparent" CornerRadius="0" BorderThickness="1, 1, 1, 1" Background="Transparent" />
                <Border Grid.Column="1" Margin="1, 1, 1, 1" BorderBrush="Transparent" Name="ButtonBorder" CornerRadius="0, 0, 0, 0" BorderThickness="0, 0, 0, 0" Background="Transparent" />
                <Path x:Name="Arrow" Grid.Column="1" HorizontalAlignment="Center" Margin="1,-1,0,0" VerticalAlignment="Center" Data="M 0 0 L 4 4 L 8 0 Z" Fill="Transparent"/>
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property="UIElement.IsMouseOver" Value="True">
                    <Setter Property="Panel.Background" TargetName="ButtonBorder" Value="Transparent"/>
                    <Setter Property="Shape.Fill" TargetName="Arrow" Value="Transparent" />
                </Trigger>
                <Trigger Property="UIElement.IsMouseOver" Value="False">
                    <Setter Property="Panel.Background" TargetName="ButtonBorder" Value="Transparent"/>
                    <Setter Property="Shape.Fill" TargetName="Arrow" Value="Transparent" />
                </Trigger>
                <Trigger Property="ToggleButton.IsChecked" Value="True">
                    <Setter Property="Panel.Background" TargetName="ButtonBorder" Value="Transparent"/>
                    <Setter Property="Shape.Fill" TargetName="Arrow" Value="Transparent"/>
                </Trigger>
                <Trigger Property="UIElement.IsEnabled" Value="False">
                    <Setter Property="Panel.Background" TargetName="Border" Value="Transparent"/>
                    <Setter Property="Panel.Background" TargetName="ButtonBorder" Value="Transparent"/>
                    <Setter Property="Border.BorderBrush" TargetName="ButtonBorder" Value="Transparent"/>
                    <Setter Property="TextElement.Foreground" Value="White"/>
                    <Setter Property="Shape.Fill" TargetName="Arrow" Value="White"/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>

        <Style x:Key="ComboBoxFlatStyle"  TargetType="{x:Type ComboBox}">
            <Setter Property="FontSize" Value="20"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="FontFamily" Value="$path\files\resources\EurostileNextLTPro-Regular.ttf #Eurostile Next LT Pro"/>
            <Setter Property="UIElement.SnapsToDevicePixels" Value="True"/>
            <Setter Property="FrameworkElement.OverridesDefaultStyle" Value="True"/>
            <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Auto"/>
            <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto"/>
            <Setter Property="ScrollViewer.CanContentScroll" Value="True"/>
            <Setter Property="TextElement.Foreground" Value="White"/>
            <Setter Property="FrameworkElement.FocusVisualStyle" Value="{x:Null}"/>
            <Setter Property="Control.Template">
                <Setter.Value>
                    <ControlTemplate TargetType="ComboBox">
                        <Grid>
                            <ToggleButton Name="PART_ToggleButton" Grid.Column="2" ClickMode="Press" Focusable="False" IsChecked="{Binding Path=IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" Template="{StaticResource ComboBoxToggleButtonTemplate}"/>
                            <ContentPresenter Name="ContentSite" Margin="5, 3, 23, 3" IsHitTestVisible="False" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Content="{TemplateBinding ComboBox.SelectionBoxItem}" ContentTemplate="{TemplateBinding ComboBox.SelectionBoxItemTemplate}" ContentTemplateSelector="{TemplateBinding ItemTemplateSelector}"/>
                            <TextBox Name="PART_EditableTextBox" Margin="3, 3, 23, 3" IsReadOnly="{TemplateBinding IsReadOnly}" Visibility="Hidden" Background="Transparent" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Focusable="True" >
                                <TextBox.Template>
                                    <ControlTemplate TargetType="TextBox" >
                                        <Border Name="PART_ContentHost" Focusable="False" />
                                    </ControlTemplate>
                                </TextBox.Template>
                            </TextBox>
                            <Border Name="OutlineBoder" BorderBrush="Transparent" IsHitTestVisible="False" />
                            <Popup Name="PART_Popup" Placement="Bottom" Focusable="False" AllowsTransparency="True" IsOpen="{TemplateBinding ComboBox.IsDropDownOpen}" PopupAnimation="Slide">
                                <Grid Name="DropDown" SnapsToDevicePixels="True" >
                                    <Border Name="DropDownBorder" Background="Black" Margin="0, 1, 0, 0" CornerRadius="0" BorderThickness="1,1,1,1" BorderBrush="Black" Opacity="0.9"/>
                                    <ScrollViewer Margin="0,0,0,0" SnapsToDevicePixels="True" Width="255">
                                        <ItemsPresenter KeyboardNavigation.DirectionalNavigation="Contained" />
                                    </ScrollViewer>
                                </Grid>
                            </Popup>
                        </Grid>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style TargetType="{x:Type ComboBoxItem}">
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ComboBoxItem}">
                        <Border x:Name="myBorder">
                            <ContentPresenter />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Background" TargetName="myBorder" Value="DarkRed" />
                                <Setter Property="Opacity" TargetName="myBorder" Value="0.7" />
                                <Setter Property="Width" TargetName="myBorder" Value="255" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <SolidColorBrush x:Key="StandardBorderBrush" Color="#888" />
        <SolidColorBrush x:Key="StandardBackgroundBrush" Color="Black" />
        <SolidColorBrush x:Key="HoverBorderBrush" Color="#DDD" />
        <SolidColorBrush x:Key="SelectedBackgroundBrush" Color="Gray" />
        <SolidColorBrush x:Key="SelectedForegroundBrush" Color="White" />
        <SolidColorBrush x:Key="DisabledForegroundBrush" Color="#888" />
        <SolidColorBrush x:Key="GlyphBrush" Color="#444" />
        <SolidColorBrush x:Key="NormalBrush" Color="#888" />
        <SolidColorBrush x:Key="NormalBorderBrush" Color="#888" />
        <SolidColorBrush x:Key="HorizontalNormalBrush" Color="#FF686868" />
        <SolidColorBrush x:Key="HorizontalNormalBorderBrush" Color="#888" />
        <LinearGradientBrush x:Key="ListBoxBackgroundBrush" StartPoint="0,0" EndPoint="1,0.001">
            <GradientBrush.GradientStops>
                <GradientStopCollection>
                    <GradientStop Color="White" Offset="0.0" />
                    <GradientStop Color="White" Offset="0.6" />
                    <GradientStop Color="#DDDDDD" Offset="1.2"/>
                </GradientStopCollection>
            </GradientBrush.GradientStops>
        </LinearGradientBrush>
        <LinearGradientBrush x:Key="StandardBrush" StartPoint="0,0" EndPoint="0,1">
            <GradientBrush.GradientStops>
                <GradientStopCollection>
                    <GradientStop Color="#FFF" Offset="0.0"/>
                    <GradientStop Color="#CCC" Offset="1.0"/>
                </GradientStopCollection>
            </GradientBrush.GradientStops>
        </LinearGradientBrush>
        <LinearGradientBrush x:Key="PressedBrush" StartPoint="0,0" EndPoint="0,1">
            <GradientBrush.GradientStops>
                <GradientStopCollection>
                    <GradientStop Color="#BBB" Offset="0.0"/>
                    <GradientStop Color="#EEE" Offset="0.1"/>
                    <GradientStop Color="#EEE" Offset="0.9"/>
                    <GradientStop Color="#FFF" Offset="1.0"/>
                </GradientStopCollection>
            </GradientBrush.GradientStops>
        </LinearGradientBrush>
        <Style x:Key="ScrollBarLineButton" TargetType="{x:Type RepeatButton}">
            <Setter Property="Visibility" Value="Hidden"/>
            <Setter Property="SnapsToDevicePixels" Value="True"/>
            <Setter Property="OverridesDefaultStyle" Value="true"/>
            <Setter Property="Focusable" Value="false"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type RepeatButton}">
                        <Border Name="Border" Margin="1" CornerRadius="2" Background="{StaticResource NormalBrush}" BorderBrush="{StaticResource NormalBorderBrush}" BorderThickness="1">
                            <Path HorizontalAlignment="Center" VerticalAlignment="Center" Fill="{StaticResource GlyphBrush}" Data="{Binding Path=Content, RelativeSource={RelativeSource TemplatedParent}}" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsPressed" Value="true">
                                <Setter TargetName="Border" Property="Background" Value="{StaticResource PressedBrush}" />
                            </Trigger>
                            <Trigger Property="IsEnabled" Value="false">
                                <Setter Property="Foreground" Value="{StaticResource DisabledForegroundBrush}"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        <Style x:Key="ScrollBarPageButton" TargetType="{x:Type RepeatButton}">
            <Setter Property="Visibility" Value="Hidden"/>
            <Setter Property="SnapsToDevicePixels" Value="True"/>
            <Setter Property="OverridesDefaultStyle" Value="true"/>
            <Setter Property="IsTabStop" Value="false"/>
            <Setter Property="Focusable" Value="false"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type RepeatButton}">
                        <Border Background="Black" />
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    
        <Style x:Key="ScrollBarThumb" TargetType="{x:Type Thumb}">
            <Setter Property="SnapsToDevicePixels" Value="True"/>
            <Setter Property="OverridesDefaultStyle" Value="true"/>
            <Setter Property="IsTabStop" Value="false"/>
            <Setter Property="Focusable" Value="false"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type Thumb}">
                        <Border CornerRadius="4" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="0"  Width="8" Margin="8,0,-2,0"/>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <ControlTemplate x:Key="VerticalScrollBar" TargetType="{x:Type ScrollBar}">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition MaxHeight="0"/>
                    <RowDefinition Height="0.00001*"/>
                    <RowDefinition MaxHeight="0"/>
                </Grid.RowDefinitions>
                <Border Grid.RowSpan="3" CornerRadius="2" Background="Transparent" />
        <RepeatButton Grid.Row="0" Style="{StaticResource ScrollBarLineButton}" Height="18" Command="ScrollBar.LineUpCommand" Content="M 0 4 L 8 4 L 4 0 Z" />
        <Track Name="PART_Track" Grid.Row="1" IsDirectionReversed="true">
            <Track.DecreaseRepeatButton>
                <RepeatButton Style="{StaticResource ScrollBarPageButton}" Command="ScrollBar.PageUpCommand" />
            </Track.DecreaseRepeatButton>
            <Track.Thumb>
                <Thumb Style="{StaticResource ScrollBarThumb}" Margin="1,0,1,0" Background="{StaticResource HorizontalNormalBrush}" BorderBrush="{StaticResource HorizontalNormalBorderBrush}" />
            </Track.Thumb>
            <Track.IncreaseRepeatButton>
                <RepeatButton Style="{StaticResource ScrollBarPageButton}" Command="ScrollBar.PageDownCommand" />
                    </Track.IncreaseRepeatButton>
                </Track>
                <RepeatButton Grid.Row="3" Style="{StaticResource ScrollBarLineButton}" Height="18" Command="ScrollBar.LineDownCommand" Content="M 0 0 L 4 4 L 8 0 Z"/>
            </Grid>
        </ControlTemplate>
        <ControlTemplate x:Key="HorizontalScrollBar" TargetType="{x:Type ScrollBar}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition MaxWidth="18"/>
                    <ColumnDefinition Width="0.00001*"/>
                    <ColumnDefinition MaxWidth="18"/>
                </Grid.ColumnDefinitions>
                <Border Grid.ColumnSpan="3" CornerRadius="2" Background="#F0F0F0"/>
                <RepeatButton Grid.Column="0"  Style="{StaticResource ScrollBarLineButton}" Width="18" Command="ScrollBar.LineLeftCommand" Content="M 4 0 L 4 8 L 0 4 Z" />
                <Track Name="PART_Track" Grid.Column="1" IsDirectionReversed="False">
                    <Track.DecreaseRepeatButton>
                        <RepeatButton Style="{StaticResource ScrollBarPageButton}" Command="ScrollBar.PageLeftCommand" />
                    </Track.DecreaseRepeatButton>
                    <Track.Thumb>
                        <Thumb Style="{StaticResource ScrollBarThumb}" Margin="0,1,0,1" Background="{StaticResource NormalBrush}" BorderBrush="{StaticResource NormalBorderBrush}" />
                    </Track.Thumb>
                    <Track.IncreaseRepeatButton>
                        <RepeatButton Style="{StaticResource ScrollBarPageButton}" Command="ScrollBar.PageRightCommand" />
                    </Track.IncreaseRepeatButton>
                </Track>
                <RepeatButton Grid.Column="3" Style="{StaticResource ScrollBarLineButton}" Width="18" Command="ScrollBar.LineRightCommand" Content="M 0 0 L 4 4 L 0 8 Z"/>
            </Grid>
        </ControlTemplate>
        <Style x:Key="{x:Type ScrollBar}" TargetType="{x:Type ScrollBar}">
            <Setter Property="SnapsToDevicePixels" Value="True"/>
            <Setter Property="OverridesDefaultStyle" Value="true"/>
            <Style.Triggers>
                <Trigger Property="Orientation" Value="Horizontal">
                    <Setter Property="Width" Value="Auto"/>
                    <Setter Property="Height" Value="0" />
                    <Setter Property="Template" Value="{StaticResource HorizontalScrollBar}" />
                </Trigger>
                <Trigger Property="Orientation" Value="Vertical">
                    <Setter Property="Width" Value="40"/>
                    <Setter Property="Height" Value="Auto" />
                    <Setter Property="Template" Value="{StaticResource VerticalScrollBar}" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>
    <Grid Name="Grid" Margin="0" Width="919" Height="519" Background="Transparent" ScrollViewer.HorizontalScrollBarVisibility="Hidden">
        <Image Name="GridBackground" HorizontalAlignment="Left" Margin="0" VerticalAlignment="Top" Width="919" Height="519"/>
        <Image Name="PayloadGroupPicture" HorizontalAlignment="Left" Margin="18,49,0,0" VerticalAlignment="Top" Width="890" Height="84"/>
        <Image Name="FirmwareGroupPicture" HorizontalAlignment="Left" Margin="18,148,0,0" VerticalAlignment="Top" Width="890" Height="84"/>
        <TextBlock Name="PayloadLabel" HorizontalAlignment="Left" Margin="107,62,0,0" TextWrapping="Wrap" Text="Custom Payload: " VerticalAlignment="Top" Style="{StaticResource Euro}"/>
        <TextBlock Name="DriveLabel" HorizontalAlignment="Left" Margin="56,101,0,0" TextWrapping="Wrap" Text="Select Dragoninjector: " VerticalAlignment="Top" Style="{StaticResource Euro}"/>
        <Image Name="DriveBoxPicture" HorizontalAlignment="Left" Margin="276,95,0,0" VerticalAlignment="Top" Width="254" Height="31"/>
        <ComboBox Name="DriveBox" HorizontalAlignment="Left" Margin="276,95,0,0" VerticalAlignment="Top" Width="254" Height="31" Style="{StaticResource ComboBoxFlatStyle}"/>
        <TextBox Name="PayloadTextBox" HorizontalAlignment="Left" Margin="277,56,0,0" TextWrapping="Wrap" Text="Leave blank for DragonBoot" VerticalAlignment="Top" Width="254" Height="31" Style="{StaticResource EuroBox}"/>
        <Button Name="PayloadButton" HorizontalAlignment="Left" Margin="538,56,0,0" VerticalAlignment="Top" Width="31" Height="31" Style="{StaticResource NoHover}"/>
        <Button Name="DriveButton" HorizontalAlignment="Left" Margin="538,95,0,0" VerticalAlignment="Top" Width="31" Height="31" Style="{StaticResource NoHover}" BorderThickness="0"/>
        <TextBlock Name="FirmwareLabel" HorizontalAlignment="Left" Margin="88,153,0,0" TextWrapping="Wrap" Text="Current Firmware: " VerticalAlignment="Top" Style="{StaticResource Euro}"/>
        <TextBlock Name="LatestFirmwareLabel" HorizontalAlignment="Left" Margin="102,193,0,0" Text="Latest Firmware: " VerticalAlignment="Top" Style="{StaticResource Euro}"/>
        <Button Name="FlashButton" HorizontalAlignment="Left" Margin="569,169,0,0" VerticalAlignment="Top" Height="44" Width="191" Style="{StaticResource NoHover}"/>
        <Button Name="FlashAllButton" Content="Flash All" HorizontalAlignment="Left" Margin="776,169,0,0" VerticalAlignment="Top" Width="113" Height="44" Style="{StaticResource NoHover}"/>
        <Image Name="BootloaderGroupPicture" HorizontalAlignment="Left" Margin="18,245,0,0" VerticalAlignment="Top" Width="890" Height="184"/>
        <Image Name="OutputGroupPicture" HorizontalAlignment="Left" Margin="18,344,0,0" VerticalAlignment="Top" Width="890" Height="159"/>
        <TextBlock Name="BootloaderLabel" HorizontalAlignment="Left" Margin="75,255,0,0" TextWrapping="Wrap" Text="Current Bootloader:" VerticalAlignment="Top" Foreground="White" Style="{StaticResource Euro}"/>
        <TextBlock Name="BootloaderVersionLabel" HorizontalAlignment="Left" Margin="276,257,0,0" TextWrapping="Wrap" Opacity="0.6" VerticalAlignment="Top" Foreground="White" Style="{StaticResource Euro}"/>
        <TextBlock Name="LatestBootloaderVersionLabel" HorizontalAlignment="Left" Margin="276,299,0,0" TextWrapping="Wrap" Opacity="0.6" VerticalAlignment="Top" Foreground="White" Style="{StaticResource Euro}"/>
        <TextBlock Name="FirmwareVersionLabel" HorizontalAlignment="Left" Margin="276,155,0,0" TextWrapping="Wrap" Opacity="0.6" VerticalAlignment="Top" Foreground="White" Style="{StaticResource Euro}"/>
        <TextBlock Name="LatestFirmwareVersionLabel" HorizontalAlignment="Left" Margin="276,196,0,0" TextWrapping="Wrap" Opacity = "0.6" VerticalAlignment="Top" Foreground="White" Style="{StaticResource Euro}"/>
        <TextBlock Name="LatestBootloaderLabel" HorizontalAlignment="Left" Margin="90,297,0,0" TextWrapping="Wrap" Text="Latest Bootloader:" VerticalAlignment="Top" Foreground="White" Style="{StaticResource Euro}"/>
        <Button Name="BootloaderButton" HorizontalAlignment="Left" Margin="507,266,0,0" VerticalAlignment="Top" Height="44" Width="230" Style="{StaticResource NoHover}"/>
        <Button Name="BootloaderAllButton" HorizontalAlignment="Left" Margin="754,266,0,0" VerticalAlignment="Top" Height="44" Width="135" Style="{StaticResource NoHover}"/>
        <Image Name="TitlePicture" HorizontalAlignment="Left" Margin="0,0,0,0" Height="35" VerticalAlignment="Top" Width="919"/>
        <TextBlock Name="TitleLabel" Margin="50,6,0,0" VerticalAlignment="Top" Style="{StaticResource EuroTitle}"/>
        <Button Name="CloseButton" HorizontalAlignment="Left" Margin="855,-1,0,0" VerticalAlignment="Top" Height="42" Width="64" BorderThickness="0" Style="{StaticResource NoHover}"/>
        <Button Name="CheckUpdateButton" HorizontalAlignment="Left" Margin="657,70,0,0" VerticalAlignment="Top" Height="42" Width="232" BorderThickness="0" Style="{StaticResource NoHover}"/>
        <ScrollViewer Name="OutputBox" HorizontalAlignment="Left" Height="150" Margin="130,350,0,0" Content="...Starting DragonInjector Firmware Tool" VerticalAlignment="Top" Width="800" Style="{StaticResource EuroConsole}"/>
        <Button Name="LogoButton" HorizontalAlignment="Left" Height="85" Margin="13,410,0,0" VerticalAlignment="Top" Width="109" Style="{StaticResource NoHover}" BorderThickness="0"/>
    </Grid>
</Window>
"@

    $Reader = (New-Object System.Xml.XmlNodeReader $Xaml)	
    $Window = [Windows.Markup.XamlReader]::Load($Reader)

    Get-Drives

    $GridBackgroundImg = "$path\files\resources\background_0_0.png"
    $Window.FindName("GridBackground").Source = $GridBackgroundImg

    $DriveBoxPictureImg = "$path\files\resources\input_field_with_arrow_normal_277_56.png"
    $Window.FindName("DriveBoxPicture").Source = $DriveBoxPictureImg

    $Window.Findname("DriveBox").add_SelectionChanged({$bootloader = Get-Bootloader; $Window.Findname("BootloaderVersionLabel").Text  = "$bootloader"})

    $FlashButtonImg = New-Object System.Windows.Controls.Image
    $FlashButtonImg.Source = "$path\files\resources\flash_firmware_normal_569_169.png"
    $FlashButtonImg.Stretch = 'Fill'
    $FlashButtonImgHover = New-Object System.Windows.Controls.Image
    $FlashButtonImgHover.Source = "$path\files\resources\flash_firmware_hover_569_169.png"
    $FlashButtonImgHover.Stretch = 'Fill'
    $Window.Findname("FlashButton").Add_MouseEnter({$Window.Findname("FlashButton").Content = $FlashButtonImgHover})
    $Window.Findname("FlashButton").Add_MouseLeave({$Window.Findname("FlashButton").Content = $FlashButtonImg})
    $Window.Findname("FlashButton").Content = $FlashButtonImg
    $Window.Findname("FlashButton").Add_Click({$Global:selecteddrive = $Window.Findname("DriveBox").SelectedItem; Start-Flash})

    $FlashAllButtonImg = New-Object System.Windows.Controls.Image
    $FlashAllButtonImg.Source = "$path\files\resources\flash_all_normal_776_169.png"
    $FlashAllButtonImg.Stretch = 'Fill'
    $FlashAllButtonImgHover = New-Object System.Windows.Controls.Image
    $FlashAllButtonImgHover.Source = "$path\files\resources\flash_all_hover_776_169.png"
    $FlashAllButtonImgHover.Stretch = 'Fill'
    $Window.Findname("FlashAllButton").Add_MouseEnter({$Window.Findname("FlashAllButton").Content = $FlashAllButtonImgHover})
    $Window.Findname("FlashAllButton").Add_MouseLeave({$Window.Findname("FlashAllButton").Content = $FlashAllButtonImg})
    $Window.Findname("FlashAllButton").Content = $FlashAllButtonImg
    $Window.Findname("FlashAllButton").Add_Click({$Global:alldrives = $Window.Findname("DriveBox").Items; Start-FlashAll})

    $DriveButtonImg = New-Object System.Windows.Controls.Image
    $DriveButtonImg.Source = "$path\files\resources\refresh_button_normal_538_56.png"
    $DriveButtonImg.Stretch = 'Fill'
    $DriveButtonImgHover = New-Object System.Windows.Controls.Image
    $DriveButtonImgHover.Source = "$path\files\resources\refresh_button_hover_538_56.png"
    $DriveButtonImgHover.Stretch = 'Fill'
    $Window.Findname("DriveButton").Add_MouseEnter({$Window.Findname("DriveButton").Content = $DriveButtonImgHover})
    $Window.Findname("DriveButton").Add_MouseLeave({$Window.Findname("DriveButton").Content = $DriveButtonImg})
    $Window.Findname("DriveButton").Content = $DriveButtonImg
    $Window.Findname("DriveButton").Add_Click({New-Drives; Get-Bootloader})

    $PayloadButtonImg = New-Object System.Windows.Controls.Image
    $PayloadButtonImg.Source = "$path\files\resources\clear_button_normal.png"
    $PayloadButtonImg.Stretch = 'Fill'
    $PayloadButtonImgHover = New-Object System.Windows.Controls.Image
    $PayloadButtonImgHover.Source = "$path\files\resources\clear_button_hover.png"
    $PayloadButtonImgHover.Stretch = 'Fill'
    $Window.Findname("PayloadButton").Add_MouseEnter({$Window.Findname("PayloadButton").Content = $PayloadButtonImgHover})
    $Window.Findname("PayloadButton").Add_MouseLeave({$Window.Findname("PayloadButton").Content = $PayloadButtonImg})
    $Window.Findname("PayloadButton").Content = $PayloadButtonImg
    $Window.Findname("PayloadButton").Add_Click({$Window.Findname("PayloadTextBox").Text = ''})

    $Window.Findname("PayloadTextBox").add_PreviewMouseDown({Get-Filename; $binshortname = [io.path]::GetFileNameWithoutExtension($binfile + ".uf2"); $Window.Findname("PayloadTextBox").Text = "$binshortname"})

    $CheckUpdateButtonImg = New-Object System.Windows.Controls.Image
    $CheckUpdateButtonImg.Source = "$path\files\resources\check_for_updates_normal_657_70.png"
    $CheckUpdateButtonImg.Stretch = 'Fill'
    $CheckUpdateButtonImgHover = New-Object System.Windows.Controls.Image
    $CheckUpdateButtonImgHover.Source = "$path\files\resources\check_for_updates_hover_657_70.png"
    $CheckUpdateButtonImgHover.Stretch = 'Fill'
    $Window.Findname("CheckUpdateButton").Add_MouseEnter({$Window.Findname("CheckUpdateButton").Content = $CheckUpdateButtonImgHover})
    $Window.Findname("CheckUpdateButton").Add_MouseLeave({$Window.Findname("CheckUpdateButton").Content = $CheckUpdateButtonImg})
    $Window.Findname("CheckUpdateButton").Content = $CheckUpdateButtonImg
    $Window.Findname("CheckUpdateButton").Add_Click({$result = [System.Windows.Forms.MessageBox]::Show('Not yet implemented!', 'Error', 'Ok')})

    $BootloaderButtonImg = New-Object System.Windows.Controls.Image
    $BootloaderButtonImg.Source = "$path\files\resources\update_bootloader_normal_507_266.png"
    $BootloaderButtonImg.Stretch = 'Fill'
    $BootloaderButtonImgHover = New-Object System.Windows.Controls.Image
    $BootloaderButtonImgHover.Source = "$path\files\resources\update_bootloader_hover_507_266.png"
    $BootloaderButtonImgHover.Stretch = 'Fill'
    $Window.Findname("BootloaderButton").Add_MouseEnter({$Window.Findname("BootloaderButton").Content = $BootloaderButtonImgHover})
    $Window.Findname("BootloaderButton").Add_MouseLeave({$Window.Findname("BootloaderButton").Content = $BootloaderButtonImg})
    $Window.Findname("BootloaderButton").Content = $BootloaderButtonImg
    $Window.Findname("BootloaderButton").Add_Click({$result = [System.Windows.Forms.MessageBox]::Show('Not yet implemented!', 'Error', 'Ok')})

    $BootloaderAllButtonImg = New-Object System.Windows.Controls.Image
    $BootloaderAllButtonImg.Source = "$path\files\resources\update_all_normal_754_266.png"
    $BootloaderAllButtonImg.Stretch = 'Fill'
    $BootloaderAllButtonImgHover = New-Object System.Windows.Controls.Image
    $BootloaderAllButtonImgHover.Source = "$path\files\resources\update_all_hover_754_266.png"
    $BootloaderAllButtonImgHover.Stretch = 'Fill'
    $Window.Findname("BootloaderAllButton").Add_MouseEnter({$Window.Findname("BootloaderAllButton").Content = $BootloaderAllButtonImgHover})
    $Window.Findname("BootloaderAllButton").Add_MouseLeave({$Window.Findname("BootloaderAllButton").Content = $BootloaderAllButtonImg})
    $Window.Findname("BootloaderAllButton").Content = $BootloaderAllButtonImg
    $Window.Findname("BootloaderAllButton").Add_Click({$result = [System.Windows.Forms.MessageBox]::Show('Not yet implemented!', 'Error', 'Ok')})

    $CloseButtonImg = New-Object System.Windows.Controls.Image
    $CloseButtonImg.Source = "$path\files\resources\exit_normal_855_0.png"
    $CloseButtonImg.Stretch = 'Fill'
    $CloseButtonImgHover = New-Object System.Windows.Controls.Image
    $CloseButtonImgHover.Source = "$path\files\resources\exit_hover_855_0.png"
    $CloseButtonImgHover.Stretch = 'Fill'
    $Window.Findname("CloseButton").Content = $CloseButtonImg
    $Window.Findname("CloseButton").Add_MouseEnter({$Window.Findname("CloseButton").Content = $CloseButtonImgHover})
    $Window.Findname("CloseButton").Add_MouseLeave({$Window.Findname("CloseButton").Content = $CloseButtonImg})
    $Window.Findname("CloseButton").Add_Click({$window.Close()})

    $TitlePictureImg = "$path\files\resources\title_bar_0_0.png"
    $Window.Findname("TitlePicture").Source = $TitlePictureImg
    $Window.Findname("TitlePicture").add_MouseLeftButtonDown({$Window.DragMove()})

    $Window.Findname("TitleLabel").Text = "DragonInjector Firmware Tool - Version $version"
    $Window.Findname("TitleLabel").add_MouseLeftButtonDown({$Window.DragMove()})

    $LogoButtonImg = New-Object System.Windows.Controls.Image
    $LogoButtonImg.Source = "$path\files\resources\di_logo_13_417.png"
    $LogoButtonImg.Stretch = 'Fill'
    $Window.Findname("LogoButton").Content = $LogoButtonImg
    $Window.FindName("LogoButton").Add_Click({Start-Process 'https://www.dragoninjector.com'})

    $PayloadGroupPictureImg= "$path\files\resources\payload_group_background_18_49.png"
    $Window.Findname("PayloadGroupPicture").Source = $PayloadGroupPictureImg

    $FirmwareGroupPictureImg = "$path\files\resources\firmware_group_background_18_148.png"
    $Window.Findname("FirmwareGroupPicture").Source = $FirmwareGroupPictureImg

    $BootloaderGroupPictureImg = "$path\files\resources\bootloader_group_background_18_245.png"
    $Window.Findname("BootloaderGroupPicture").Source = $BootloaderGroupPictureImg

    $OutputGroupPictureImg = "$path\files\resources\output_text_background_18_344.png"
    $Window.Findname("OutputGroupPicture").Source = $OutputGroupPictureImg

    $bootloader = Get-Bootloader
    $Window.Findname("BootloaderVersionLabel").Text  = "$bootloader"

    $Window.Findname("LatestBootloaderVersionLabel").Text  = "v1.1" #CREATE FUNCTION TO PULL FROM GIT

    $Window.Findname("FirmwareVersionLabel").Text  = "UNKNOWN" #HOW WE GONNA DO THIS ONE CLEETUS???

    $Window.Findname("LatestFirmwareVersionLabel").Text  = "UNKNOWN" #CREATE FUNCTION TO PULL FROM GIT

    $Window.Icon = "$path\files\resources\DragonInjector.ico"
    $Window.WindowStyle = "None"
    $Window.ShowDialog()
}

Function Start-Main {
    $Global:binaryselected = $false
    Get-Directory
    New-GUI
}

#########################################################################################################################
#                                                     S T A R T                                                         #
#########################################################################################################################
$version = "1.0"
Start-Main

<#
TODO:
customization options = show boot logo, show path only, no visual feedback
Figure out how to pull firmware version
scrollviewer too slow to update, "put text adds onclick before function calls" didn't work
add click states to buttons
make output more verbose
#>