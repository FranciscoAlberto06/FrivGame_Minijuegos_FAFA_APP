using BGestionFAFA;

namespace FrivGame_Minijuegos_FAFA_APP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        //protected async override void OnSleep()
        //{
        

        //    base.OnSleep();

        //    // Comprobamos el estado de la red
        //    NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;

        //    // Si tenemos conexion a internet actualizamos datos nuevo que hubiera en la nube
        //    if (accesoRed == NetworkAccess.Internet)
        //    {

        //        await ApiAivenFAFA.SubirDatosCompletosHaciaAiven();

        //    }
        //}
    }
}