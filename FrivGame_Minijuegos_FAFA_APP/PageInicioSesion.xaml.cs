

using BGestionFAFA;
using BModelosFAFA;
using BModelosSQLFAFA;
using Plugin.Maui.Audio;

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
   
   
        // Cargamos la rutas y creamos si es necesario los archivos sqlite si es nuevo dispositivo
        ApiSQLiteFAFA.ComprobarRutasSQL(FileSystem.AppDataDirectory);

        // TODO: Para cuando hagamosla parte de la nube
        //Comprobamos el estado de la red
        NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;

        // Si tenemos conexion a internet actualizamos datos nuevo que hubiera en la nube
        if (accesoRed == NetworkAccess.Internet)
        {
            #region CARGAR DATOS DE LA NUBE
            await ApiRestFAFA.CargarDatosDesdeApi(FileSystem.AppDataDirectory);
            await ApiRestFAFA.SincronizarHaciaApi("Todo");
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
        // Preparamos nuestra musica a poner
        // Hemos usado el nudget llamado  Plugin.Maui.Audio
        IAudioPlayer player = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("boing_musica.mp3"));
 

        // 3. APARECER Y REBOTAR

        // Primero le damos opacidad para que se ve
        await LogoImage.FadeTo(1, 200);
        player.Play();
        // Despues lo agrandomos un poco más de lo normal 
        await LogoImage.ScaleTo(1.9, 400);

        // Y despues lo volvemos a su tamaño normal para dar un efecto de rebote
        await LogoImage.ScaleTo(1.0, 400);

        // 4. SE PARA OTRO SEGUNDO
        player.Stop();
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
        string passwordParaValidar = ""; // Aquí guardaremos o la real (local) o el Hash (nube)        
        try
        {
            // 1. Validar campos vacíos antes de ir a la DB
            if (string.IsNullOrEmpty(eEmail.Text) || string.IsNullOrEmpty(ePassword.Text))
                throw new Exception("Introduce todos los datos");

            // Si hay internet extraemos desde Aiven sino desde sqlite
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                // 1. Extraemos el HASH y el ID desde Aiven
                Usuario datosNube = await ApiRestFAFA.ExtraerDatosLoginPorEmail(eEmail.Text);

                if (datosNube == null) throw new Exception("Correo no registrado en la nube");

                idUsuario = datosNube.IdUsuario;
                string hashEnAiven = datosNube.Password;

                // 2. Convertimos lo que el usuario escribió a HASH para comparar 
                string hashDeMiEntrada = ApiRestFAFA.GenerarHash(ePassword.Text);

                if (hashDeMiEntrada != hashEnAiven)
                {
                    throw new Exception("La contraseña no coincide");
                }

                // 3. Como acertó y hay internet, comprobamos que este en el SecureStorage
                bool existaYa = await ApiSQLiteFAFA.ComprobarExisteEnSecureStorage(idUsuario, ePassword.Text);

                // 4. Sino existe la guardamos, para que pueda iniciar cuando no tenga internet
                if (!existaYa)
                {
                    ApiSQLiteFAFA.GuardarContrasenaOculta(idUsuario, ePassword.Text);
                }
            }
            else
            {
                // 1. Buscamos el ID asociado al email en el móvil
                idUsuario = ApiSQLiteFAFA.ExtraerIdUsuarioPorEmail(eEmail.Text);

                if (idUsuario == -1)
                    throw new Exception("Este usuario no existe en este dispositivo. Conéctate una vez a internet.");

                // 2. Intentamos recuperar la "llave de repuesto"
                passwordParaValidar = await ApiSQLiteFAFA.ObtenerPasswordOculta(idUsuario);

                // 3. Verificamos que realmente tengamos algo guardado
                if (string.IsNullOrEmpty(passwordParaValidar))
                {
                    throw new Exception("No hay datos de acceso local. Inicia sesión con internet primero.");
                }

                if (ePassword.Text != passwordParaValidar)
                {
                    throw new Exception("Contraseña incorrecta.");
                }
            }



        }
        catch(Exception error)
        {
            lError.IsVisible = true;
            errorEncontrado = true;
            lError.Text = error.Message;
            lError.TextColor = Colors.Red;
            ePassword.Text = "";
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