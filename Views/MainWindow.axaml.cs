using Avalonia.Controls;
using TelegaApp.Models;
using Avalonia.Threading;
using System;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Styling;
using System.Linq;
using System.Collections.Generic;

namespace TelegaApp.Views
{
    public partial class MainWindow : Window
    {
        private RecognitionModel rec { get; set; }

        private List<string> checkBoxesChecked = new List<string>();

        private int isButtonChanged = 0;

        DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            rec = new RecognitionModel();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(record);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();
        }


        public void record(object? sender, EventArgs e)
        {
            if (isButtonChanged == 0)
            {
                imageBox.Source = (Avalonia.Media.IImage)rec.GetImage(checkBoxesChecked);
            }
            else
            {
                imageBox.Source = (Avalonia.Media.IImage)rec.GetMask(checkBoxesChecked);
            }
            InvalidateVisual();   
        }

        private void SetOrigin(object sender, RoutedEventArgs e)
        {
            isButtonChanged = 0;
        }

        private void SetMask(object sender, RoutedEventArgs e)
        {
            isButtonChanged = 1;
        }

        private void RedCheck(object sender, RoutedEventArgs e)
        {
            GetCheckBoxCollection();
        }
        private void GreenCheck(object sender, RoutedEventArgs e)
        {
            GetCheckBoxCollection();
        }
        private void BlueCheck(object sender, RoutedEventArgs e)
        {
            GetCheckBoxCollection();
        }
        private void PurpleCheck(object sender, RoutedEventArgs e)
        {
            GetCheckBoxCollection();
        }
        private void YellowCheck(object sender, RoutedEventArgs e)
        {
            GetCheckBoxCollection();
        }
        private void GetCheckBoxCollection()
        {
            var list = grid.Children.OfType<CheckBox>();
            foreach (var chbox in list)
            {
                if (chbox.Name != null)
                {
                    if (chbox.IsChecked == true)
                    {
                        checkBoxesChecked.Add(chbox.Name);
                    }
                    else
                    {
                        checkBoxesChecked.Remove(chbox.Name);
                    }
                }
            }
        }
    }
}