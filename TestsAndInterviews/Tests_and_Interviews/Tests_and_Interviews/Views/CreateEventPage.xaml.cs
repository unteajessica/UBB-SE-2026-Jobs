using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Validators;
using Tests_and_Interviews.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Tests_and_Interviews.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateEventPage : Page
    {
        public CreateEventViewModel CreateEventViewModel { get; }
        private bool pageIsLoaded = false;
        private bool isStartDateModified = false;
        private bool isEndDateModified = false;

        /// <summary>
        /// Create event page constructor that initializes its view model
        /// </summary>
        public CreateEventPage()
        {
            var mainWindow = App.MainWindow;

            CreateEventViewModel = new CreateEventViewModel(mainWindow.EventsService, mainWindow.CompanyService, mainWindow.SessionService, mainWindow.CollabsService, mainWindow.EventValidator);
            this.DataContext = CreateEventViewModel;

            InitializeComponent();
            pageIsLoaded = true;
        }

        /// <summary>
        /// Function that calls the "AddCollaborator" function and displays the error text, if one exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var companyName = CollaboratorNameBox.Text?.Trim() ?? string.Empty;
            var (success, errorMessage) = await CreateEventViewModel.TryAddCollaboratorByName(companyName);
            if (success)
            {
                CollaboratorNameBox.Text = string.Empty;
                CollaboratorErrorTextBlock.Text = string.Empty;
                RenderCollaboratorTags();
            }
            else
            {
                CollaboratorErrorTextBlock.Text = errorMessage;
            }
        }

        /// <summary>
        /// Function that adds a "Collaborator" tag and "Remove" button if a collaborator was added to an event
        /// </summary>
        private void RenderCollaboratorTags()
        {
            CollaboratorsPanel.Children.Clear();

            foreach (var collaborator in CreateEventViewModel.SelectedCollaborators)
            {
                var removeButton = new Button
                {
                    Content = "x",
                    Tag = collaborator.Name,
                    Padding = new Thickness(6, 0, 6, 0),
                    MinWidth = 28
                };
                removeButton.Click += RemoveCollaborator_Click;

                var chip = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Background = new SolidColorBrush(Colors.LightGray),
                    Padding = new Thickness(8, 4, 8, 4)
                };

                chip.Children.Add(new TextBlock { Text = collaborator.Name, VerticalAlignment = VerticalAlignment.Center });
                chip.Children.Add(removeButton);

                CollaboratorsPanel.Children.Add(chip);
            }
        }

        /// <summary>
        /// Function that calls the "RemoveCollaborator" function and displays the error message, if one exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveCollaborator_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string collaboratorName)
            {
                CreateEventViewModel.RemoveCollaboratorByName(collaboratorName);
                CollaboratorErrorTextBlock.Text = string.Empty;
                RenderCollaboratorTags();
            }
        }

        /// <summary>
        /// Function that displays a ComtentDialog if the user tries to press the "Cancel"
        /// button. The ContentDialog shows 2 buttons: Yes and No. If the chosen button
        /// was "Yes", the user is taken back to the "Our Events" page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CancelChanges_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog cancelConfirmationDialog = new ContentDialog
            {
                Title = "Confirm cancel",
                Content = "Are you sure you want to cancel the modifications?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var chosenButton = await cancelConfirmationDialog.ShowAsync();

            if (chosenButton == ContentDialogResult.Primary)
            {
                NavigateBack_Click(sender, e);
            }
        }

        /// <summary>
        /// Function that takes the user back to the "Our events" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow;
            mainWindow.RootFrame.Navigate(typeof(OurEventsPage));
        }

        /// <summary>
        /// Function that displays an appropriate ContentDialog, based on the success/
        /// failure of creating a new event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            // because click usually runs before command so we must make it run before
            await CreateEventViewModel.CreateEvent();

            if (CreateEventViewModel.IsEverythingValid)
            {
                NavigateBack_Click(sender, e);
            }
            else
            {
                return;
            }

            ContentDialog popup;
            if (CreateEventViewModel.EventCreatedSuccessfully)
            {
                popup = new ContentDialog
                {
                    Title = "YEY!",
                    Content = "Event created successfully!",
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
            }
            else
            {
                popup = new ContentDialog
                {
                    Title = "Oops!",
                    Content = "We're sorry, an error occurred. The event was not created. Please try again.",
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
            }

            await popup.ShowAsync();
        }

        /// <summary>
        /// Function that allows the user to attach an image and displays the
        /// photo on the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AttachImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            // Common image formats
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");

            // Required for WinUI 3 pickers
            IntPtr hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            PhotoFileNameTextBlock.Text = file.Name;

            // Read file bytes and convert to base64 (stored in ViewModel.Photo)
            byte[] bytes;
            using (var input = await file.OpenReadAsync())
            using (var reader = new DataReader(input.GetInputStreamAt(0)))
            {
                await reader.LoadAsync((uint)input.Size);
                bytes = new byte[input.Size];
                reader.ReadBytes(bytes);
            }

            CreateEventViewModel.Photo = Convert.ToBase64String(bytes);

            // Create preview image from the selected bytes
            var bitmapImage = new BitmapImage();
            using (var memStream = new InMemoryRandomAccessStream())
            {
                await memStream.WriteAsync(bytes.AsBuffer());
                memStream.Seek(0);
                bitmapImage.SetSource(memStream);
            }
            PhotoPreviewImage.Source = bitmapImage;
        }

        /// <summary>
        /// Function that controls the border colour of the title textbox based on
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Title_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = TitleBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (CreateEventViewModel.ValidateTitle())
            {
                TitleBox.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                TitleBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Function that controls the border colour of the description textbox based on
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Description_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = DescriptionBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (CreateEventViewModel.ValidateDescription())
            {
                DescriptionBox.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                DescriptionBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Function that controls the border colour of the location textbox based on
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Location_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = LocationBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (CreateEventViewModel.ValidateLocation())
            {
                LocationBox.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                LocationBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Function that controls the border colour of the start date picker based on
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!pageIsLoaded)
            {
                return;
            }
            if (!isStartDateModified)
            {
                isStartDateModified = true;
                return;
            }

            if (CreateEventViewModel.ValidateStartDate())
            {
                StartDatePicker.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StartDatePicker.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Function that controls the border colour of the end date picker based on
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!pageIsLoaded)
            {
                return;
            }
            if (!isEndDateModified)
            {
                isEndDateModified = true;
                return;
            }

            if (CreateEventViewModel.ValidateEndDate())
            {
                EndDatePicker.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                EndDatePicker.BorderBrush = new SolidColorBrush(Colors.Red);
            }

            if (isStartDateModified)
            {
                if (CreateEventViewModel.ValidateDatesCronologity())
                {
                    EndDatePicker.BorderBrush = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    EndDatePicker.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }
    }
}
