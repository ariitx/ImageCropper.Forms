using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Stormlion.ImageCropper
{
    public class ImageCropper
    {
        public static ImageCropper Current { get; set; }

        public ImageCropper()
        {
            Current = this;
        }

        public enum CropShapeType
        {
            Rectangle,
            Oval
        };

        public enum ResultErrorType
        {
            None,
            CaptureNotSupported,
        };

        public CropShapeType CropShape { get; set; } = CropShapeType.Rectangle;

        public ResultErrorType ResultError { get; set; }

        public int AspectRatioX { get; set; } = 0;

        public int AspectRatioY { get; set; } = 0;

        public string PageTitle { get; set; } = null;

        public string SelectSourceTitle { get; set; } = "Select source";

        public string TakePhotoTitle { get; set; } = "Take Photo";

        public string PhotoLibraryTitle { get; set; } = "Photo Library";

        public string CancelButtonTitle { get; set; } = "Cancel";

        public Action<string> Success { get; set; }

        public Action Faiure { get; set; }

        /*
        public PickMediaOptions PickMediaOptions { get; set; } = new PickMediaOptions
        {
            PhotoSize = PhotoSize.Large,
        };

        public StoreCameraMediaOptions StoreCameraMediaOptions { get; set; } = new StoreCameraMediaOptions();
        */

        public MediaPickerOptions MediaPickerOptions { get; set; } = new MediaPickerOptions();


        public async void Show(Page page, string imageFile = null)
        {
            ResultError = ResultErrorType.None;
            if (imageFile == null)
            {
                FileResult file = null;
                string newFile = null;

                string action = await page.DisplayActionSheet(SelectSourceTitle, CancelButtonTitle, null, TakePhotoTitle, PhotoLibraryTitle);
                try
                {
                    if (action == TakePhotoTitle)
                    {
                        if (MediaPicker.IsCaptureSupported)
                        {
                            file = await MediaPicker.CapturePhotoAsync(MediaPickerOptions);
                        }
                        else {
                            //No soporta la captura desde la camara
                            ResultError = ResultErrorType.CaptureNotSupported;
                        }
                    }
                    else if (action == PhotoLibraryTitle)
                    {
                        file = await MediaPicker.PickPhotoAsync(MediaPickerOptions);
                    }
                    else
                    {
                        Faiure?.Invoke();
                        return;
                    }

                    //Si se capturo correctamente
                    if (file != null)
                    {
                        // save the file into local storage
                        newFile = Path.Combine(FileSystem.CacheDirectory, file.FileName);
                        using (var stream = await file.OpenReadAsync())
                        using (var newStream = File.OpenWrite(newFile))
                            await stream.CopyToAsync(newStream);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
                }

                if (file == null || newFile == null)
                {
                    Faiure?.Invoke();
                    return;
                }

                imageFile = newFile;
            }

            // small delay
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            DependencyService.Get<IImageCropperWrapper>().ShowFromFile(this, imageFile);
        }
    }
}
