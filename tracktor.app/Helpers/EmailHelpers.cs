namespace tracktor.app
{
    public static class EmailHelpers
    {
        public static bool Validate(string e)
        {
            return new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(e);
        }
    }
}
