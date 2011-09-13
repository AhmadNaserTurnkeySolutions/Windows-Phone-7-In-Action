﻿using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Devices;
using System.Windows.Navigation;
using Microsoft.Phone;
using ExifLib;

namespace PhotoEditor
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WriteableBitmap currentImage;
        private PhotoCamera camera;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (State.ContainsKey("customCamera"))
            {
                State.Remove("customCamera");
                InitializeCamera();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (camera != null)
            {
                CleanUpCamera();
                State["customCamera"] = true;
            }
        }

        private void Choose_Click(object sender, EventArgs e)
        {
            var task = new PhotoChooserTask();
            task.ShowCamera = true;
            task.Completed += chooserTask_Completed;
            task.Show();
        }

        private void Capture_Click(object sender, EventArgs e)
        {
            var task = new CameraCaptureTask();
            task.Completed += chooserTask_Completed;
            task.Show();
        }

        void chooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                int angle = GetAngleFromExif(e.ChosenPhoto);
                currentImage = DecodeImage(e.ChosenPhoto, angle);
                //currentImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);

                photoContainer.Fill = new ImageBrush { ImageSource = currentImage };
                imageDetails.Text = string.Format("Image from {0}\n", sender.GetType().Name);
                imageDetails.Text += string.Format("Image rotated {0} degrees.\n", angle);
                imageDetails.Text += string.Format("Original filename:\n{0}", e.OriginalFileName);
            }
            else
            {
                photoContainer.Fill = new SolidColorBrush(Colors.Gray);
                imageDetails.Text = e.TaskResult.ToString();
            }
        }

        private void Camera_Click(object sender, EventArgs e)
        {
            if (camera == null)
            {
                currentImage = null;
                imageDetails.Text = string.Format("Choose custom camera again to close camera. Use the hardware shutter button to take a picture.\n");
                InitializeCamera();
            }
            else
            {
                CleanUpCamera();
                photoContainer.Fill = new SolidColorBrush(Colors.Gray);
                imageDetails.Text = "Choose an image source from the menu.";
            }
        }

        void InitializeCamera()
        {
            camera = new PhotoCamera(CameraType.Primary);
            camera.Initialized += camera_Initialized;
            camera.CaptureImageAvailable += camera_CaptureImageAvailable;
            camera.CaptureCompleted += camera_CaptureCompleted;

            CameraButtons.ShutterKeyPressed += cameraButtons_ShutterKeyPressed;

            // create and rotate the brush since our orientation does not match the cameras default orientation.
            var brush = new VideoBrush();
            brush.SetSource(camera);
            brush.RelativeTransform = new RotateTransform { CenterX = 0.5, CenterY = 0.5, Angle = camera.Orientation };
            photoContainer.Fill = brush;
        }

        void camera_Initialized(object sender, CameraOperationCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                imageDetails.Text += string.Format("{0} supported resolutions.\n", camera.AvailableResolutions.Count());
                imageDetails.Text += string.Format("Current resolution: {0}\n", camera.Resolution);
                imageDetails.Text += string.Format("Preview resolution: {0}\n", camera.PreviewResolution);
            });

            camera.Initialized -= camera_Initialized;
        }

        void CleanUpCamera()
        {
            CameraButtons.ShutterKeyPressed -= cameraButtons_ShutterKeyPressed;
            camera.CaptureImageAvailable -= camera_CaptureImageAvailable;
            camera.CaptureCompleted -= camera_CaptureCompleted;
            camera.Dispose();
            camera = null;
        }

        private void cameraButtons_ShutterKeyPressed(object sender, EventArgs e)
        {
            camera.CaptureImage();
        }

        void camera_CaptureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                currentImage = DecodeImage(e.ImageStream, (int)camera.Orientation);
                photoContainer.Fill = new ImageBrush { ImageSource = currentImage };
                imageDetails.Text = "Image captured from PhotoCamera.";
            });
        }

        void camera_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            if (!e.Succeeded)
            {
                photoContainer.Fill = new SolidColorBrush(Colors.Gray);
                imageDetails.Text = "Camera capture failed.\n" + e.Exception.Message;
            }
            CleanUpCamera();
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            if (currentImage != null)
            {
                currentImage.Invalidate();
                var transform = new CompositeTransform
                {
                    ScaleX = currentImage.PixelWidth / photoContainer.ActualWidth,
                    ScaleY = currentImage.PixelHeight / photoContainer.ActualHeight,
                    Rotation = -35,
                    TranslateX = 100 * currentImage.PixelWidth / photoContainer.ActualWidth,
                    TranslateY = 250 * currentImage.PixelHeight / photoContainer.ActualHeight,
                };
                currentImage.Render(photoStamp, transform);
                currentImage.Invalidate();
                imageDetails.Text = "The picture has been stamped.";
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (currentImage != null)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = storage.CreateFile(@"customphoto.jpg"))
                    {
                        currentImage.SaveJpeg(stream, currentImage.PixelWidth, currentImage.PixelHeight, 0, 100);
                    }
                }
                imageDetails.Text = string.Format("Image saved with filename:\ncustomphoto.jpg");
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(@"customphoto.jpg"))
                {
                    using (IsolatedStorageFileStream stream =
                        storage.OpenFile(@"customphoto.jpg", FileMode.Open))
                    {
                        currentImage = PictureDecoder.DecodeJpeg(stream);
                        photoContainer.Fill = new ImageBrush { ImageSource = currentImage };
                    }
                    imageDetails.Text = string.Format("Image loaded from filename:\ncustomphoto.jpg");
                }
                else
                {
                    photoContainer.Fill = new SolidColorBrush(Colors.Gray);
                    imageDetails.Text = "Image not found!";
                }
            }
        }

        private WriteableBitmap DecodeImage(Stream imageStream, int angle)
        {
            WriteableBitmap source = PictureDecoder.DecodeJpeg(imageStream);

            switch (angle)
            {
                case 90:
                case 270:
                    return RotateBitmap(source, source.PixelHeight, source.PixelWidth, angle);
                case 180:
                    return RotateBitmap(source, source.PixelWidth, source.PixelHeight, angle);
                default:
                    return source;
            }
        }

        private int GetAngleFromExif(Stream imageStream)
        {
            var position = imageStream.Position;
            imageStream.Position = 0;
            var orientation = ExifReader.ReadJpeg(imageStream, String.Empty).Orientation;
            imageStream.Position = position;

            switch (orientation)
            {
                case ExifOrientation.TopRight:
                    return 90;
                case ExifOrientation.BottomRight:
                    return 180;
                case ExifOrientation.BottomLeft:
                    return 270;
                case ExifOrientation.TopLeft:
                case ExifOrientation.Undefined:
                default:
                    return 0;
            }
        }

        private WriteableBitmap RotateBitmap(WriteableBitmap source, int width, int height, int angle)
        {
            var target = new WriteableBitmap(width, height);
            int sourceIndex = 0;
            int targetIndex = 0;
            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    sourceIndex = x + y * source.PixelWidth;
                    switch (angle)
                    {
                        case 90:
                            targetIndex = (source.PixelHeight - y - 1) + x * target.PixelWidth;
                            break;
                        case 180:
                            targetIndex = (source.PixelWidth - x - 1) + (source.PixelHeight - y - 1) * source.PixelWidth;
                            break;
                        case 270:
                            targetIndex = y + (source.PixelWidth - x - 1) * target.PixelWidth;
                            break;
                    }
                    target.Pixels[targetIndex] = source.Pixels[sourceIndex];
                }
            }
            return target;
        }

    }
}
