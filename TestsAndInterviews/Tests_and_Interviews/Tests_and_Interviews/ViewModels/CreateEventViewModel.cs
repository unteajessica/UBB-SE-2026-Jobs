namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.Validators;

    /// <summary>
    /// ViewModel for creating events, handling validation, collaborator management, and invitation sending.
    /// </summary>
    public partial class CreateEventViewModel : ObservableObject
    {
        private const string AdminEmailAddress = "carla.draghiciu@cnglsibiu.ro";
        private const string AdminEmailPassword = "[REDACTED_PASSWORD]";
        private const string SmtpHostAddress = "smtp.gmail.com";
        private const int SmtpHostPort = 587;
        private const int SmtpTimeoutMilliseconds = 60000;
        private const string EmailSubject = "Event Invitation";
        private const string EmailSentDebugMessage = "Email sent!";
        private const string MissingEmailDebugMessage = "Company has no email";

        private const string EmptyStringValue = "";
        private const string ErrorInputsInvalid = "Please enter valid inputs before creating an event";
        private const string ErrorCompanyNameMissing = "Please enter a company name.";
        private const string ErrorCompanyNotFound = "Company was not found.";
        private const string ErrorCompanyAlreadyAdded = "Company is already added as a collaborator.";

        private readonly ICollaboratorsService collaboratorsService;
        private readonly IEventsService eventsService;
        private readonly ICompanyService companyService;
        private readonly SessionService sessionService;
        private readonly IEventValidator eventValidator;

        /// <summary>
        /// Gets the list of selected collaborators for the event.
        /// </summary>
        public List<Company> SelectedCollaborators { get; } = new();

        [ObservableProperty]
        private string photo;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string titleError;

        private bool titleIsValid = false;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string descriptionError;

        private bool descriptionIsValid = true;

        [ObservableProperty]
        private DateTimeOffset? startDate = DateTimeOffset.Now;

        [ObservableProperty]
        private string startDateError;

        private bool startDateIsValid = true;

        [ObservableProperty]
        private DateTimeOffset? endDate = DateTimeOffset.Now;

        [ObservableProperty]
        private string endDateError;

        private bool endDateIsValid = true;

        [ObservableProperty]
        private string location;

        [ObservableProperty]
        private string locationError;

        private bool locationIsValid = false;

        [ObservableProperty]
        private string addError = EmptyStringValue;

        /// <summary>
        /// Gets a value indicating whether all inputs are valid for event creation.
        /// </summary>
        public bool IsEverythingValid => this.AddError == EmptyStringValue;

        public bool EventCreatedSuccessfully = false;

        /// <summary>
        /// Create Event View Model constructor.
        /// </summary>
        /// <param name="eventsService"> events service. </param>
        /// <param name="companyService"> company service. </param>
        /// <param name="sessionService"> session service. </param>
        public CreateEventViewModel(IEventsService eventsService, ICompanyService companyService, SessionService sessionService, ICollaboratorsService collaboratorsService, IEventValidator eventValidator)
        {
            this.eventsService = eventsService;
            this.companyService = companyService;
            this.sessionService = sessionService;
            this.collaboratorsService = collaboratorsService;
            this.eventValidator = eventValidator;
        }

        /// <summary>
        /// Function that sends an email to a company.
        /// </summary>
        /// <param name="destinationCompany"> company to send email to. </param>
        private async void SendMailToCompany(Company destinationCompany)
        {
            if (string.IsNullOrEmpty(destinationCompany.Email))
            {
                System.Diagnostics.Debug.WriteLine(MissingEmailDebugMessage);
                return;
            }

            string sourceCompanyName = this.sessionService.LoggedInUser.Name;
            var fromAddress = new MailAddress(AdminEmailAddress, sourceCompanyName);

            var toAddress = new MailAddress(destinationCompany.Email, destinationCompany.Name);
            string emailBodyText = $"Hello, you have been invited to collaborate on {sourceCompanyName}'s event: {this.Title}\nPlease reply to this email within 7 days from receiving it, if you would like to accept the invitation.";

            var smtpClient = new SmtpClient
            {
                Host = SmtpHostAddress,
                Port = SmtpHostPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, AdminEmailPassword),
                Timeout = SmtpTimeoutMilliseconds,
            };

            using (var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = EmailSubject,
                Body = emailBodyText,
            })
            {
                await smtpClient.SendMailAsync(mailMessage);
            }

            System.Diagnostics.Debug.WriteLine(EmailSentDebugMessage);
        }

        /// <summary>
        /// Function that sends the invitations to all the selected companies,
        /// after the user creates the event.
        /// </summary>
        private async Task SendInvitations()
        {
            foreach (Company invitedCompany in this.SelectedCollaborators)
            {
                this.SendMailToCompany(invitedCompany);
            }
        }

        private void AddAllCollaboratorsWhenEventCreated(Event eventOfCollaboration)
        {
            foreach (Company invitedCompany in this.SelectedCollaborators)
            {
                this.collaboratorsService.AddCollaborator(eventOfCollaboration, invitedCompany, this.sessionService.LoggedInUser.CompanyId);
            }
        }

        /// <summary>
        /// Function that tries to create a new event.
        /// </summary>
        [RelayCommand]
        public async Task CreateEvent()
        {
            if (!this.titleIsValid || !this.descriptionIsValid || !this.startDateIsValid || !this.endDateIsValid || !this.locationIsValid)
            {
                this.AddError = ErrorInputsInvalid;
                return;
            }

            try
            {
                this.AddError = EmptyStringValue;
                DateTime eventStartDateTime = this.StartDate.Value.DateTime;
                DateTime eventEndDateTime = this.EndDate.Value.DateTime;

                int hostCompanyId = this.sessionService.LoggedInUser.CompanyId;
                Event createdEvent = await this.eventsService.AddEvent(this.Photo, this.Title, this.Description, eventStartDateTime, eventEndDateTime, this.Location, hostCompanyId, this.SelectedCollaborators.ToList());
                this.EventCreatedSuccessfully = true;

                this.AddAllCollaboratorsWhenEventCreated(createdEvent);
                await this.SendInvitations();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                this.EventCreatedSuccessfully = false;
            }
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event title is valid.
        /// </summary>
        /// <returns> true if the title is valid, false otherwise. </returns>
        public bool ValidateTitle()
        {
            try
            {
                if (this.eventValidator.ValidateEventTitle(this.Title))
                {
                    this.TitleError = EmptyStringValue;
                    this.titleIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.TitleError = exception.Message;
                this.titleIsValid = false;
            }

            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event description is valid.
        /// </summary>
        /// <returns> true if the description is valid, false otherwise. </returns>
        public bool ValidateDescription()
        {
            try
            {
                if (this.eventValidator.ValidateEventDescription(this.Description))
                {
                    this.DescriptionError = EmptyStringValue;
                    this.descriptionIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.DescriptionError = exception.Message;
                this.descriptionIsValid = false;
            }

            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event location is valid.
        /// </summary>
        /// <returns> true if the location is valid, false otherwise. </returns>
        public bool ValidateLocation()
        {
            try
            {
                if (this.eventValidator.ValidateEventLocation(this.Location))
                {
                    this.LocationError = EmptyStringValue;
                    this.locationIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.LocationError = exception.Message;
                this.locationIsValid = false;
            }

            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event starting date is valid.
        /// </summary>
        /// <returns> true if the starting date is valid, false otherwise. </returns>
        public bool ValidateStartDate()
        {
            try
            {
                if (this.eventValidator.ValidateEventStartDate(this.StartDate))
                {
                    this.StartDateError = EmptyStringValue;
                    this.startDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.StartDateError = exception.Message;
                this.startDateIsValid = false;
            }

            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event ending date is valid.
        /// </summary>
        /// <returns> true if the ending date is valid, false otherwise. </returns>
        public bool ValidateEndDate()
        {
            try
            {
                if (this.eventValidator.ValidateEventEndDate(this.EndDate))
                {
                    this.EndDateError = EmptyStringValue;
                    this.endDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.EndDateError = exception.Message;
                this.endDateIsValid = false;
            }

            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event dates are cronologically valid.
        /// </summary>
        /// <returns> true if the dates are valid, false otherwise. </returns>
        public bool ValidateDatesCronologity()
        {
            try
            {
                if (this.eventValidator.ValidateEventDatesChronologically(this.StartDate, this.EndDate))
                {
                    this.EndDateError = EmptyStringValue;
                    this.endDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.EndDateError = exception.Message;
                this.endDateIsValid = false;
            }

            return false;
        }

        /// <summary>
        /// Function that tries to add a collaborator to the event.
        /// </summary>
        /// <param name="companyName"> the invited company's name. </param>
        /// <returns> tuple with success boolean and error message. </returns>
        public async Task<(bool success, string errorMessage)> TryAddCollaboratorByName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return (false, ErrorCompanyNameMissing);
            }

            Company? companyToInvite = await this.companyService.GetCompanyByName(companyName);
            if (companyToInvite == null)
            {
                return (false, ErrorCompanyNotFound);
            }

            if (this.SelectedCollaborators.Any(collaborator => string.Equals(collaborator.Name, companyToInvite.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, ErrorCompanyAlreadyAdded);
            }

            this.SelectedCollaborators.Add(companyToInvite);
            return (true, EmptyStringValue);
        }

        /// <summary>
        /// Function that removes a collaborator.
        /// </summary>
        /// <param name="companyName"> the name of the company to be removed from the collaborators list. </param>
        public void RemoveCollaboratorByName(string companyName)
        {
            foreach (Company selectedCompany in this.SelectedCollaborators.ToList())
            {
                if (selectedCompany.Name == companyName)
                {
                    this.SelectedCollaborators.Remove(selectedCompany);
                }
            }
        }
    }
}