namespace Tests_and_Interviews.Views
{
	using System;
	using System.Collections.Generic;
	using Microsoft.UI.Xaml;
	using Microsoft.UI.Xaml.Controls;
	using Tests_and_Interviews.Dtos;
	using Tests_and_Interviews.Models.Enums;
	using Tests_and_Interviews.Repositories;
	using Tests_and_Interviews.Services;
	using Tests_and_Interviews.ViewModels;

	/// <summary>
	/// Represents the recruiter page for managing interview slots, pending reviews, and navigation within the application.
	/// </summary>
	public sealed partial class RecruiterPage : Page
	{
		private const int MIN_TIME_SLOT_DURATION = 60;
		private const int MAX_TIME_SLOT_DURATION = 90;

		/// <summary>
		/// Initializes a new instance of the <see cref="RecruiterPage"/> class.
		/// </summary>
		public RecruiterPage()
		{
			this.InitializeComponent();

			var slotRepository = new SlotRepository();
			var slotService = new SlotService();
			this.DataContext = new RecruiterViewModel(slotService);
		}

		private RecruiterViewModel ViewModel => (RecruiterViewModel)this.DataContext;

		private void MainCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
		{
			if (sender.SelectedDates.Count > 0)
			{
				this.ViewModel.SelectedDate = sender.SelectedDates[0].DateTime;
			}
		}

		private async void Slot_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (sender is Grid grid && grid.DataContext is SlotDto slot)
			{
				try
				{
					if (slot.Status != SlotStatus.Free)
					{
						return;
					}

					if (slot.InterviewType == "Available")
					{
						var manageDialog = new ContentDialog
						{
							Title = "Manage Slot",
							Content = "Do you want to edit or delete slot between " + slot.TimeRange + " ?",
							PrimaryButtonText = "Edit",
							SecondaryButtonText = "Delete",
							CloseButtonText = "Cancel",
							XamlRoot = this.XamlRoot,
						};

						ContentDialogResult manageSlotResult = await manageDialog.ShowAsync();

						if (manageSlotResult == ContentDialogResult.Primary)
						{
							// edit slot
							var editContent = new StackPanel();

							var newStartTimePicker = new TimePicker
							{
								Header = "Start time",
								SelectedTime = new TimeSpan(slot.StartTime.Hour, slot.StartTime.Minute, 0),
								MinuteIncrement = 30,
							};

							var newInterviewTimeComboBox = new ComboBox
							{
								Header = "Duration",
								ItemsSource = new List<string> { MIN_TIME_SLOT_DURATION + " min", MAX_TIME_SLOT_DURATION + " min" },
								SelectedIndex = 0,
							};

							editContent.Children.Add(newStartTimePicker);
							editContent.Children.Add(newInterviewTimeComboBox);

							var editDialog = new ContentDialog
							{
								Title = "Edit Slot",
								Content = editContent,
								PrimaryButtonText = "Edit",
								CloseButtonText = "Cancel",
								XamlRoot = this.XamlRoot,
							};

							if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
							{
								TimeSpan newStartTime = newStartTimePicker.SelectedTime ?? slot.StartTime.TimeOfDay;
								int newDuration = newInterviewTimeComboBox.SelectedIndex == 0 ? MIN_TIME_SLOT_DURATION : MAX_TIME_SLOT_DURATION;

								await this.ViewModel.UpdateSlotAsync(slot, newStartTime, newDuration);
							}
						}
						else if (manageSlotResult == ContentDialogResult.Secondary)
						{
							// delete slot
							await this.ViewModel.DeleteSlotAsync(slot.Id);
						}

						return;
					}

					var interviewTimeComboBox = new ComboBox
					{
						Header = "Duration",
						ItemsSource = new List<string> { MIN_TIME_SLOT_DURATION + " min", MAX_TIME_SLOT_DURATION + " min" },
						SelectedIndex = 0,
					};

					var createDialog = new ContentDialog
					{
						Title = "Create Slot",
						Content = interviewTimeComboBox,
						PrimaryButtonText = "Create",
						CloseButtonText = "Cancel",
						XamlRoot = this.XamlRoot,
					};

					if (await createDialog.ShowAsync() == ContentDialogResult.Primary)
					{
						int duration = interviewTimeComboBox.SelectedIndex == 0 ? MIN_TIME_SLOT_DURATION : MAX_TIME_SLOT_DURATION;

						await this.ViewModel.CreateSlotAsync(slot, duration);
					}
				} catch (Exception ex)
				{
					var errorDialog = new ContentDialog
					{
						Content = ex.Message,
						CloseButtonText = "Cancel",
						XamlRoot = this.XamlRoot,
					};

					await errorDialog.ShowAsync();
				}
			}
		}

		private void LeaderboardInfo_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(RecruiterTestsPage));
		}

		private void RefreshPendingReviews_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.LoadPendingReviews();
		}

		private void ReviewPending_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button b)
			{
				var tag = b.Tag;
				int sessionId = 0;
				if (tag is int i)
				{
					sessionId = i;
				} else if (tag is string s && int.TryParse(s, out int parsed))
				{
					sessionId = parsed;
				}

				if (sessionId > 0)
				{
					this.Frame.Navigate(typeof(InterviewInterviewerPage), sessionId);
				}
			}
		}

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.ReturnToMainMenu();
        }
    }
}