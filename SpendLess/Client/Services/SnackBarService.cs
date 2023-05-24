using MudBlazor;

namespace SpendLess.Client.Services
{
    public class SnackBarService : ISnackBarService
    {
        readonly private ISnackbar _snackbar;

        public SnackBarService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
            _snackbar.Configuration.PreventDuplicates = false;
            _snackbar.Configuration.MaxDisplayedSnackbars = 10;
            _snackbar.Configuration.ShowCloseIcon = true;
            _snackbar.Configuration.SnackbarVariant = Variant.Outlined;
        }

        public void SuccessMsg(String msg)
        {
            //_snackbar.Add(msg, Severity.Success, config => { config.VisibleStateDuration = 1000; config.ShowTransitionDuration = 100; config.HideTransitionDuration = 500; });
        }

        public void WarningMsg(String msg)
        {
            //_snackbar.Add(msg, Severity.Warning, config => { config.VisibleStateDuration = 2000; config.ShowTransitionDuration = 100; config.HideTransitionDuration = 500; });
        }

        public void ErrorMsg(String msg)
        {
            //_snackbar.Add(msg, Severity.Error, config => { config.VisibleStateDuration = 2000; config.ShowTransitionDuration = 100; config.HideTransitionDuration = 500; });
        }
    }
}
