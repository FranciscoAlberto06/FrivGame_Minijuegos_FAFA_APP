

using BGestionFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PageInicioSesion : ContentPage
{
    public PageInicioSesion()
	{
		InitializeComponent();
		AnimacionInicio();
        CargarBD();
	}

    private async void CargarBD()
    {
   
        // Cargamos siempre los datos locales del dispositivo
        #region CARGAR DATOS LOCALES
        // Cargamos todos los datos locales que tenga ya el usuario de sqlite
        // Cargamos la rutas y creamos si es necesario los archivos sqlite si es nuevo dispositivo
        ApiSQLiteFAFA.ComprobarRutasSQL(FileSystem.AppDataDirectory);
        #endregion

        // TODO: Para cuando hagamosla parte de la nube
        //Comprobamos el estado de la red
        NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;

        // Si tenemos conexion a internet actualizamos datos nuevo que hubiera en la nube
        if (accesoRed == NetworkAccess.Internet)
        {
            #region CARGAR DATOS DE LA NUBE
            await ApiAivenFAFA.CargarDatosNuevosDesdeAiven(FileSystem.AppDataDirectory);
            #endregion
        }

    }

    private async void AnimacionInicio()
    {
        // 1. ESTADO INICIAL
        LoginContainer.Opacity = 0;
        LoginContainer.IsVisible = false;

        LogoImage.Scale = 0.5; // Empezamos un poco mas grande para que no sea desde la nada
        LogoImage.Opacity = 0;

        // 2. ESPERAR UN SEGUNDO
        await Task.Delay(1000);

        // 3. APARECER Y REBOTAR
        // Primero le damos opacidad para que se ve
        await LogoImage.FadeTo(1, 200);

        // Despues lo agrandomos un poco más de lo normal 
        await LogoImage.ScaleTo(1.9, 400);

        // Y despues lo volvemos a su tamaño normal para dar un efecto de rebote
        await LogoImage.ScaleTo(1.0, 400);

        // 4. SE PARA OTRO SEGUNDO
        await Task.Delay(1000);

        // 5. DESAPARECER LENTAMENTE
        await LogoImage.FadeTo(0, 600);
        await SplashContainer.FadeTo(0, 400);

        SplashContainer.IsVisible = false;

        // 6. MOSTRAR EL LOGIN
        LoginContainer.IsVisible = true;
        await LoginContainer.FadeTo(1, 800);
    }

    private async void botonInicio(object sender, EventArgs e)
    {
        bool errorEncontrado = false;
        int idUsuario = -1;
        string passwordOculta;
        try
        {
            // 1. Validar campos vacíos antes de ir a la DB
            if (string.IsNullOrEmpty(eEmail.Text) || string.IsNullOrEmpty(ePassword.Text))
                throw new Exception("Introduce todos los datos");

            // 2. Comprobar que la cuenta exista y la contraseña sea correcto
            // 2.1. Sacamos el id del usuario con ese correo para la sacar la contraseña oculta
            idUsuario = ApiSQLiteFAFA.ExtraerIdUsuarioPorEmail(eEmail.Text);

            // 2.2 Comprobamos que haya devuelto un id 
            if (idUsuario == null)
            {
                throw new Exception("El correo electrónico no está registrado.");
            }

            // 2.3. Una vez extraido obtenemos la contraseña real de ese usuario
            passwordOculta =  await ApiSQLiteFAFA.ObtenerPasswordOculta(idUsuario);

            // 2.4. Comprobamos que las contrasena coincidan
            if(ePassword.Text != passwordOculta)
            {
                ePassword.Text = "";
                throw new Exception("La contraseña no coincide");

            }

       

        }
        catch(Exception error)
        {
            lError.IsVisible = true;
            errorEncontrado = true;
            lError.Text = error.Message;
            lError.TextColor = Colors.Red;
        }
        finally
        {
            if (!errorEncontrado)
            {
                // Limpiamos entrys
                eEmail.Text = "";
                ePassword.Text = "";

                // Mostramos un mensaje de estado
                lError.IsVisible = true;

                // Mostramos mensaje de que todo salio bien
                lError.Text = "Todo Correcto. Iniciando.....";
                lError.TextColor = Colors.Green;

                // Esperamos 2 s y navegamos
                await Task.Delay(2000);
                // Ocultamos mensaje de error por si volvemos
                lError.IsVisible = false;
                await Navigation.PushAsync(new MenuJuegos(idUsuario));

            }

        }

    }

    private async void IrPaginaRegistro(object sender, TappedEventArgs e)
    {
        // Comprobamos el estado de la red
        NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;

        // Si tenemos conexion a internet a internet entramos a las zona de registro
        if (accesoRed == NetworkAccess.Internet)
        {
            await Navigation.PushAsync(new PaginaRegistro());

        }
        else
        {
            await DisplayAlert("UPSSS!!!", "Parece que no tiene acceso a Internet. Lo sentimos mucho pero no se puede realizar registros sin conexion. Disculpe las molestias :D", "Aceptar");
        }

    }

 
}