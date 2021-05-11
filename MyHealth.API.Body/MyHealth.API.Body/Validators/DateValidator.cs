using System;
using System.Globalization;

namespace MyHealth.API.Body.Validators
{
    public class DateValidator : IDateValidator
    {
        public bool IsWeightLogDateValid(string weightLogDate)
        {
            bool isDateValid = false;
            string pattern = "yyyy-MM-dd";
            DateTime parsedWeightLogDate;

            if (DateTime.TryParseExact(weightLogDate, pattern, null, DateTimeStyles.None, out parsedWeightLogDate))
            {
                isDateValid = true;
            }

            return isDateValid;
        }
    }
}
