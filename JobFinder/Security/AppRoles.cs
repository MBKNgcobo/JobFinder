namespace JobFinder.Security
{
    /// <summary>
    /// Well-known role names used for role-based authorization.
    /// </summary>
    public static class AppRoles
    {
        /// <summary>
        /// Grants administrative access to shared reference data
        /// (companies and skills) as well as to any user-owned
        /// record within the application.
        /// </summary>
        public const string Admin = "Admin";
    }
}