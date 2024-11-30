using Autodesk.Windows;
using MyApp.Common;
using MyApp.Presentation.Ribbons;
using System;
using System.Windows.Media.Imaging;
using RibbonButton = Autodesk.Windows.RibbonButton;
using RibbonControl = Autodesk.Windows.RibbonControl;
using RibbonTab = Autodesk.Windows.RibbonTab;

public static class RibbonManager
{
    private const string RibbonTabId = "MY_CUSTOM_TAB";
    public static void AddRibbons()
    {

        RibbonControl ribbonControl = Autodesk.Windows.ComponentManager.Ribbon;
        if (ribbonControl == null) return;

        //RibbonTab existingTab = ribbonControl.FindTab(RibbonTabId);
        //if (existingTab != null)
        //{
           
        //    return;
        //}

        RibbonTab ribbonTab = new RibbonTab
        {
            Title = "My Tab",
            Id = RibbonTabId

        };

        ribbonControl.Tabs.Add(ribbonTab);

        RibbonPanelSource ribbonPanelSource = new RibbonPanelSource
        {
            Title = "My Commands"
        };
        RibbonPanel ribbonPanel = new RibbonPanel
        {
            Source = ribbonPanelSource
        };


        ribbonTab.Panels.Add(ribbonPanel);

        RibbonButton myButton = new RibbonButton
        {
            Text = "Show MainWindow",
            Orientation = System.Windows.Controls.Orientation.Vertical,

            ShowText = true,
            ShowImage = true,
            Size = RibbonItemSize.Large,
            LargeImage = new BitmapImage(new Uri("pack://application:,,,/MyApp;component/Resources/a_large.png", UriKind.RelativeOrAbsolute)),// MyApp.Properties.Images.a_large.ToBitmapImage(),
            Image = new BitmapImage(new Uri("pack://application:,,,/MyApp;component/Resources/a_small.png", UriKind.RelativeOrAbsolute)),//MyApp.Properties.Images.a_small.ToBitmapImage(),
            CommandHandler = new RibbonMainCommand(),

        };

        // Add the button to the ribbon panel
        ribbonPanelSource.Items.Add(myButton);
    }
}