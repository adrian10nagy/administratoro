
namespace Admin.Helpers.Constants
{
    using System.ComponentModel;

    public enum ErrorMessages
    {
        BadRequest = 0,

        [Description("User inexistent, nu poți edita acest user!")]
        UserInvalid = 1,

        [Description("Book inexistent, nu poți edita acest user!")]
        BookInvalid = 2,
    }
}