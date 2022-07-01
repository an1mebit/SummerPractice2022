using Avalonia.Controls;
using TelegaApp.Models;
using Avalonia.Threading;
using System;
using Avalonia.Interactivity;

namespace TelegaApp.Views
{
    public partial class MainWindow : Window
    {
        private RecognitionModel rec { get; set; }

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
            if (rec.GetImage() != null && isButtonChanged == 0)
            {
                imageBox.Source = (Avalonia.Media.IImage)rec.GetImage();
            }
            else
            {
                imageBox.Source = (Avalonia.Media.IImage)rec.GetMask();
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
    }
}