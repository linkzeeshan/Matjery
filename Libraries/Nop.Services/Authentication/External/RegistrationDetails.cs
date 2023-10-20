//Contributor:  Nicholas Mayne


namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Registration details
    /// </summary>
    public struct RegistrationDetails
    {
        public RegistrationDetails(OpenAuthenticationParameters parameters)
            : this()
        {
            if (parameters.UserClaims != null)
                foreach (var claim in parameters.UserClaims)
                {
                    //email, username
                    if (string.IsNullOrEmpty(EmailAddress))
                    {
                        if (claim.Contact != null)
                        {
                            EmailAddress = claim.Contact.Email;
                            UserName = claim.Contact.Email;
                        }
                    }

                    //gender
                    if (string.IsNullOrEmpty(Gender))
                    {
                        if (claim.Person != null)
                        {
                            if (!string.IsNullOrEmpty(claim.Person.Gender))
                            {
                                Gender = claim.Person.Gender;
                            }
                        }
                    }
                    //nationality
                    if (string.IsNullOrEmpty(Nationality))
                    {
                        if (claim.Contact != null)
                        {
                            if (claim.Contact.Address != null)
                            {
                                if (!string.IsNullOrEmpty(claim.Contact.Address.Country))
                                {
                                    Nationality = claim.Contact.Address.Country;
                                }
                            }
                            if (claim.Contact.Phone != null)
                            {
                                if (!string.IsNullOrEmpty(claim.Contact.Phone.Mobile))
                                {
                                    PhoneNumber = claim.Contact.Phone.Mobile;
                                }
                            }
                        }
                    }
                    //DOB
                    if (string.IsNullOrEmpty(DateOfBirth))
                    {
                        if (claim.BirthDate != null)
                        {
                            if (claim.BirthDate.GeneratedBirthDate != null)
                            {
                                    DateOfBirth = claim.BirthDate.GeneratedBirthDate.ToString();
                            }
                        }
                    }
                    //first name
                    if (string.IsNullOrEmpty(FirstName))
                        if (claim.Name != null)
                            FirstName = claim.Name.First;
                    //last name
                    if (string.IsNullOrEmpty(LastName))
                        if (claim.Name != null)
                            LastName = claim.Name.Last;

                 
                }
        }

        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string RegistrationType { get; set; }
    }
}