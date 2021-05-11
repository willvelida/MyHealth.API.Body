namespace MyHealth.API.Body.Validators
{
    public interface IDateValidator
    {
        /// <summary>
        /// Check the provided date is in a valid format
        /// </summary>
        /// <param name="weightLogDate"></param>
        /// <returns></returns>
        bool IsWeightLogDateValid(string weightLogDate);
    }
}
