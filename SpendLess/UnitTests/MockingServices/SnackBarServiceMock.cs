using MudBlazor;
using SpendLess.Client.Services;

namespace Tests.MockingServices
{
    public class SnackBarServiceMock : ISnackBarService
    {
        readonly private ISnackbar _snackbar;

        public SnackBarServiceMock(ISnackbar snackbar)
        {
            _snackbar = snackbar;
            _snackbar.Configuration.PreventDuplicates = false;
            _snackbar.Configuration.MaxDisplayedSnackbars = 10;
            _snackbar.Configuration.ShowCloseIcon = true;
            _snackbar.Configuration.SnackbarVariant = Variant.Outlined;
        }

        public void SuccessMsg(String msg)
        {
            return;
        }

        public void WarningMsg(String msg)
        {
            return;
        }

        public void ErrorMsg(String msg)
        {
            return;
        }
    }
}
