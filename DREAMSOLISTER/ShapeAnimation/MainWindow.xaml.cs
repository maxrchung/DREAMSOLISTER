﻿using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ShapeAnimation {
    public partial class MainWindow : Window {
        #region Variables
        private ViewModel viewModel;

        private DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ObservableCollection<SAShape>));

        private TimeSpan timerInterval = TimeSpan.FromMilliseconds(15);
        private DispatcherTimer moveTimer = new DispatcherTimer(DispatcherPriority.Render);
        private Vector moveOffset;
        private DispatcherTimer rotateTimer = new DispatcherTimer(DispatcherPriority.Render);
        private Angle previousRotation;
        private DispatcherTimer scaleTimer = new DispatcherTimer(DispatcherPriority.Render);
        private Vector scaleOffset;
        #endregion

        #region Initialization
        public MainWindow() {
            moveTimer.Interval = timerInterval;
            moveTimer.Tick += new EventHandler(moveTimerTick);

            rotateTimer.Interval = timerInterval;
            rotateTimer.Tick += new EventHandler(rotateTimerTick);

            scaleTimer.Interval = timerInterval;
            scaleTimer.Tick += new EventHandler(scaleTimerTick);

            InitializeComponent();

            // These require InitializeComponent() to be called first
            viewModel = (ViewModel)DataContext;
        }
        #endregion

        #region Helpers
        private Vector getMousePosition() {
            var mousePosition = Mouse.GetPosition(shapesCanvas);
            return new Vector((float)mousePosition.X, (float)mousePosition.Y);
        }

        private void insertShape(SAShape shape) {
            if (viewModel.selected != null) {
                var insertIndex = viewModel.shapes.IndexOf(viewModel.selected);
                viewModel.shapes.Insert(insertIndex + 1, shape);
            }
            else {
                viewModel.shapes.Add(shape);
            }
        } 
        #endregion

        #region Commands
        private void deselect(object sender, ExecutedRoutedEventArgs args) {
            viewModel.selected = null;
        }
        private void delete(object sender, ExecutedRoutedEventArgs args) {
            if (viewModel.selected != null) {
                viewModel.shapes.Remove(viewModel.selected);
                viewModel.selected = null;
            }
        }
        private void createRectangle(object sender, ExecutedRoutedEventArgs args) {
            var shape = new SARectangle();
            shape.position = getMousePosition();
            insertShape(shape);
            viewModel.selected = shape;
        }
        private void createTriangle(object sender, ExecutedRoutedEventArgs args) {
            var shape = new SATriangle();
            shape.position = getMousePosition();
            insertShape(shape);
            viewModel.selected = shape;
        }
        private void createEllipse(object sender, ExecutedRoutedEventArgs args) {
            var shape = new SAEllipse();
            shape.position = getMousePosition();
            insertShape(shape);
            viewModel.selected = shape;
        }
        private void createSemicircle(object sender, ExecutedRoutedEventArgs args) {
            var shape = new SASemicircle();
            shape.position = getMousePosition();
            insertShape(shape);
            viewModel.selected = shape;
        }
        private void copy(object sender, ExecutedRoutedEventArgs args) {
            if (viewModel.selected != null) {
                var shape = viewModel.selected.copy();
                shape.position = getMousePosition();
                insertShape(shape);
                viewModel.selected = shape;
            }
        }
        private void moveUp(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                int oldIndex = viewModel.shapes.IndexOf(viewModel.selected);
                if (oldIndex != 0) {
                    viewModel.shapes.Move(oldIndex, oldIndex - 1);
                }
            }
        }
        private void moveDown(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                int oldIndex = viewModel.shapes.IndexOf(viewModel.selected);
                if (oldIndex != viewModel.shapes.Count - 1) {
                    viewModel.shapes.Move(oldIndex, oldIndex + 1);
                }
            }
        }
        private void moveToTop(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                int oldIndex = viewModel.shapes.IndexOf(viewModel.selected);
                viewModel.shapes.Move(oldIndex, 0);
            }
        }
        private void moveToBottom(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                int oldIndex = viewModel.shapes.IndexOf(viewModel.selected);
                viewModel.shapes.Move(oldIndex, viewModel.shapes.Count - 1);
            }
        }
        private void toggleVisibilityBG(object sender, RoutedEventArgs e) {
            viewModel.visibilityBG = !viewModel.visibilityBG;
        }
        private void toggleVisibilityShapes(object sender, RoutedEventArgs e) {
            viewModel.visibilityShapes = !viewModel.visibilityShapes;
        }
        private void eyeDrop(object sender, RoutedEventArgs e) {
            BitmapSource screenimage;
            byte[] pixels;
            System.Drawing.Point _point = System.Windows.Forms.Control.MousePosition;
            Point point = new Point(_point.X, _point.Y);
            screenimage = InteropHelper.CaptureRegion(InteropHelper.GetDesktopWindow(),
                                                                       (int)SystemParameters.VirtualScreenLeft,
                                                                       (int)SystemParameters.VirtualScreenTop,
                                                                       (int)SystemParameters.PrimaryScreenWidth,
                                                                       (int)SystemParameters.PrimaryScreenHeight);
            if (screenimage != null && viewModel.selected != null) {
                int stride = (screenimage.PixelWidth * screenimage.Format.BitsPerPixel + 7) / 8;
                pixels = new byte[screenimage.PixelHeight * stride];
                Int32Rect rect = new Int32Rect((int)point.X, (int)point.Y, 1, 1);
                screenimage.CopyPixels(rect, pixels, stride, 0);
                SolidColorBrush meme = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
                viewModel.selected.color = (Color)ColorConverter.ConvertFromString(InteropHelper.ConvertToString(meme));
            }

        }
        #endregion

        #region Timer Tick Events
        private void moveTimerTick(object sender, EventArgs e) {
            // Timers may be off from event
            if (viewModel.selected != null) {
                viewModel.selected.position = getMousePosition() - moveOffset;
            }
        }
        private void rotateTimerTick(object sender, EventArgs e) {
            if (viewModel.selected != null) {
                var rotation = (getMousePosition() - viewModel.selected.position).angle;
                var rotateOffset = rotation - previousRotation;
                previousRotation = rotation;
                viewModel.selected.rotation += rotateOffset;
            }
        }
        private void scaleTimerTick(object sender, EventArgs e) {
            if (viewModel.selected != null) {
                var position = getMousePosition() - viewModel.selected.position - scaleOffset;
                var unrotate = position.rotate(-viewModel.selected.rotation.radian);
                var scaleVector = new Vector(Math.Abs(unrotate.x), Math.Abs(unrotate.y)) / SAShape.fixedSize * 2;
                viewModel.selected.scaleVector = scaleVector;
            }
        }
        #endregion

        #region Mouse Events
        private void shapeMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (viewModel.selected == null) {
                var shape = (Shape)sender;
                var contentPresenter = (ContentPresenter)shape.TemplatedParent;
                var canvas = (Canvas)VisualTreeHelper.GetParent(contentPresenter);
                var index = canvas.Children.IndexOf(contentPresenter);
                viewModel.selected = viewModel.shapes[index];
            }
        }
        private void rotateMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (viewModel.selected != null && !rotateTimer.IsEnabled) {
                previousRotation = (getMousePosition() - viewModel.selected.position).angle;
                rotateTimer.Start();
                e.Handled = true;
            }
        }
        private void scaleMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (viewModel.selected != null && !scaleTimer.IsEnabled) {
                var corner = (Ellipse)sender;
                var translateTransform = (TranslateTransform)corner.RenderTransform;
                var topLeft = new Vector((float)translateTransform.X, (float)translateTransform.Y);
                var center = topLeft + new Vector((float)corner.Width) / 2;
                scaleOffset = getMousePosition() - center;
                scaleTimer.Start();
                e.Handled = true;
            }
        }
        private void backgroundMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (viewModel.selected != null && !moveTimer.IsEnabled) {
                moveOffset = getMousePosition() - viewModel.selected.position;
                moveTimer.Start();
            }
        }
        private void backgroundMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            moveTimer.Stop();
            rotateTimer.Stop();
            scaleTimer.Stop();
        }
        private void rotateMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            rotateTimer.Stop();
        }
        private void scaleMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            scaleTimer.Stop();
        }
        #endregion

        #region Slider Events
        private void redSliderValueChanged(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                var color = viewModel.selected.color;
                viewModel.selected.color = Color.FromRgb(Convert.ToByte(redSlider.Value), color.G, color.B);
            }
        }
        private void greenSliderValueChanged(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                var color = viewModel.selected.color;
                viewModel.selected.color = Color.FromRgb(color.R, Convert.ToByte(greenSlider.Value), color.B);
            }
        }
        private void blueSliderValueChanged(object sender, RoutedEventArgs e) {
            if (viewModel.selected != null) {
                var color = viewModel.selected.color;
                var b = Convert.ToByte(blueSlider.Value);
                viewModel.selected.color = Color.FromRgb(color.R, color.G, Convert.ToByte(blueSlider.Value));
            }
        }
        #endregion

        #region Dialogs
        private void loadClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.png, *.jpg, *.jpeg, *.gif, *.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All Files (*.*)|*.*";
            if (dialog.ShowDialog() == true) {
                try {
                    var bitmap = new BitmapImage(new Uri(dialog.FileName));
                    image.Source = bitmap;
                }
                catch (Exception exception) {
                    Debug.WriteLine(exception.ToString());
                }
            }
        }
        private void openClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "ShapeAnimation file (*.shan)|*.shan";
            if (dialog.ShowDialog() == true) {
                try {
                    var file = new FileStream(dialog.FileName, FileMode.Open);
                    viewModel.shapes = (ObservableCollection<SAShape>)serializer.ReadObject(file);
                    viewModel.selected = null;
                }
                catch (Exception exception) {
                    Debug.WriteLine(exception.ToString());
                }
            }
        }
        private void saveClick(object sender, RoutedEventArgs e) {
            var dialog = new SaveFileDialog();
            dialog.Filter = "ShapeAnimation file (*.shan)|*.shan";
            if (dialog.ShowDialog() == true) {
                try {
                    var file = new FileStream(dialog.FileName, FileMode.Create);
                    serializer.WriteObject(file, viewModel.shapes);
                }
                catch (Exception exception) {
                    Debug.WriteLine(exception.ToString());
                }
            }
        }
        #endregion

    }
}
