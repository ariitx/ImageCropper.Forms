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

        public CropShapeType CropShape { get; set; } = CropShapeType.Rectangle;

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
            if (imageFile == null)
            {
                FileResult file = null;
                string newFile = null;

                string action = await page.DisplayActionSheet(SelectSourceTitle, CancelButtonTitle, null, TakePhotoTitle, PhotoLibraryTitle);
                try
                {
                    if (action == TakePhotoTitle)
                    {
                        /*
                        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                        {
                            await page.DisplayAlert("No Camera", ":( No camera available.", "OK");
                            Faiure?.Invoke();
                            return;
                        }
                        file = await CrossMedia.Current.TakePhotoAsync(StoreCameraMediaOptions);
                        */
                        file = await MediaPicker.CapturePhotoAsync(MediaPickerOptions);
                    }
                    else if (action == PhotoLibraryTitle)
                    {
                        /*
                        if(!CrossMedia.Current.IsPickPhotoSupported)
                        {
                            await page.DisplayAlert("Error", "This device is not supported to pick photo.", "OK");
                            Faiure?.Invoke();
                            return;
                        }
                        file = await CrossMedia.Current.PickPhotoAsync(PickMediaOptions);
                        */

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

                //imageFile = file.Path;

                imageFile = newFile;
            }

            // small delay
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            DependencyService.Get<IImageCropperWrapper>().ShowFromFile(this, imageFile);
        }
    }
}
