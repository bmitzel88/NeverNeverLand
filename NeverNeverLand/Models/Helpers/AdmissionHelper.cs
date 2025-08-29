namespace NeverNeverLand.Models.Helpers
{
    public static class AdmissionHelper
    {
        public static string GetAdmissionType(int age)
        {
            if (age < 3) return "Infant";   // could be free
            if (age <= 17) return "Child";
            return "Adult";
        }
    }
}
